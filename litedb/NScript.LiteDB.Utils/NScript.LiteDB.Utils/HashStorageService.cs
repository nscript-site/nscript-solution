using LiteDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB.Services
{
    /// <summary>
    /// 可散列的数据
    /// </summary>
    public interface IDataHashable
    {
        public String Id { get; set; }
        public String BucketId { get; set; }
    }

    internal class HashStorageBucket<T> : DataService<T>
        where T : class, IDataHashable 
    {
        protected override string DBName { get; } = "service_hashstorage_" + typeof(T).FullName.ToString();
        protected override void EnsureIndex(ILiteCollection<T> col)
        {
            col.EnsureIndex(x => x.Id);
        }

        protected override String GetBaseDir()
        {
            return GetSubDir("sys_hashdata/" + HashStorageName, true);
        }

        protected string HashStorageName
        {
            get
            {
                if (_hashStorageName == null) _hashStorageName = GetHashStorageName();
                return _hashStorageName;
            }
        }

        private string? _hashStorageName;

        private string GetHashStorageName()
        {
            var typeInfo = typeof(T);
            var attributes = typeInfo.GetCustomAttributes(true);
            foreach (var item in attributes)
            {
                if (item is LiteDBSetAttribute liteDBAtt)
                {
                    String storageName = liteDBAtt.HashStorageName;
                    if (storageName != null) storageName = storageName.Trim();
                    if (String.IsNullOrEmpty(storageName) == false)
                    {
                        return storageName;
                    }
                }
            }
            return typeof(T).FullName!.ToLower();
        }

        public HashStorageBucket(String bucketName)
        {
            this.DBName = "hash_bucket_" + bucketName + ".db";
        }
    }

    /// <summary>
    /// 散列数据存储服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashStorageService<T> where T : class, IDataHashable
    {
        internal static HashStorageBucket<T> FindBucket(String? bucketId)
        {
            if (bucketId == null) return default(HashStorageBucket<T>);

            var bucket = new HashStorageBucket<T>(bucketId);
            return bucket;
        }

        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool Save(T val)
        {
            if (val == null) return false;

            var bucket = FindBucket(val.BucketId);

            if (bucket == null) throw new ArgumentException("BucketId is not valid");

            bucket.Insert(val);

            return true;
        }

        /// <summary>
        /// 查找数据
        /// </summary>
        /// <param name="bucketId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T? Find(String bucketId, String id)
        {
            if (bucketId == null || id == null) return null;

            var bucket = FindBucket(bucketId);
            if (bucket == null) throw new ArgumentException("BucketId is not valid");

            if (bucket.Exists() == false) return null;

            return bucket.FindOne(item => item.Id == id);
        }
    }
}
