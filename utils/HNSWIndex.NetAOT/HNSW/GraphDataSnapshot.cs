using MemoryPack;
using System.Collections.Concurrent;
using System.Numerics;

namespace HNSW;

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
}