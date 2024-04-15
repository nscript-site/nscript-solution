using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB.Utils.Base;

/// <summary>
/// 分片存储
/// </summary>
public class ShardingDataService<TEntity> : BaseDataService where TEntity : class
{
    public ShardingDataService()
    {
    }

    protected override string DBName => "default";

    /// <summary>
    /// 如果需要设置索引，可以重写本方法
    /// </summary>
    /// <param name="col"></param>
    protected virtual void EnsureIndex(ILiteCollection<TEntity> col)
    {
    }
}
