﻿

using NScript.LiteDB;
using NScript.LiteDB.Services;
using NScript.LiteDB.Utils.Test.Data;
using RocksDbSharp;
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

void TestRocksDBFileStorageServiceByBucketPerformance(int count)
{
    var storage = new RocksDBFileStorageService(DBBucketStrategy.ByMonth,12);
    storage.BaseDir = DateTime.Now.ToFileTime().ToString();

    byte[] data = new byte[1024];

    List<String> files = new List<String>();

    DateTime dateTime = DateTime.Now;

    Random r = new Random();

    string GetNextFileId()
    {
        var nextFileId = storage.NextFileId(".dat");
        var month = r.Next(0, 12).ToString().PadLeft(2,'0');
        return nextFileId.Substring(0,4) + month + "00" + nextFileId.Substring(8);
    }


    Stopwatch sw = new Stopwatch();
    sw.Start();
    for (int i = 0; i < count; i++)
    {
        var fileId = GetNextFileId();
        storage.Save(fileId, data);
        files.Add(fileId);
        Console.WriteLine($"{i + 1}/{count}:{fileId}");
    }
    sw.Stop();

    long ts1 = sw.ElapsedMilliseconds;

    sw = new Stopwatch();
    for (int i = 0; i < count; i++)
    {
        var fileId = files[i];
        var bytes = storage.Find(fileId);
        Console.WriteLine($"{i + 1}/{count}: load file {fileId}, {bytes?.Length ?? 0} bytes");
    }
    sw.Stop();
    long ts2 = sw.ElapsedMilliseconds;

    Console.WriteLine($"Save {count} files, elapsed {ts1} ms");
    Console.WriteLine($"Load {count} files, elapsed {ts2} ms");
}

void TestRocksDB()
{
    var db = RocksDb.Open(new DbOptions().SetCreateIfMissing(true), "test.db");
    byte[] query = new byte[8];
    new Span<byte>(query).Fill(0x03);
    byte[] keys = new byte[10];
    byte[] values = new byte[10];
    new Span<byte>(keys).Fill(0x01);
    new Span<byte>(values).Fill(0x01);
    db.Put(keys,values);
    new Span<byte>(keys).Fill(0x03);
    new Span<byte>(values).Fill(0x03);
    db.Put(keys, values);
    new Span<byte>(keys).Fill(0x02);
    var it = db.NewIterator().Seek(query);
    var valid = it.Valid();
    var val = it.Value();
    Console.WriteLine(valid);
    Console.WriteLine(it.Key()[0]);
    Console.WriteLine(val.Length);
    Console.WriteLine(val[0]);
}

unsafe void TestFoo()
{
    //var span = stackalloc Foo[10];
    var s = new Foo();
    s.Name = "Hello World!";
    var p = &s;
    Console.WriteLine(p->Name);
}

//TestFoo();

//TestTypedDataService();
//TestFileStorageService();
//TestFileStorageService("test_data_litedb");
//TestRocksDBFileStorageService();
//TestRocksDBFileStorageService("test_data_rocksdb");
//TestRocksDBFileStorageServicePerformance(1000);
TestRocksDBFileStorageServiceByBucketPerformance(1000);
//TestRocksDB();
