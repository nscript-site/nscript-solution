using System.Numerics;

namespace HNSW.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        var vectors = HNSWPoint.Random(128, 2_000);
        var index = new HNSWIndex(HNSWPoint.CosineMetricUnitCompute);

        for (int i = 0; i < vectors.Count; i++)
            index.Add(vectors[i]);

        index.Serialize("GraphData.bin");
        var decodedIndex = HNSWIndex.Deserialize(HNSWPoint.CosineMetricUnitCompute, "GraphData.bin");

        var originalResults = index.KnnQuery(vectors[0], 5);
        var decodeResults = decodedIndex.KnnQuery(vectors[0], 5);
        Console.WriteLine(decodeResults);
    }
}
