using MemoryPack;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace HNSW;

[MemoryPackable]
public partial class HNSWPoint
{
    public float[] Data { get; set; } = Array.Empty<float>();

    public string Label { get; set; } = String.Empty;

    public static unsafe float CosineMetricUnitCompute(HNSWPoint a, HNSWPoint b)
    {
        return Metrics.CosineMetric.UnitCompute(a.Data, b.Data);
    }

    public static float Magnitude(float[] vector)
    {
        float magnitude = 0.0f;
        int step = Vector<float>.Count;
        for (int i = 0; i < vector.Length; i++)
        {
            magnitude += vector[i] * vector[i];
        }
        return (float)Math.Sqrt(magnitude);
    }

    public static void Normalize(float[] vector)
    {
        float normFactor = 1f / Magnitude(vector);
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] *= normFactor;
        }
    }

    public static List<HNSWPoint> Random(int vectorSize, int vectorsCount, bool normalize = true)
    {
        var random = new Random(vectorsCount);
        var vectors = new List<HNSWPoint>();

        for (int i = 0; i < vectorsCount; i++)
        {
            var vector = new float[vectorSize];
            for (int d = 0; d < vectorSize; d++)
                vector[d] = random.NextSingle();
            if(normalize == true)
                Normalize(vector);
            vectors.Add(new HNSWPoint() { Data = vector, Label = i.ToString() });
        }

        return vectors;
    }
}
