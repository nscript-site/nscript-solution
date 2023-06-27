using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB
{
    /// <summary>
    /// LiteDB 设置属性
    /// </summary>
    public class LiteDBSetAttribute : Attribute
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public String FileName { get; set; } = "";

        /// <summary>
        /// 索引器类型
        /// </summary>
        public Type? Indexer { get; set; } = null;

        /// <summary>
        /// 哈希存储文件名称
        /// </summary>
        public String HashStorageName { get; set; } = "";
    }
}
