namespace NScript.LiteDB.Services;

/// <summary>
/// 本地文件分片存储系统。对于小文件，存储在 rocksdb 中，大文件，存储在本地文件系统里面。均按照日期分片存储。
/// </summary>
public class LocalShardingOnTimeFileStorageService
{
    private RocksDBShardingOnTimeFileStorageService rocksDBFileStorageService;
    private LocalDiskShardingOnTimeFileStorageService localDiskFileStorageService;

    public ShardingOnTimeStrategy BucketStrategy { get; private set; }

    public String NextFileId(String fileExtention = "")
    {
        return BucketStrategy.NextFileId(null, fileExtention);
    }

    public String NextFileId(DateTime time, String fileExtention = "")
    {
        return BucketStrategy.NextFileId(time, fileExtention);
    }

    private int MaxBytesSaveInRocksDB = 16 * 1024 * 1024;

    public LocalShardingOnTimeFileStorageService(ShardingOnTimeStrategy bucketStrategy = ShardingOnTimeStrategy.ByMonth, int maxMBytesSaveInRocksDB = 16, int maxCacheBuckets = 6)
    {
        BucketStrategy = bucketStrategy;
        MaxBytesSaveInRocksDB = Math.Max(1, maxMBytesSaveInRocksDB) * 1024 * 1024;
        rocksDBFileStorageService = new RocksDBShardingOnTimeFileStorageService(bucketStrategy, maxCacheBuckets);
        localDiskFileStorageService = new LocalDiskShardingOnTimeFileStorageService(bucketStrategy);
    }

    /// <summary>
    /// 删除指定 fileId 的文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Delete(String fileId)
    {
        if (fileId == null) return false;

        if (rocksDBFileStorageService.Delete(fileId) == true) return true;
        else return localDiskFileStorageService.Delete(fileId);
    }

    public bool Save(String fileId, Byte[] data)
    {
        if (String.IsNullOrEmpty(fileId)) throw new ArgumentException(nameof(fileId));

        SaveInternal(fileId, data);
        return true;
    }

    public String Save(Byte[] data, String fileExtention)
    {
        String fileId = NextFileId(fileExtention);
        SaveInternal(fileId, data);
        return fileId;
    }

    public bool Save(String fileId, Stream stream)
    {
        if (String.IsNullOrEmpty(fileId)) throw new ArgumentException(nameof(fileId));

        SaveInternal(fileId, stream);
        return true;
    }

    public String Save(Stream stream, String fileExtention)
    {
        String fileId = NextFileId(fileExtention);
        SaveInternal(fileId, stream);
        return fileId;
    }

    protected bool SaveInternal(String fileId, Stream stream)
    {
        if (stream.Length > MaxBytesSaveInRocksDB) return localDiskFileStorageService.SaveInternal(fileId, stream);
        else return rocksDBFileStorageService.SaveInternal(fileId, stream);
    }

    protected bool SaveInternal(String fileId, Byte[] data)
    {
        if (data == null) return false;
        if(data.Length > MaxBytesSaveInRocksDB) return localDiskFileStorageService.SaveInternal(fileId, data);
        else return rocksDBFileStorageService.SaveInternal(fileId,data);
    }

    public byte[]? Find(String fileId)
    {
        return rocksDBFileStorageService.Find(fileId) ?? localDiskFileStorageService.Find(fileId);
    }

    public Stream? FindStream(String fileId)
    {
        return rocksDBFileStorageService.FindStream(fileId) ?? localDiskFileStorageService.FindStream(fileId);
    }
}
