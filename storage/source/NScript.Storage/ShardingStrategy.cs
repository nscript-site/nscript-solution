using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage;

public enum ShardingStrategy
{
    ByDay,
    ByMonth,
    ByYear
}

public static class ShardingStrategy_ClassHelper
{
    public static string GetShardingId(this ShardingStrategy BucketStrategy, ref DateTime now)
    {
        if (BucketStrategy == ShardingStrategy.ByDay)
            return now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0');
        else if (BucketStrategy == ShardingStrategy.ByMonth)
            return now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + "00";
        else
            return now.Year.ToString() + "0000";
    }

    public static String NextFileId(this ShardingStrategy BucketStrategy, DateTime? time, String fileExtention = "")
    {
        DateTime timeVal = time ?? DateTime.Now;
        String bucket = BucketStrategy.GetShardingId(ref timeVal);
        String id = Guid.NewGuid().ToString("N");
        if (String.IsNullOrEmpty(fileExtention)) return bucket + id;
        if (fileExtention.StartsWith('.') == false) fileExtention = '.' + fileExtention;
        return bucket + id + fileExtention;
    }
}
