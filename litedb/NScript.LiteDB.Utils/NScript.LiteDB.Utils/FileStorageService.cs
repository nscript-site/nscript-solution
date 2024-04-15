using LiteDB;
using NScript.LiteDB.Utils;
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

    internal class FileDataBucket : DataService<FileData>
    {
        protected override string DBName { get; } = "service_files";

        protected override void EnsureIndex(ILiteCollection<FileData> col)
        {
            col.EnsureIndex(x => x.FileId);
        }

        internal string BucketBaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

        protected override String GetBaseDir()
        {
            return GetSubDir(BucketBaseDir, "litedb_files", true);
        }

        public FileDataBucket(String baseDir, String bucketName)
        {
            if (String.IsNullOrEmpty(baseDir) == false)
            {
                this.BucketBaseDir = baseDir;
            }

            this.DBName = "files_bucket_" + bucketName + ".db";
        }

        public bool DeleteFile(String fileId)
        {
            if (String.IsNullOrEmpty(fileId)) return false;

            int count = 0;
            this.UsingCollection(
                x =>
                {
                    count = x.DeleteMany(item=>item.FileId == fileId);
                }
                );
            return count > 0;
        }
    }

    /// <summary>
    /// 本地文件存储
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        public string BaseDir { get; set; } = LiteDBSetting.DefaultDataDirectory;

        public String NextFileId(String fileExtention = "")
        {
            DateTime now = DateTime.Now;
            String bucket = now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0');
            String id = Guid.NewGuid().ToString("N");
            if (String.IsNullOrEmpty(fileExtention)) return bucket + id;
            if (fileExtention.StartsWith('.') == false) fileExtention = '.' + fileExtention;
            return bucket + id + fileExtention;
        }

        /// <summary>
        /// 删除指定 fileId 的文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool Delete(String fileId)
        {
            if(fileId == null) return false;
            
            var bucket = FindBucket(fileId);
            if(bucket == null) return false;
            return bucket.DeleteFile(fileId); ;
        }

        internal FileDataBucket FindBucket(String fileId)
        {
            if (fileId == null || fileId.Length < 10) return null;
            String bucketId = fileId.Substring(0, 8);
            FileDataBucket bucket = new FileDataBucket(BaseDir, bucketId);
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
            FileDataBucket bucket = FindBucket(fileId);
            if (bucket == null) throw new ArgumentException("fileId is not valid");
            bucket.Insert(new FileData() { FileId = fileId, Data = data, Length = data.LongLength });
            return true;
        }

        public byte[]? Find(String fileId)
        {
            FileDataBucket bucket = FindBucket(fileId);
            if (bucket == null) throw new ArgumentException("fileId is not valid");
            if (bucket.Exists() == false) return null;
            var find = bucket.FindOne(item => item.FileId == fileId);
            return find?.Data ?? null;
        }
    }
}
