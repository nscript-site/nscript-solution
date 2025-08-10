using System.IO;
using System.IO.Compression;
using System.Numerics;

namespace HNSW.Demo;

internal class Program
{
    static void Main(string[] args)
    {
        //bool nomalize = true;
        //var vectors = HNSWPoint.Random(128, 2_000, nomalize);
        //var vectors2 = HNSWPoint.Random(128, 1, nomalize);
        //var index = new HNSWIndex(HNSWPoint.CosineMetricUnitCompute);

        //for (int i = 0; i < vectors.Count; i++)
        //    index.Add(vectors[i]);

        //index.Serialize("GraphData.bin");
        //var decodedIndex = HNSWIndex.Deserialize(HNSWPoint.CosineMetricUnitCompute, "GraphData.bin");

        //var originalResults = index.KnnQuery(vectors2[0], 5);
        //var decodeResults = decodedIndex.KnnQuery(vectors2[0], 5);
        //Console.WriteLine($"nomalize: {nomalize}");
        //Console.WriteLine(decodeResults[0].Distance);
        //Console.WriteLine(originalResults[0].Distance);
        //Console.WriteLine(decodeResults[2].Distance);
        //Console.WriteLine(originalResults[2].Distance);
        //Console.WriteLine(decodeResults[0].Point.Data[0]);
        //Console.WriteLine(originalResults[0].Point.Data[0]);
        //Console.WriteLine(decodeResults[0].Point.Label);
        //Console.WriteLine(originalResults[0].Point.Label);

        //var r1 = index.KnnQuery(vectors[0], 5);
        //Console.WriteLine(r1[0].Distance);

        Load("D:\\测试数据\\im\\StockPhotos(CC0)\\meta.omnivdat");
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
