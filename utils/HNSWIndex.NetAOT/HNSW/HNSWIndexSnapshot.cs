using MemoryPack;
using System.Collections.Concurrent;
using System.Numerics;

namespace HNSW;

/// <summary>
/// Wrapper for HNSWIndex for serialization.
/// </summary>
[MemoryPackable]
public partial class HNSWIndexSnapshot
{
    public HNSWParameters? Parameters { get; set; }

    public GraphDataSnapshot? DataSnapshot { get; set; }

    [MemoryPackConstructor]
    public HNSWIndexSnapshot() { }

    public HNSWIndexSnapshot(HNSWParameters parameters, GraphData data)
    {
        Parameters = parameters;
        DataSnapshot = new GraphDataSnapshot(data);
    }

    public bool NeedSlice(int maxCount = 500000)
    {
        return DataSnapshot?.Items?.Count > maxCount || DataSnapshot?.Nodes?.Count > maxCount;
    }

    public (List<ItemSlice>,List<NodeSlice>) Slice(int maxCount = 500000)
    {
        return (DataSnapshot?.SliceItems(maxCount) ?? new List<ItemSlice>(), DataSnapshot?.SliceNodes(maxCount) ?? new List<NodeSlice>());
    }

    // ºÏ²¢
    public void Merge(List<ItemSlice> itemSlices, List<NodeSlice> nodeSlices)
    {
        if (DataSnapshot == null) DataSnapshot = new GraphDataSnapshot();
        DataSnapshot.MergeItems(itemSlices);
        DataSnapshot.MergeNodes(nodeSlices);
    }
}