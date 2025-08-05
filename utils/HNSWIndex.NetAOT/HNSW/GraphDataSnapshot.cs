using System.Collections.Concurrent;
using System.Numerics;
using ProtoBuf;

namespace HNSW;

/// <summary>
/// Wrapper for GraphData for serialization.
/// </summary>
[ProtoContract]
internal class GraphDataSnapshot
{
    [ProtoMember(1)]
    internal List<Node>? Nodes { get; set; }

    [ProtoMember(2)]
    internal ConcurrentDictionary<int, HNSWPoint>? Items { get; set; }

    [ProtoMember(3)]
    internal Queue<int>? RemovedIndexes { get; set; }

    [ProtoMember(4)]
    internal int EntryPointId = -1;

    [ProtoMember(5)]
    internal int Capacity;

    internal GraphDataSnapshot() { }

    internal GraphDataSnapshot(GraphData data)
    {
        Nodes = data.Nodes;
        Items = data.Items;
        RemovedIndexes = data.RemovedIndexes;
        EntryPointId = data.EntryPointId;
        Capacity = data.Capacity;
    }
}