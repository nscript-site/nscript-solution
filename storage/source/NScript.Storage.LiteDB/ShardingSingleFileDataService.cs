using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage.LiteDB;

/// <summary>
/// 按时间分片存储。每个分片存储在单个文件中。
/// </summary>
public class ShardingSingleFileDataService<TEntity> : MultiFileDataService<TEntity> where TEntity : class
{
    private string DataBaseName { get; set; } = "sharding";

    public ShardingSingleFileDataService(string databaseName, ShardingStrategy strategy, DateTime time, string? baseDir = null) 
        : base(strategy.GetShardingId(ref time))
    {
        this.SetBaseDir(baseDir);

        DataBaseName = databaseName + strategy switch
        {
            ShardingStrategy.ByDay => "#d",
            ShardingStrategy.ByMonth => "#m",
            ShardingStrategy.ByYear => "#y",
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
