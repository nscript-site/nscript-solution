using System.Numerics;
using MemoryPack;

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
}