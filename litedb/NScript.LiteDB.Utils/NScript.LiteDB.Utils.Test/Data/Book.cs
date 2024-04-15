using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB.Utils.Test.Data;

[LiteDBSet(FileName = "books.db", Indexer = typeof(BookIndexer))]
public class Book
{
    public class BookIndexer : Indexer<Book>
    {
        public override void EnsureIndex(ILiteCollection<Book> col)
        {
            col.EnsureIndex(x => x.Name);
        }
    }

    public String Name { get; set; }
}

public class DefaultBook
{
    public String Name { get; set; }
}
