using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB.Services
{
    public class FileData
    {
        public String FileId { get; set; }
        public byte[] Data { get; set; }
        public long Length { get; set; }
    }

    public class FileDataBucket : DataService<FileData>
    {
        protected override string DBName { get; } = "service_files";

        protected override void EnsureIndex(ILiteCollection<FileData> col)
        {
            col.EnsureIndex(x => x.FileId);
        }

        protected override String GetBaseDir()
        {
            return GetSubDir("sys_services_files", true);
        }

        public FileDataBucket(String bucketName)
        {
            this.DBName = "files_bucket_" + bucketName + ".db";
        }
    }

    /// <summary>
    /// 本地文件存储
    /// </summary>
    public class FileStorageService
    {
        public static String NextFileId(String fileExtention = "")
        {
            DateTime now = DateTime.Now;
            String bucket = now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0');
            String id = Guid.NewGuid().ToString("N");
            return bucket + id + (fileExtention ?? "");
        }

        internal static FileDataBucket FindBucket(String fileId)
        {
            if (fileId == null || fileId.Length < 10) return null;
            String bucketId = fileId.Substring(0, 8);
            FileDataBucket bucket = new FileDataBucket(bucketId);
            return bucket;
        }

        internal static bool Save(String fileId, Byte[] data)
        {
            if (data == null) return false;
            FileDataBucket bucket = FindBucket(fileId);
            if (bucket == null) throw new ArgumentException("fileId is not valid");
            bucket.Insert(new FileData() { FileId = fileId, Data = data, Length = data.LongLength });
            return true;
        }

        internal static byte[]? Find(String fileId)
        {
            FileDataBucket bucket = FindBucket(fileId);
            if (bucket == null) throw new ArgumentException("fileId is not valid");
            if (bucket.Exists() == false) return null;
            var find = bucket.FindOne(item => item.FileId == fileId);
            return find?.Data ?? null;
        }
    }
}
