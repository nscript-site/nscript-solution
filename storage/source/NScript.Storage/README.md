# NScript.Storage

NScript.Storage 项目，针对 AI 应用场景，基于 RocksDB，提供了一些方便的文件存储的功能。当前支持：

- 分片式文件存储

## 分片式文件存储

`LocalShardingFileStorageService` 类支持按照年或月或日，进行分片式存储。可以设定一个文件大小的阈值，当文件大小小于该阈值时，将文件存储在 RocksDB 中，当文件大小大于该阈值时，将文件存储在磁盘上。可以根据文件 id 进行读取和删除。

需要注意的是，分片式文件存储规定文件 id 必须为特殊格式，规定如下：

- 按年分片：文件 id 示例为 20240000*************
- 按月分片：文件 id 示例为 20241200*************
- 按日分片：文件 id 示例为 20241221*************

文件 id 的前8个字符为分片/分桶的id，有时也称为 bucket id。

ShardingStrategy 提供了扩展方法 `GetShardingId` 可以获得某 DateTime 的分片id。提供了 `NextFileId` 扩展方法，可以直接获得某 DateTime 下的合法随机文件 id。

使用示例：

```csharp

    var dir = new DirectoryInfo(DateTime.Now.ToFileTime().ToString());

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

    // 删除文件
    bool deleteResult = service.Delete(fileId1);

```

Save 方法也支持对 Stream 进行操作。另提供了 FindStream 方法，以流的方式获取文件。对于大文件的处理，推荐通过流的方式进行。

`RocksDBShardingFileStorageService`类和 `LocalDiskShardingFileStorageService`类也提供和 `LocalShardingFileStorageService` 相似的服务，不同的是，RocksDBShardingFileStorageService 的数据只存于 RocksDB 中，LocalDiskShardingFileStorageService 的数据只存于文件系统中。
