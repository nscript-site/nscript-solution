using MemoryPack;

namespace HNSW;

[MemoryPackable]
public partial class Node
{
    public int Id;

    [MemoryPackIgnore]
    public object OutEdgesLock { get; } = new();

    [MemoryPackIgnore]
    public object InEdgesLock { get; } = new();

    public List<List<int>> OutEdges { get; set; } = new();

    public List<List<int>> InEdges { get; set; } = new();

    public int MaxLayer => OutEdges.Count - 1;

    // Trick to serialize lists of lists
    private List<IntListWrapper> OutEdgesSerialized
    {
        get => OutEdges.Select(l => new IntListWrapper(l)).ToList();
        set => OutEdges = (value ?? new List<IntListWrapper>()).Select(w => w.Values).ToList();
    }

    // Trick to serialize lists of lists
    private List<IntListWrapper> InEdgesSerialized
    {
        get => InEdges.Select(l => new IntListWrapper(l)).ToList();
        set => InEdges = (value ?? new List<IntListWrapper>()).Select(w => w.Values).ToList();
    }

    private void AfterDeserialization()
    {
        for (int i = 0; i <= MaxLayer; i++)
        {
            OutEdges[i] ??= new List<int>();
            InEdges[i] ??= new List<int>();
        }
    }
}

[MemoryPackable]
public partial struct IntListWrapper
{
    [MemoryPackConstructor]
    public IntListWrapper()
    {
    }

    public IntListWrapper(List<int> values) => Values = values;

    public List<int> Values { get; set; }
}