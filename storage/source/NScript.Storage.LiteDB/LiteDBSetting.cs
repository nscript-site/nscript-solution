using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage.LiteDB;

public class LiteDBSetting
{
    public static String DefaultStorageDirectory { get; set; } = "./nstorage_litedb";

    public string DataDirectory { get; set; }

    public static Lazy<LiteDBSetting> DefaultSetting { get; private set; } = new Lazy<LiteDBSetting>(() =>
    {
        var setting = new LiteDBSetting();
        setting.DataDirectory = DefaultStorageDirectory;
        return setting;
    }, true);
}
