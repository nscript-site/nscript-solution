using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB
{
    public class DataService<TEntity> : BaseDataService where TEntity : class
    {
        protected String CollectionName { get; set; } = "default";

        protected readonly string _bucketName;

        public DataService(string bucketName = null)
        {
            _bucketName = bucketName;
        }

        private string GetBucketName()
        {
            return String.IsNullOrEmpty(_bucketName)?String.Empty: _bucketName;
        }

        protected override string DBName
        {
            get
            {
                if (_dbName == null)
                {
                    String dbName = GetDBName();
                    String bucketName = GetBucketName();
                    if(String.IsNullOrEmpty(bucketName) == false)
                    {
                        if (dbName.EndsWith(".db"))
                            dbName = dbName.Substring(0, dbName.Length - 3) + "#" + _bucketName + ".db";
                        else
                            dbName += "#" + _bucketName;
                    }
                    _dbName = dbName;
                }
                return _dbName;
            }
        }

        private string? _dbName;

        private string GetDBName()
        {
            var typeInfo = typeof(TEntity);
            var attributes = typeInfo.GetCustomAttributes(true);
            foreach (var item in attributes)
            {
                if (item is LiteDBSetAttribute liteDBAtt)
                {
                    String fileName = liteDBAtt.FileName;
                    if (fileName != null) fileName = fileName.Trim();
                    if (String.IsNullOrEmpty(fileName) == false)
                    {
                        if (fileName.EndsWith(".db") == false) fileName += ".db";
                        return fileName;
                    }
                }
            }
            return typeof(TEntity).FullName + ".db";
        }

        /// <summary>
        /// 计算所包含的数据量
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            int count = 0;
            UsingCollection( c => count = c.Count() );
            return count;
        }

        /// <summary>
        /// 如果需要设置索引，可以重写本方法
        /// </summary>
        /// <param name="col"></param>
        protected virtual void EnsureIndex(ILiteCollection<TEntity> col)
        {
            GetIndexer().EnsureIndex(col);
        }

        private Indexer<TEntity> GetIndexer()
        {
            if (_indexer == null) _indexer = CreateIndexer();
            return _indexer;
        }

        private Indexer<TEntity>? _indexer;

        private Indexer<TEntity> CreateIndexer()
        {
            var typeInfo = typeof(TEntity);
            var attributes = typeInfo.GetCustomAttributes(true);
            foreach (var item in attributes)
            {
                if (item is LiteDBSetAttribute liteDBAtt)
                {
                    Type? indexerType = liteDBAtt.Indexer;
                    if(indexerType != null)
                    {
                        object obj = Activator.CreateInstance(indexerType)!;
                        if (obj is Indexer<TEntity> indexer)
                            return indexer;
                    }
                }
            }
            return new Indexer<TEntity>();
        }

        public void UsingCollection(Action<ILiteCollection<TEntity>> onCollection)
        {
            if (CollectionName == null || onCollection == null) return;
            using (var db = new LiteDatabase(GetConnString()))
            {
                var col = db.GetCollection<TEntity>(CollectionName);
                onCollection(col);
            }
        }

        internal TRtn Query<TRtn>(Func<ILiteCollection<TEntity>,TRtn> func) where TRtn : class
        {
            TRtn rtn = null;
            UsingCollection(col =>
            {
                rtn = func(col);
            });
            return rtn;
        }

        public TEntity FindOne(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity rtn = null;
            UsingCollection(c => {
                rtn = c.FindOne(predicate);
            });
            return rtn;
        }

        public bool UpdateOne(Expression<Func<TEntity, bool>> predicate, Action<TEntity> onUpdate)
        {
            if (onUpdate == null || predicate == null) return false;

            TEntity match = null;
            UsingCollection(c => {
                match = c.FindOne(predicate);
                if(match != null)
                {
                    onUpdate(match);
                    c.Update(match);
                }
            });
            return match != null;
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> rtn = null;
            UsingCollection(c =>
            {
                rtn = c.Find(predicate);
            });
            return rtn;
        }

        /// <summary>
        /// 是否是本次程序运行以来第一次插入
        /// </summary>
        private bool IsFirstInsert = true;

        public void Insert(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            UsingCollection(
                col =>
                {
                    col.Insert(entity);
                    if(IsFirstInsert == true)
                    {
                        IsFirstInsert = false;
                        EnsureIndex(col);
                    }
                }
            );
        }

        public void Insert(BsonValue bsonId, TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            UsingCollection(
                col =>
                {
                    col.Insert(bsonId, entity);
                    if (IsFirstInsert == true)
                    {
                        IsFirstInsert = false;
                        EnsureIndex(col);
                    }
                }
            );
        }

        public void Insert(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            UsingCollection(
                col =>
                {
                    col.Insert(entities);
                    if (IsFirstInsert == true)
                    {
                        IsFirstInsert = false;
                        EnsureIndex(col);
                    }
                }
            );
        }

        public bool Delete(BsonValue bsonId)
        {
            var rtn = false;
            UsingCollection(
                col =>
                {
                    rtn = col.Delete(bsonId);
                }
            );
            return rtn;
        }
    }
}
