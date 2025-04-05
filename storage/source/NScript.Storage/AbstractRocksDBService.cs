using RocksDbSharp;

namespace NScript.Storage;

public class RocksDbInfo
{
    public string DBPath { get; set; }
    public RocksDb DataBase { get; set; }
    public bool Using { get; set; }
}

public abstract class AbstractRocksDBService
{
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

    protected abstract RocksDbInfo GetOrCreateDb(string dbPath);
}

internal class RocksDBFileDataBucket
{
    protected string DBName { get; } = "service_files";

    protected string DBPath { get; }

    internal string BucketBaseDir { get; set; } = StorageSetting.DefaultStorageDirectory;

    internal AbstractRocksDBService Owner { get; set; }

    protected static String GetDir(String baseDir, String dirName, string bucketName, bool createIfNotExists = false)
    {
        if (dirName == null) throw new ArgumentNullException(nameof(dirName));

        String path = String.IsNullOrEmpty(bucketName) ? Path.Combine(baseDir, dirName) : Path.Combine(baseDir, dirName, bucketName);

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
        var path = String.IsNullOrEmpty(bucketId) ? Path.Combine(baseDir, "rocksdb_files") : Path.Combine(baseDir, "rocksdb_files", "rocksdb_bucket_" + bucketId);
        return Directory.Exists(path);
    }

    public RocksDBFileDataBucket(String baseDir, String bucketId, AbstractRocksDBService owner)
    {
        if (String.IsNullOrEmpty(baseDir) == false)
        {
            this.BucketBaseDir = baseDir;
        }

        this.Owner = owner;

        this.BucketId = bucketId;

        this.DBName = String.IsNullOrEmpty(bucketId)? String.Empty : "rocksdb_bucket_" + bucketId;

        DBPath = GetDir(BucketBaseDir, "rocksdb_files", DBName, true);

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

