using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB;

/// <summary>
/// 按时间分片存储。
/// </summary>
public class ShardingOnTimeDataService<TEntity> : DataService<TEntity> where TEntity : class
{
    private string DataBaseName { get; set; } = "sharding";

    public ShardingOnTimeDataService(string databaseName, ShardingOnTimeStrategy strategy, DateTime time) 
        : base(strategy.GetBucketId(ref time))
    {
        DataBaseName = databaseName + strategy switch
        {
            ShardingOnTimeStrategy.ByDay => "#d",
            ShardingOnTimeStrategy.ByMonth => "#m",
            ShardingOnTimeStrategy.ByYear => "#y",
            _ => ""
        };
        if (DataBaseName.EndsWith(".db") == false) DataBaseName += ".db";

        bool inited = false;

        var typeInfo = typeof(TEntity);
        var attributes = typeInfo.GetCustomAttributes(true);
        foreach (var item in attributes)
        {
            if (item is LiteDBSetAttribute liteDBAtt)
            {
                String fileName = liteDBAtt.FileName;
                if (fileName != null) fileName = fileName.Trim();
                if (String.IsNullOrEmpty(fileName)) fileName = typeInfo.Name;
                if (fileName.EndsWith(".db") == true) fileName = fileName.Substring(0, fileName.Length - 3);
                
                this.CollectionName = fileName;
                inited = true;
                break;
            }
        }

        if(inited == false)
        {
            this.CollectionName = typeInfo.Name;
        }
    }

    protected override string GetDBName()
    {
        return DataBaseName;
    }
}
