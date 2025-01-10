using RocksDbSharp;

namespace NScript.Storage;

/// <summary>
/// 文件分片存储服务
/// </summary>
public interface IFileShardingStorageService
{
    /// <summary>
    /// 生成下一个文件的 Id。文件名称为 8字符日期+UUID+扩展名
    /// </summary>
    /// <param name="fileExtention"></param>
    /// <returns></returns>
    public string NextFileId(String fileExtention = "");

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Delete(String fileId);

    /// <summary>
    /// 保存文件，返回文件 fileId
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileExtention"></param>
    /// <returns></returns>
    public String Save(Byte[] data, String fileExtention);

    /// <summary>
    /// 保存文件到指定 fileId
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Save(String fileId, Byte[] data);

    /// <summary>
    /// 保存文件，返回文件 fileId
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileExtention"></param>
    /// <returns></returns>
    public String Save(Stream stream, String fileExtention);

    /// <summary>
    /// 保存文件到指定 fileId
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool Save(String fileId, Stream stream);

    /// <summary>
    /// 根据文件Id查找文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public byte[]? Find(String fileId);

    public Stream? FindStream(String fileId);
}

public class LocalDiskShardingFileStorageService : IFileShardingStorageService
{
    public string BaseDir { get; set; } = StorageSetting.DefaultStorageDirectory;

    public ShardingStrategy BucketStrategy { get; private set; }

    public String NextFileId(String fileExtention = "")
    {
        return BucketStrategy.NextFileId(null, fileExtention);
    }

    public String NextFileId(DateTime time, String fileExtention = "")
    {
        return BucketStrategy.NextFileId(time, fileExtention);
    }

    public LocalDiskShardingFileStorageService(ShardingStrategy bucketStrategy = ShardingStrategy.ByMonth, string? baseDir = null)
    {
        BucketStrategy = bucketStrategy;
        if (baseDir != null) BaseDir = baseDir;
    }

    /// <summary>
    /// 删除指定 fileId 的文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Delete(String fileId)
    {
        if (fileId == null) return false;

        var bucket = FindBucket(fileId);
        if (bucket == null) return false;
        return DeleteFile(bucket, fileId);
    }

    private bool DeleteFile(DirectoryInfo dir, string fileName)
    {
        var path = Path.Combine(dir.FullName, fileName);
        if (File.Exists(path) == true)
        {
            File.Delete(path);
            return true;
        }
        return false;
    }

    internal DirectoryInfo? FindBucket(String fileId)
    {
        if (fileId == null || fileId.Length < 10) return null;
        String bucketId = fileId.Substring(0, 8);
        return new DirectoryInfo(Path.Combine(BaseDir, "large_files", bucketId));
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

    internal bool SaveInternal(String fileId, Stream stream)
    {
        var bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        if (bucket.Exists == false) bucket.Create();

        var filePath = Path.Combine(bucket.FullName, fileId);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            stream.CopyTo(fileStream);
        }
        return true;
    }

    internal bool SaveInternal(String fileId, Byte[] data)
    {
        if (data == null) return false;
        var bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        if (bucket.Exists == false) bucket.Create();
        var filePath = Path.Combine(bucket.FullName, fileId);
        File.WriteAllBytes(filePath, data);
        return true;
    }

    public byte[]? Find(String fileId)
    {
        var bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        return GetFileData(bucket, fileId);
    }

    public Stream? FindStream(String fileId)
    {
        var bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        return GetFileStream(bucket, fileId);
    }

    private byte[]? GetFileData(DirectoryInfo dirInfo, string fileId)
    {
        var path = Path.Combine(dirInfo.FullName, fileId);
        if (File.Exists(path)) return File.ReadAllBytes(path);
        else return null;
    }

    private Stream? GetFileStream(DirectoryInfo dirInfo, string fileId)
    {
        var path = Path.Combine(dirInfo.FullName, fileId);
        if (File.Exists(path)) return File.OpenRead(path);
        else return null;
    }
}

internal class RocksDBFileDataBucket
{
    protected string DBName { get; } = "service_files";

    protected string DBPath { get; }

    internal string BucketBaseDir { get; set; } = StorageSetting.DefaultStorageDirectory;

    internal RocksDBShardingFileStorageService Owner { get; set; }

    protected String GetDir()
    {
        return GetDir(BucketBaseDir, "rocksdb_files", DBName, true);
    }

    protected static String GetDir(String baseDir, String dirName, string dbName, bool createIfNotExists = false)
    {
        if (dirName == null) throw new ArgumentNullException(nameof(dirName));

        String path = Path.Combine(baseDir, dirName, dbName);

        if (createIfNotExists == true)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists == false) dirInfo.Create();
        }

        return path;
    }

    public string BucketId { get; private set; }

    public static bool IsBucketExists(string baseDir, string bucketId)
    {
        var path = Path.Combine(baseDir, "rocksdb_files", "rocksdb_bucket_" + bucketId);
        return Directory.Exists(path);
    }

    public RocksDBFileDataBucket(String baseDir, String bucketId, RocksDBShardingFileStorageService owner)
    {
        if (String.IsNullOrEmpty(baseDir) == false)
        {
            this.BucketBaseDir = baseDir;
        }

        this.Owner = owner;

        this.BucketId = bucketId;

        this.DBName = "rocksdb_bucket_" + bucketId;
        DBPath = GetDir();
        Console.WriteLine($"Create RocksDBFileDataBucket: {DBPath}");
    }

    public bool DeleteFile(String fileId)
    {
        if (String.IsNullOrEmpty(fileId)) return false;

        bool rtn = false;
        UsingDB(db =>
        {
            var key = GetKey(fileId);
            db.Remove(key);
        });
        return true;
    }

    public void Insert(FileData fileData)
    {
        if (fileData == null || fileData.FileId == null || fileData.Data == null) return;

        UsingDB(db =>
        {
            var key = GetKey(fileData.FileId);
            db.Put(key, fileData.Data);
        });
    }

    public bool Exists()
    {
        return Directory.Exists(DBPath);
    }

    public FileData? FindOne(string fileid)
    {
        if (fileid == null) return null;

        FileData? find = null;
        byte[]? data = null;
        UsingDB(db => {
            var key = GetKey(fileid);
            data = db.Get(key);
        });

        if (data != null)
        {
            find = new FileData();
            find.FileId = fileid;
            find.Data = data;
            find.Length = data.LongLength;
        }
        return find;
    }

    private byte[] GetKey(string fileName)
    {
        return System.Text.Encoding.UTF8.GetBytes(fileName);
    }

    private void UsingDB(Action<RocksDb> onDatabase)
    {
        if (onDatabase == null) return;
        Owner.WithDB(DBPath, onDatabase);
    }
}

internal class RocksDbInfo
{
    public string DBPath { get; set; }
    public RocksDb DataBase { get; set; }
    public bool Using { get; set; }
}

/// <summary>
/// 本地RocksDB文件存储
/// </summary>
public class RocksDBShardingFileStorageService : IFileShardingStorageService, IDisposable
{
    public String NextFileId(String fileExtention = "")
    {
        return BucketStrategy.NextFileId(null, fileExtention);
    }

    public String NextFileId(DateTime time, String fileExtention = "")
    {
        return BucketStrategy.NextFileId(time, fileExtention);
    }

    public string BaseDir { get; set; } = StorageSetting.DefaultStorageDirectory;

    /// <summary>
    /// 设置数据库的 Options。默认为创建数据库
    /// </summary>
    public DbOptions Options { get; set; } = new DbOptions().SetCreateIfMissing(true);

    private int _maxCacheBuckets = 6;
    private List<RocksDbInfo> _cache = new List<RocksDbInfo>();

    public ShardingStrategy BucketStrategy { get; private set; }

    public RocksDBShardingFileStorageService(ShardingStrategy bucketStrategy = ShardingStrategy.ByMonth, int maxCacheBuckets = 6, string? baseDir = null)
    {
        _maxCacheBuckets = Math.Max(1, maxCacheBuckets);
        BucketStrategy = bucketStrategy;
        if (baseDir != null) BaseDir = baseDir;
    }

    private RocksDBFileDataBucket? LastBucket = null;

    internal RocksDbInfo GetOrCreateDb(string dbPath)
    {
        RocksDbInfo? db = null;
        lock (_cache)
        {
            db = _cache.FirstOrDefault(x => x.DBPath == dbPath);
            if (db == null)
            {
                var option = Options ?? new DbOptions().SetCreateIfMissing(true);
                db = new RocksDbInfo() { DBPath = dbPath, DataBase = RocksDb.Open(option, dbPath) };
                _cache.Add(db);
            }

            if (_cache.Count > _maxCacheBuckets)
            {
                // 移除旧的
                try
                {
                    var rdb = _cache[0];
                    rdb?.DataBase?.Dispose();
                    _cache.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        return db!;
    }

    internal void WithDB(string dbPath, Action<RocksDb> onDatabase)
    {
        if (onDatabase == null) return;

        RocksDbInfo? dbInfo = null;
        try
        {
            dbInfo = GetOrCreateDb(dbPath);
            dbInfo.Using = true;
            onDatabase(dbInfo.DataBase);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (dbInfo != null)
                dbInfo.Using = false;
        }
    }

    /// <summary>
    /// 删除指定 fileId 的文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Delete(String fileId)
    {
        if (fileId == null) return false;

        var bucket = FindBucket(fileId);
        if (bucket == null) return false;
        return bucket.DeleteFile(fileId); ;
    }

    public bool IsValid(String fileId)
    {
        if (fileId == null || fileId.Length < 10) return false;
        for (int i = 0; i < 8; i++)
        {
            if (Char.IsNumber(fileId[i]) == false) return false;
        }
        return true;
    }

    internal RocksDBFileDataBucket? FindBucket(String fileId, bool creatIfNotExist = true)
    {
        if (IsValid(fileId) == false) return null;
        String bucketId = fileId.Substring(0, 8);

        if (LastBucket?.BucketId == bucketId)  // 最近的 bucket 极大可能是所需要的 bucket
            return LastBucket;

        if (creatIfNotExist == false && RocksDBFileDataBucket.IsBucketExists(BaseDir, bucketId) == false)
            return null;

        var bucket = new RocksDBFileDataBucket(BaseDir, bucketId, this);
        LastBucket = bucket;
        return bucket;
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

    internal bool SaveInternal(String fileId, Byte[] data)
    {
        if (data == null) return false;
        RocksDBFileDataBucket? bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        bucket.Insert(new FileData() { FileId = fileId, Data = data, Length = data.LongLength });
        return true;
    }

    internal bool SaveInternal(String fileId, Stream stream)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            var data = memoryStream.ToArray();
            return SaveInternal(fileId, data);
        }
    }

    public byte[]? Find(String fileId)
    {
        RocksDBFileDataBucket? bucket = FindBucket(fileId, false);
        if (bucket == null) return null;
        if (bucket.Exists() == false) return null;
        var find = bucket.FindOne(fileId);
        return find?.Data ?? null;
    }

    public Stream? FindStream(String fileId)
    {
        var data = Find(fileId);
        return data == null ? null : new MemoryStream(data);
    }

    public void Dispose()
    {
        lock (_cache)
        {
            foreach (var item in _cache)
            {
                item?.DataBase?.Dispose();
            }
            _cache.Clear();
        }
    }
}

/// <summary>
/// 本地文件分片存储系统。对于小文件，存储在 rocksdb 中，大文件，存储在本地文件系统里面。均按照日期分片存储。
/// </summary>
public class LocalShardingFileStorageService
{
    private RocksDBShardingFileStorageService rocksDBFileStorageService;
    private LocalDiskShardingFileStorageService localDiskFileStorageService;

    public ShardingStrategy BucketStrategy { get; private set; }

    public String NextFileId(String fileExtention = "")
    {
        return BucketStrategy.NextFileId(null, fileExtention);
    }

    public String NextFileId(DateTime time, String fileExtention = "")
    {
        return BucketStrategy.NextFileId(time, fileExtention);
    }

    private int MaxBytesSaveInRocksDB = 16 * 1024 * 1024;

    public LocalShardingFileStorageService(ShardingStrategy bucketStrategy = ShardingStrategy.ByMonth, int maxMBytesSaveInRocksDB = 16, int maxCacheBuckets = 6, string? baseDir = null)
    {
        BucketStrategy = bucketStrategy;
        MaxBytesSaveInRocksDB = Math.Max(0, maxMBytesSaveInRocksDB) * 1024 * 1024;
        rocksDBFileStorageService = new RocksDBShardingFileStorageService(bucketStrategy, maxCacheBuckets, baseDir);
        localDiskFileStorageService = new LocalDiskShardingFileStorageService(bucketStrategy, baseDir);
    }

    /// <summary>
    /// 删除指定 fileId 的文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Delete(String fileId)
    {
        if (fileId == null) return false;

        if (MaxBytesSaveInRocksDB > 0 && rocksDBFileStorageService.Delete(fileId) == true) return true;
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
        if (data.Length > MaxBytesSaveInRocksDB) return localDiskFileStorageService.SaveInternal(fileId, data);
        else return rocksDBFileStorageService.SaveInternal(fileId, data);
    }

    public byte[]? Find(String fileId)
    {
        if (MaxBytesSaveInRocksDB > 0)
            return rocksDBFileStorageService.Find(fileId) ?? localDiskFileStorageService.Find(fileId);
        else
            return localDiskFileStorageService.Find(fileId);
    }

    public Stream? FindStream(String fileId)
    {
        if (MaxBytesSaveInRocksDB > 0)
            return rocksDBFileStorageService.FindStream(fileId) ?? localDiskFileStorageService.FindStream(fileId);
        else
            return localDiskFileStorageService.FindStream(fileId);
    }
}
