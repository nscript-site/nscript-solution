using LiteDB;
using NScript.LiteDB.Utils;
using RocksDbSharp;

namespace NScript.LiteDB.Services;

internal class RocksDBFileDataBucket
{
    protected string DBName { get; } = "service_files";

    protected string DBPath { get; }

    internal string BucketBaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

    internal RocksDBShardingOnTimeFileStorageService Owner { get; set; }

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

    public RocksDBFileDataBucket(String baseDir, String bucketId, RocksDBShardingOnTimeFileStorageService owner)
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
        if(fileid == null) return null;

        FileData? find = null;
        byte[]? data = null;
        UsingDB(db => {
            var key = GetKey(fileid);
            data = db.Get(key);
        });

        if(data != null)
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
/// 本地文件存储
/// </summary>
public class RocksDBShardingOnTimeFileStorageService : IFileStorageService, IDisposable
{
    public String NextFileId(String fileExtention = "")
    {
        return BucketStrategy.NextFileId(null, fileExtention);
    }

    public String NextFileId(DateTime time, String fileExtention = "")
    {
        return BucketStrategy.NextFileId(time, fileExtention);
    }

    public string BaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

    /// <summary>
    /// 设置数据库的 Options。默认为创建数据库
    /// </summary>
    public DbOptions Options { get; set; } = new DbOptions().SetCreateIfMissing(true);

    private int _maxCacheBuckets = 6;
    private List<RocksDbInfo> _cache = new List<RocksDbInfo>();

    public ShardingOnTimeStrategy BucketStrategy { get; private set; }

    public RocksDBShardingOnTimeFileStorageService(ShardingOnTimeStrategy bucketStrategy = ShardingOnTimeStrategy.ByMonth, int maxCacheBuckets = 6, string? baseDir = null)
    {
        _maxCacheBuckets = Math.Max(1,maxCacheBuckets);
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
                catch(Exception ex)
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
            if(dbInfo != null)
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
        for(int i = 0; i < 8; i++)
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
            foreach(var item in _cache)
            {
                item?.DataBase?.Dispose();
            }
            _cache.Clear();
        }
    }
}