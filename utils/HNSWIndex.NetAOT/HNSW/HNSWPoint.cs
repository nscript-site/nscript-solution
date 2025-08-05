using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HNSW;

[ProtoContract]
public class HNSWPoint
{
    [ProtoMember(1)]
    public float[] Data { get; set; } = Array.Empty<float>();

    [ProtoMember(2)]
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

    public static List<HNSWPoint> Random(int vectorSize, int vectorsCount)
    {
        var random = new Random(vectorsCount);
        var vectors = new List<HNSWPoint>();

        for (int i = 0; i < vectorsCount; i++)
        {
            var vector = new float[vectorSize];
            for (int d = 0; d < vectorSize; d++)
                vector[d] = random.NextSingle();
            vectors.Add(new HNSWPoint() { Data = vector, Label = i.ToString() });
        }

        return vectors;
    }
}
