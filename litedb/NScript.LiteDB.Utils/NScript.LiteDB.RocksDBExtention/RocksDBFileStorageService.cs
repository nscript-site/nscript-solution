using LiteDB;
using NScript.LiteDB.Utils;
using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB.Services;

internal class RocksDBFileDataBucket
{
    protected string DBName { get; } = "service_files";

    protected string DBPath { get; }

    internal string BucketBaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

    protected String GetDir()
    {
        return GetDir(BucketBaseDir, "rocksdb_files", true);
    }

    protected String GetDir(String baseDir, String dirName, bool createIfNotExists = false)
    {
        if (dirName == null) throw new ArgumentNullException(nameof(dirName));

        String path = Path.Combine(baseDir, dirName, DBName);

        if (createIfNotExists == true)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists == false) dirInfo.Create();
        }

        return path;
    }

    public RocksDBFileDataBucket(String baseDir, String bucketName)
    {
        if (String.IsNullOrEmpty(baseDir) == false)
        {
            this.BucketBaseDir = baseDir;
        }

        this.DBName = "rocksdb_bucket_" + bucketName;
        DBPath = GetDir();
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
        var options = new DbOptions()
            .SetCreateIfMissing(true);
        try
        {
            using (var db = RocksDb.Open(options, DBPath))
            {
                onDatabase(db);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

/// <summary>
/// 本地文件存储
/// </summary>
public class RocksDBFileStorageService : IFileStorageService
{
    public String NextFileId(String fileExtention = "")
    {
        DateTime now = DateTime.Now;
        String bucket = now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0');
        String id = Guid.NewGuid().ToString("N");
        if (String.IsNullOrEmpty(fileExtention)) return bucket + id;
        if (fileExtention.StartsWith('.') == false) fileExtention = '.' + fileExtention;
        return bucket + id + fileExtention;
    }

    public string BaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

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

    internal RocksDBFileDataBucket FindBucket(String fileId)
    {
        if (fileId == null || fileId.Length < 10) return null;
        String bucketId = fileId.Substring(0, 8);
        var bucket = new RocksDBFileDataBucket(BaseDir, bucketId);
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

    protected bool SaveInternal(String fileId, Byte[] data)
    {
        if (data == null) return false;
        RocksDBFileDataBucket bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        bucket.Insert(new FileData() { FileId = fileId, Data = data, Length = data.LongLength });
        return true;
    }

    public byte[]? Find(String fileId)
    {
        RocksDBFileDataBucket bucket = FindBucket(fileId);
        if (bucket == null) throw new ArgumentException("fileId is not valid");
        if (bucket.Exists() == false) return null;
        var find = bucket.FindOne(fileId);
        return find?.Data ?? null;
    }
}