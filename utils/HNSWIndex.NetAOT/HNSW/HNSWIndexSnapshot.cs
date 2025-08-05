using System.Numerics;
using ProtoBuf;

namespace HNSW;

/// <summary>
/// Wrapper for HNSWIndex for serialization.
/// </summary>
[ProtoContract]
internal class HNSWIndexSnapshot
{
    [ProtoMember(1)]
    internal HNSWParameters? Parameters { get; set; }

    [ProtoMember(2)]
    internal GraphDataSnapshot? DataSnapshot { get; set; }

    internal HNSWIndexSnapshot() { }

    internal HNSWIndexSnapshot(HNSWParameters parameters, GraphData data)
    {
        Parameters = parameters;
        DataSnapshot = new GraphDataSnapshot(data);
    }
}