using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace NScript.Storage.LiteDB;

public abstract class BaseDataService
{
    protected String DefaultBaseDir
    {
        get {

            LiteDBSetting setting = this.DBSetting ?? LiteDBSetting.DefaultSetting.Value;
            string path = setting.DataDirectory;
            // 检查目录是否存在，若不存在，则创建它
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists == false) dirInfo.Create();
            return path;
        }
    }

    protected string? _baseDir;

    protected void SetBaseDir(String? baseDir)
    {
        if (baseDir == null) return;

        var dirInfo = new DirectoryInfo(baseDir);
        if (dirInfo.Exists == false) dirInfo.Create();
        _baseDir = baseDir;
    }

    public LiteDBSetting? DBSetting { get; set; }

    protected abstract String DBName { get; }

    protected virtual String GetBaseDir()
    {
        if(_baseDir == null)
        {
            _baseDir = DefaultBaseDir;
        }

        return _baseDir;
    }

    public bool Exists()
    {
        return File.Exists(Path.Combine(GetBaseDir(), DBName));
    }

    //protected String GetSubDir(String dirName, bool createIfNotExists = false)
    //{
    //    return GetSubDir(GetBaseDir(), dirName, createIfNotExists);
    //}

    //protected String GetSubDir(String baseDir, String dirName, bool createIfNotExists = false)
    //{
    //    if (dirName == null) throw new ArgumentNullException(nameof(dirName));

    //    String path = Path.Combine(baseDir, dirName);

    //    if(createIfNotExists == true)
    //    {
    //        DirectoryInfo dirInfo = new DirectoryInfo(path);
    //        if (dirInfo.Exists == false) dirInfo.Create();
    //    }

    //    return path;
    //}

    protected String GetConnString()
    {
        return "Filename=" + Path.Combine(GetBaseDir(), DBName) + ";Connection=shared";
    }

    protected void UsingDB(Action<LiteDatabase> onDatabase)
    {
        if (onDatabase == null) return;
        using (var db = new LiteDatabase(GetConnString()))
        {
            onDatabase(db);
        }
    }

    protected void UsingCollection<T>(String collectionName, Action<ILiteCollection<T>> onCollection)
    {
        if (collectionName == null || onCollection == null) return;
        using (var db = new LiteDatabase(GetConnString()))
        {
            var col = db.GetCollection<T>(collectionName);
            onCollection(col);
        }
    }
}
