using MemoryPack;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace HNSW;

public class HNSWIndex
{
    // Delegates are not serializable and should be set after deserialization
    private Func<HNSWPoint, HNSWPoint, float> distanceFnc;

    private readonly HNSWParameters parameters;

    private readonly GraphData data;

    private readonly GraphConnector connector;

    private readonly GraphNavigator navigator;

    /// <summary>
    /// Construct KNN search graph with arbitrary distance function
    /// </summary>
    public HNSWIndex(Func<HNSWPoint, HNSWPoint, float> distFnc, HNSWParameters? hnswParameters = null)
    {
        hnswParameters ??= new HNSWParameters();
        distanceFnc = distFnc;
        parameters = hnswParameters;

        data = new GraphData(distFnc, hnswParameters);
        navigator = new GraphNavigator(data);
        connector = new GraphConnector(data, navigator, hnswParameters);

        data.Reallocated += OnDataResized;
    }

    /// <summary>
    /// Construct KNN search graph from serialized snapshot.
    /// </summary>
    internal HNSWIndex(Func<HNSWPoint, HNSWPoint, float> distFnc, HNSWIndexSnapshot snapshot)
    {
        if (snapshot.Parameters is null)
            throw new ArgumentNullException(nameof(snapshot.Parameters), "Parameters cannot be null during deserialization.");

        if (snapshot.DataSnapshot is null)
            throw new ArgumentNullException(nameof(snapshot.DataSnapshot), "Data cannot be null during deserialization.");

        distanceFnc = distFnc;
        parameters = snapshot.Parameters;
        data = new GraphData(snapshot.DataSnapshot, distFnc, snapshot.Parameters);

        navigator = new GraphNavigator(data);
        connector = new GraphConnector(data, navigator, parameters);

        data.Reallocated += OnDataResized;
    }

    /// <summary>
    /// Add new item with given label to the graph.
    /// </summary>
    public int Add(HNSWPoint item)
    {
        var itemId = -1;
        lock (data.indexLock)
        {
            itemId = data.AddItem(item);
        }

        lock (data.Nodes[itemId].OutEdgesLock)
        {
            connector.ConnectNewNode(itemId);
        }
        return itemId;
    }

    /// <summary>
    /// Add collection of items to the graph
    /// </summary>
    public int[] Add(List<HNSWPoint> items)
    {
        var idArray = new int[items.Count];
        Parallel.For(0, items.Count, (i) =>
        {
            idArray[i] = Add(items[i]);
        });
        return idArray;
    }

    /// <summary>
    /// Remove item with given index from graph structure
    /// </summary>
    public void Remove(int itemIndex)
    {
        var item = data.Nodes[itemIndex];
        for (int layer = item.MaxLayer; layer >= 0; layer--)
        {
            data.LockNodeNeighbourhood(item, layer);
            connector.RemoveConnectionsAtLayer(item, layer);
            if (layer == 0) data.RemoveItem(itemIndex);
            data.UnlockNodeNeighbourhood(item, layer);
        }
    }

    /// <summary>
    /// Remove collection of items associated with indexes
    /// </summary>
    public void Remove(List<int> indexes)
    {
        Parallel.For(0, indexes.Count, (i) =>
        {
            Remove(indexes[i]);
        });
    }

    public int Count
    {
        get { return data.Items.Count; }
    }

    /// <summary>
    /// Get list of items inserted into the graph structure
    /// </summary>
    public List<HNSWPoint> Items()
    {
        return data.Items.Values.ToList();
    }

    /// <summary>
    /// Directly access graph structure at given layer
    /// </summary>
    public GraphLayer GetGraphLayer(int layer)
    {
        return new GraphLayer(data.Nodes, layer);
    }

    /// <summary>
    /// Get K nearest neighbours of query point. 
    /// Optionally probide filter function to ignore certain labels.
    /// Layer parameters indicates at which layer search should be performed (0 - base layer)
    /// </summary>
    public List<KNNResult> KnnQuery(HNSWPoint query, int k, Func<HNSWPoint, bool>? filterFnc = null, int layer = 0)
    {
        if (data.Nodes.Count - data.RemovedIndexes.Count <= 0) return new List<KNNResult>();

        Func<int, bool> indexFilter = _ => true;
        if (filterFnc is not null)
            indexFilter = (index) => filterFnc(data.Items[index]);


        float queryDistance(int nodeId, HNSWPoint label)
        {
            return distanceFnc(data.Items[nodeId], label);
        }

        var neighboursAmount = Math.Max(parameters.MinNN, k);
        var distCalculator = new DistanceCalculator<HNSWPoint>(queryDistance, query);
        var ep = navigator.FindEntryPoint(layer, distCalculator);
        var topCandidates = navigator.SearchLayer(ep.Id, layer, neighboursAmount, distCalculator, indexFilter);

        if (k < neighboursAmount)
        {
            return topCandidates.OrderBy(c => c.Dist).Take(k).ToList().ConvertAll(c => new KNNResult(c.Id, data.Items[c.Id], c.Dist));
        }
        return topCandidates.ConvertAll(c => new KNNResult(c.Id, data.Items[c.Id], c.Dist));
    }

    /// <summary>
    /// Get statistical information about graph structure
    /// </summary>
    public HNSWInfo GetInfo()
    {
        return new HNSWInfo(data.Nodes, data.RemovedIndexes, data.GetTopLayer());
    }

    /// <summary>
    /// Serialize the graph snapshot image to a file.
    /// </summary>
    public void Serialize(string filePath, int sliceMaxCount = 500000)
    {
        var indexData = Serialize(sliceMaxCount);
        File.WriteAllBytes(filePath, indexData.Body);
        int num = 0;
        foreach(var item in indexData.ItemSlices)
        {
            File.WriteAllBytes($"{filePath}.{num}.items", item);
            num++;
        }
        num = 0;
        foreach (var item in indexData.NodeSlices)
        {
            File.WriteAllBytes($"{filePath}.{num}.nodes", item);
            num++;
        }
    }

    public HNSWIndexData Serialize(int sliceMaxCount = 500000)
    {
        var indexData = new HNSWIndexData();

        var snapshot = new HNSWIndexSnapshot(parameters, data);
        if(snapshot.NeedSlice(sliceMaxCount))
        {
            var slices = snapshot.Slice(sliceMaxCount);
            foreach(var item in slices.Item1)
            {
                indexData.ItemSlices.Add(MemoryPackSerializer.Serialize(item));
            }
            foreach(var item in slices.Item2)
            {
                indexData.NodeSlices.Add(MemoryPackSerializer.Serialize(item));
            }
        }
        indexData.Body = MemoryPackSerializer.Serialize(snapshot);
        return indexData;
    }

    /// <summary>
    /// Reconstruct the graph from a serialized snapshot image.
    /// </summary>
    public static HNSWIndex? Deserialize(Func<HNSWPoint, HNSWPoint, float> distFnc, string filePath, List<string>? itemSliceFilePaths = null, List<string>? nodeSliceFilePaths = null)
    {
        HNSWIndexData indexData = new HNSWIndexData();
        indexData.Body = File.ReadAllBytes(filePath);
        if(itemSliceFilePaths != null)
        {
            foreach(var path in itemSliceFilePaths)
            {
                indexData.ItemSlices.Add(File.ReadAllBytes(path));
            }
        }
        if(nodeSliceFilePaths != null)
        {
            foreach (var path in nodeSliceFilePaths)
            {
                indexData.NodeSlices.Add(File.ReadAllBytes(path));
            }
        }
        return Deserialize(distFnc, indexData);
    }

    public static HNSWIndex? Deserialize(Func<HNSWPoint, HNSWPoint, float> distFnc, byte[] buff)
    {
        var snapshot = MemoryPackSerializer.Deserialize<HNSWIndexSnapshot>(buff);
        return snapshot == null ? null : new HNSWIndex(distFnc, snapshot);
    }

    public static HNSWIndex? Deserialize(Func<HNSWPoint, HNSWPoint, float> distFnc, HNSWIndexData indexData)
    {
        var snapshot = MemoryPackSerializer.Deserialize<HNSWIndexSnapshot>(indexData.Body);
        List<ItemSlice> itemSlices = new List<ItemSlice>();
        List<NodeSlice> nodeSlices = new List<NodeSlice>();

        if(indexData.ItemSlices != null)
        {
            foreach(var item in indexData.ItemSlices)
            {
                var slice = MemoryPackSerializer.Deserialize<ItemSlice>(item);
                if(slice != null) itemSlices.Add(slice);
            }
        }

        if(indexData.NodeSlices != null)
        {
            foreach (var item in indexData.NodeSlices)
            {
                var slice = MemoryPackSerializer.Deserialize<NodeSlice>(item);
                if (slice != null) nodeSlices.Add(slice);
            }
        }

        snapshot?.Merge(itemSlices, nodeSlices);

        return snapshot == null ? null : new HNSWIndex(distFnc, snapshot);
    }

    private void OnDataResized(object? sender, ReallocateEventArgs e)
    {
        navigator.OnReallocate(e.NewCapacity);
    }
}
