using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB;

/// <summary>
/// 所有的内容存在一个数据文件里。每一类 TEntity 存储在单独的 collection 中, collection 可通过属性 FileName 项目设置。 
/// </summary>
public class SingleFileDataService<TEntity> : DataService<TEntity> where TEntity : class
{
    private string DataBaseName { get; set; } = "singlefile";

    public SingleFileDataService(string databaseName)
    {
        DataBaseName = databaseName;

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

        if (inited == false)
        {
            this.CollectionName = typeInfo.Name;
        }
    }

    protected override string GetDBName()
    {
        return DataBaseName;
    }
}

