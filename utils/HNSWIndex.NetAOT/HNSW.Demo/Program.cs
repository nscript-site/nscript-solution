using System.IO.Compression;

namespace HNSW.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        Test(2000, 5000, true);
        Test(20000, 5000, true);
    }

    static void Test(int total, int sliceMaxCount, bool nomalize = true)
    {
        Console.WriteLine($"[RUN TEST]: total - {total}, slice - {sliceMaxCount}, nomalize - {nomalize}");

        var vectors = HNSWPoint.Random(128, total, nomalize);
        var vectors2 = HNSWPoint.Random(128, 1, nomalize);

        Console.WriteLine($"{total} points generated");

        var index = new HNSWIndex(HNSWPoint.CosineMetricUnitCompute);

        index.BatchAdd(vectors, 100);

        Console.WriteLine($"index generated");

        var dataFile = $"GraphData_{total}_{sliceMaxCount}.bin";

        Console.WriteLine($"index deserialized");

        index.Serialize(dataFile, sliceMaxCount);

        Console.WriteLine($"index serialized");

        var originalResults = index.KnnQuery(vectors2[0], 5);
        Console.WriteLine("originalResults:");
        Console.WriteLine(originalResults[0].Distance);
        Console.WriteLine(originalResults[2].Distance);
        Console.WriteLine(originalResults[0].Point.Data[0]);
        Console.WriteLine(originalResults[0].Point.Label);

        var decodedIndex = HNSWIndex.Deserialize(HNSWPoint.CosineMetricUnitCompute, dataFile);

        if (decodedIndex != null)
        {
            var decodeResults = decodedIndex.KnnQuery(vectors2[0], 5);
            Console.WriteLine("decodeResults:");
            Console.WriteLine(decodeResults[0].Distance);
            Console.WriteLine(decodeResults[2].Distance);
            Console.WriteLine(decodeResults[0].Point.Data[0]);
            Console.WriteLine(decodeResults[0].Point.Label);
        }
    }

    private static string ClipIndexEntry = "clip.idx";

    static void Load(string path)
    {
        if (File.Exists(path) == true)
        {
            using (var zip = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                foreach (var item in zip.Entries)
                {
                    if (item.Name == ClipIndexEntry)
                    {
                        var buff = new byte[item.Length];
                        using (var stream = item.Open())
                        using (var ms = new MemoryStream(buff))
                        {
                            ms.Position = 0;
                            stream.CopyTo(ms);
                        }

                        var clip = HNSWIndex.Deserialize(HNSWPoint.CosineMetricUnitCompute, buff);
                        Console.WriteLine($"Clip index loaded with {clip?.Count??0} items.");
                    }
                }
            }
        }
    }
}
