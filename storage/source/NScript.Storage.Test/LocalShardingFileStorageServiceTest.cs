using System.ComponentModel.DataAnnotations;

namespace NScript.Storage.Test;

[TestClass]
public sealed class LocalShardingFileStorageServiceTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TestBaseDir()
    {
        var dir = new DirectoryInfo(DateTime.Now.ToFileTime().ToString());

        TestContext.WriteLine($"Dir: {dir.FullName}");

        var shardingStrategy = ShardingStrategy.ByDay;
        var maxMBytesSaveInRocksDB = 16;    // 16M 以下的存在 rocksdb 中
        var maxRocksDBCacheBuckets = 2;     // rocksdb 的分片实例缓存数量，避免频繁打开关闭 rocksdb
        var service = new LocalShardingFileStorageService(shardingStrategy, maxMBytesSaveInRocksDB, maxRocksDBCacheBuckets, dir.FullName);
        var bytes1 = new byte[1024];
        bytes1[0] = 1;
        var bytes2 = new byte[1024 * 1024 * 24];
        bytes2[0] = 2;
        var bytes3 = new byte[1024];
        bytes3[0] = 3;

        // 存储前先获取 fileId。根据当前时间，计算分片id，分配对应的随机文件名
        var fileId1 = service.NextFileId(".dat");

        service.Save(fileId1, bytes1);

        // 直接存储，返回根据当前时间，计算分片id，分配对应的随机文件名
        var fileId2 = service.Save(bytes2, ".dat");

        service.Save(fileId2, bytes2);

        // 根据指定时间和分片策略，计算分片 id，分配随机文件名
        var fileId3 = shardingStrategy.NextFileId(new DateTime(2024, 12, 20), ".dat");

        service.Save(fileId3, bytes3);

        // 查找
        bytes1 = service.Find(fileId1);
        bytes2 = service.Find(fileId2);
        bytes3 = service.Find(fileId3);

        Assert.IsNotNull(bytes1);
        Assert.IsNotNull(bytes2);
        Assert.IsNotNull(bytes3);
        Assert.AreEqual(1024, bytes1.Length);
        Assert.AreEqual(1024 * 1024 * 24, bytes2.Length);
        Assert.AreEqual(1, bytes1[0]);
        Assert.AreEqual(2, bytes2[0]);
        Assert.AreEqual(3, bytes3[0]);
        Assert.AreEqual(true, Directory.Exists(Path.Combine(dir.FullName, "large_files")));

        // 删除文件
        bool deleteResult = service.Delete(fileId1);
        Assert.IsTrue(deleteResult);
    }
}