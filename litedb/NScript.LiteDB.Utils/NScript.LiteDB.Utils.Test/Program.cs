

using NScript.LiteDB;
using NScript.LiteDB.Services;
using NScript.LiteDB.Utils.Test.Data;
using System.Diagnostics;

void TestTypedDataService()
{
    var service1 = new DataService<Book>();
    var service2 = new DataService<DefaultBook>();
    var service3 = new DataService<Book>("bucketA");
    service1.Insert(new Book() { Name = "book1" });
    service2.Insert(new DefaultBook() { Name = "book2" });
    service3.Insert(new Book() { Name = "book3" });
    Console.WriteLine(service1.Count());
    Console.WriteLine(service2.Count());
    Console.WriteLine(service3.Count());
}

void TestFileStorageService(String? dir = null)
{
    var storage = new FileStorageService();
    if (dir != null) storage.BaseDir = dir;

    byte[] data = new byte[10];
    var fileId = storage.Save(data, ".dat");
    Console.WriteLine(fileId);
    var item = storage.Find(fileId);
    Console.WriteLine(item.Length);
    var rtn = storage.Delete(fileId);
    Console.WriteLine(rtn);
    item = storage.Find(fileId);
    if (item == null) Console.WriteLine("Delete OK!");
}

void TestRocksDBFileStorageService(String? dir = null)
{
    var storage = new RocksDBFileStorageService();
    if (dir != null) storage.BaseDir = dir;

    byte[] data = new byte[10];
    var fileId = storage.Save(data, ".dat");
    Console.WriteLine(fileId);
    var item = storage.Find(fileId);
    Console.WriteLine(item.Length);
    var rtn = storage.Delete(fileId);
    Console.WriteLine(rtn);
    item = storage.Find(fileId);
    if (item == null) Console.WriteLine("Delete OK!");
}

void TestRocksDBFileStorageServicePerformance(int count)
{
    var storage = new RocksDBFileStorageService();
    storage.BaseDir = DateTime.Now.ToFileTime().ToString();

    byte[] data = new byte[1024];

    List<String> files = new List<String>();

    Stopwatch sw = new Stopwatch();
    sw.Start();
    for (int i = 0; i < count; i++)
    {
        var fileId = storage.Save(data, ".dat");
        files.Add(fileId);
        Console.WriteLine($"{i+1}/{count}:{fileId}");
    }
    sw.Stop();

    long ts1 = sw.ElapsedMilliseconds;

    sw = new Stopwatch();
    for (int i = 0; i < count; i++)
    {
        var fileId = files[i];
        var bytes = storage.Find(fileId);
        Console.WriteLine($"{i + 1}/{count}: load file {fileId}, {bytes?.Length??0} bytes");
    }
    sw.Stop();
    long ts2 = sw.ElapsedMilliseconds;

    Console.WriteLine($"Save {count} files, elapsed {ts1} ms");
    Console.WriteLine($"Load {count} files, elapsed {ts2} ms");
}

//TestTypedDataService();
//TestFileStorageService();
//TestFileStorageService("test_data_litedb");
//TestRocksDBFileStorageService();
//TestRocksDBFileStorageService("test_data_rocksdb");
TestRocksDBFileStorageServicePerformance(1000);
