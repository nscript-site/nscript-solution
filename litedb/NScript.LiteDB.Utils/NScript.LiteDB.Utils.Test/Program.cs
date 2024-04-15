

using NScript.LiteDB;
using NScript.LiteDB.Services;
using NScript.LiteDB.Utils.Test.Data;

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


//TestTypedDataService();
TestFileStorageService();
TestFileStorageService("test_data_litedb");
TestRocksDBFileStorageService();
TestRocksDBFileStorageService("test_data_rocksdb");
