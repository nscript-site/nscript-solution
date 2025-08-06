# 项目介绍

本项目在  HNSWINDEX 基础上修改，主要解决两个问题：

- 支持 NativeAOT
- 尽量去掉泛型，默认类型为 HNSWPoint，更贴近应用:

```csharp
public class HNSWPoint
{
    public float[] Data { get; set; } = Array.Empty<float>();

    public string Label { get; set; } = String.Empty;
}
```

```csharp
        var vectors = HNSWPoint.Random(128, 2_000);
        var vectors2 = HNSWPoint.Random(128, 1);
        var index = new HNSWIndex(HNSWPoint.CosineMetricUnitCompute);

        for (int i = 0; i < vectors.Count; i++)
            index.Add(vectors[i]);

        index.Serialize("GraphData.bin");
        var decodedIndex = HNSWIndex.Deserialize(HNSWPoint.CosineMetricUnitCompute, "GraphData.bin");

        var originalResults = index.KnnQuery(vectors2[0], 5);
        var decodeResults = decodedIndex.KnnQuery(vectors2[0], 5);
        Console.WriteLine(decodeResults[0].Distance);
        Console.WriteLine(originalResults[0].Distance);
        Console.WriteLine(decodeResults[2].Distance);
        Console.WriteLine(originalResults[2].Distance);
        Console.WriteLine(decodeResults[0].Point.Data[0]);
        Console.WriteLine(originalResults[0].Point.Data[0]);
        Console.WriteLine(decodeResults[0].Point.Label);
        Console.WriteLine(originalResults[0].Point.Label);
```
