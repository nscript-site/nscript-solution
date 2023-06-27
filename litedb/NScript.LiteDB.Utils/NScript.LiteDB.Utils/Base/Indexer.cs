using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB
{
    /// <summary>
    /// 索引器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Indexer<T> where T : class
    {
        /// <summary>
        /// 建立索引
        /// </summary>
        /// <param name="col"></param>
        public virtual void EnsureIndex(ILiteCollection<T> col)
        {
        }
    }
}
