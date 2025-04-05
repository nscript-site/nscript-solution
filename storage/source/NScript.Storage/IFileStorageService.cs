using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage;

/// <summary>
/// 文件存储服务
/// </summary>
public interface IFileStorageService
{
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

public class LocalDiskFileStorageService : IFileStorageService
{
    public string BaseDir { get; set; } = StorageSetting.DefaultStorageDirectory;

    public LocalDiskFileStorageService(string? baseDir = null)
    {
        if (baseDir != null) BaseDir = baseDir;
    }

    public String NextFileId(String fileExtention = "")
    {
        return Guid.NewGuid().ToString("N") + fileExtention ?? String.Empty;
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
        return new DirectoryInfo(Path.Combine(BaseDir, "files", bucketId));
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

/// <summary>
/// 本地RocksDB文件存储
/// </summary>
public class RocksDBFileStorageService : AbstractRocksDBService, IFileStorageService, IDisposable
{
    public String NextFileId(String fileExtention = "")
    {
        return Guid.NewGuid().ToString("N") + fileExtention ?? String.Empty;
    }

    public string BaseDir { get; set; } = StorageSetting.DefaultStorageDirectory;

    /// <summary>
    /// 设置数据库的 Options。默认为创建数据库
    /// </summary>
    public DbOptions Options { get; set; } = new DbOptions().SetCreateIfMissing(true);

    private RocksDbInfo? _cache;

    private RocksDBFileDataBucket? bucket;

    public RocksDBFileStorageService(string? baseDir = null)
    {
        if (baseDir != null) BaseDir = baseDir;
    }

    protected override RocksDbInfo GetOrCreateDb(string dbPath)
    {
        RocksDbInfo? db = null;

        if (_cache != null) return _cache;

        lock (this)
        {
            if (_cache == null)
            {
                var option = Options ?? new DbOptions().SetCreateIfMissing(true);
                _cache = new RocksDbInfo() { DBPath = dbPath, DataBase = RocksDb.Open(option, dbPath) };
            }
        }

        return _cache;
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
        return true;
    }

    internal RocksDBFileDataBucket? FindBucket(String fileId, bool creatIfNotExist = true)
    {
        if (IsValid(fileId) == false) return null;

        if (bucket != null) return bucket;

        var bucketId = String.Empty;
        if (creatIfNotExist == false && RocksDBFileDataBucket.IsBucketExists(BaseDir, bucketId) == false)
            return null;

        lock(this)
        {
            if(bucket == null)
                bucket = new RocksDBFileDataBucket(BaseDir, bucketId, this);
        }

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
        _cache = null;
    }
}

/// <summary>
/// 本地文件存储系统。对于小文件，存储在 rocksdb 中，大文件，存储在本地文件系统里面。均按照日期分片存储。
/// </summary>
public class LocalFileStorageService
{
    private RocksDBFileStorageService rocksDBFileStorageService;
    private LocalDiskFileStorageService localDiskFileStorageService;

    public String NextFileId(String fileExtention = "")
    {
        return Guid.NewGuid().ToString("N") + fileExtention ?? String.Empty;
    }

    private int MaxBytesSaveInRocksDB = 16 * 1024 * 1024;

    public LocalFileStorageService(int maxMBytesSaveInRocksDB = 16, string? baseDir = null)
    {
        MaxBytesSaveInRocksDB = Math.Max(0, maxMBytesSaveInRocksDB) * 1024 * 1024;
        rocksDBFileStorageService = new RocksDBFileStorageService(baseDir);
        localDiskFileStorageService = new LocalDiskFileStorageService(baseDir);
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