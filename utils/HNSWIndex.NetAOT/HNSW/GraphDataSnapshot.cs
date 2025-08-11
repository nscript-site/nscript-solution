using MemoryPack;
using System.Collections.Concurrent;

namespace HNSW;

[MemoryPackable]
public partial class ItemSlice
{
    public List<IndexedHNSWPoint> Items { get; set; } = new List<IndexedHNSWPoint>();

    public int Count { get => Items.Count; }
}

[MemoryPackable]
public partial class IndexedHNSWPoint
{
    public int Index;
    public HNSWPoint? Point;
}

[MemoryPackable]
public partial class NodeSlice
{
    public List<Node> Nodes { get; set; } = new List<Node>();
}

/// <summary>
/// Wrapper for GraphData for serialization.
/// </summary>
[MemoryPackable]
public partial class GraphDataSnapshot
{
    public List<Node>? Nodes { get; set; }

    public ConcurrentDictionary<int, HNSWPoint>? Items { get; set; }

    public Queue<int>? RemovedIndexes { get; set; }

    public int EntryPointId = -1;

    public int Capacity;

    [MemoryPackConstructor]
    public GraphDataSnapshot() { }

    public GraphDataSnapshot(GraphData data)
    {
        Nodes = data.Nodes;
        Items = data.Items;
        RemovedIndexes = data.RemovedIndexes;
        EntryPointId = data.EntryPointId;
        Capacity = data.Capacity;
    }

    /// <summary>
    /// ·Ö¸î
    /// </summary>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public List<ItemSlice> SliceItems(int maxCount = 500000)
    {
        var list = new List<ItemSlice>();
        if(Items != null)
        {
            ItemSlice slice = new ItemSlice();
            foreach (var kp in Items)
            {
                var p = new IndexedHNSWPoint() { Index = kp.Key, Point = kp.Value };
                slice.Items.Add(p);
                if(slice.Count >= maxCount)
                {
                    list.Add(slice);
                    slice = new ItemSlice();
                }
            }

            Items = null;
            if (slice.Count > 0)
                list.Add(slice);
        }
        return list;
    }

    // ºÏ²¢
    public void MergeItems(List<ItemSlice> slices)
    {
        if (Items == null) Items = new ConcurrentDictionary<int, HNSWPoint>();
        foreach(var slice in slices)
        {
            foreach(var item in slice.Items)
            {
                if(item.Point != null)
                    Items[item.Index] = item.Point;
            }
        }
    }

    public List<NodeSlice> SliceNodes(int maxCount = 500000)
    {
        var list = new List<NodeSlice>();
        if (Nodes != null)
        {
            NodeSlice slice = new NodeSlice();
            foreach (var node in Nodes)
            {
                slice.Nodes.Add(node);
                if (slice.Nodes.Count >= maxCount)
                {
                    list.Add(slice);
                    slice = new NodeSlice();
                }
            }
            Nodes = null;
            if (slice.Nodes.Count > 0)
                list.Add(slice);
        }
        return list;
    }

    public void MergeNodes(List<NodeSlice> slices)
    {
        if (Nodes == null) Nodes = new List<Node>();
        foreach (var slice in slices)
        {
            Nodes.AddRange(slice.Nodes);
        }
    }
}