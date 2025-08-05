namespace HNSW;

public class KNNResult
{
    public int Id { get; private set; }
    public HNSWPoint Label { get; private set; }
    public float Distance { get; private set; }

    internal KNNResult(int id, HNSWPoint label, float distance)
    {
        Id = id;
        Label = label;
        Distance = distance;
    }
}
