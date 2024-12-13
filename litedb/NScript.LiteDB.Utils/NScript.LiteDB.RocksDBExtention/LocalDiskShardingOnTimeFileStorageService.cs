using NScript.LiteDB.Utils;
using System.Net.Sockets;

namespace NScript.LiteDB.Services;

public class LocalDiskShardingOnTimeFileStorageService : IFileStorageService
{
    public string BaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

    public ShardingOnTimeStrategy BucketStrategy { get; private set; }

    public String NextFileId(String fileExtention = "")
    {
        return BucketStrategy.NextFileId(null, fileExtention);
    }

    public String NextFileId(DateTime time, String fileExtention = "")
    {
        return BucketStrategy.NextFileId(time, fileExtention);
    }

    public LocalDiskShardingOnTimeFileStorageService(ShardingOnTimeStrategy bucketStrategy = ShardingOnTimeStrategy.ByMonth)
    {
        BucketStrategy = bucketStrategy;
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
        return GetFileData(bucket,fileId);
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
