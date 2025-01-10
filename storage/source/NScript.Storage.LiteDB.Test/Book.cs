using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage.LiteDB.Test;

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

    public String Id { get; set; }

    public String Name { get; set; }
}

[LiteDBSet(FileName = "books2.db", Indexer = typeof(Book2Indexer))]
public class Book2
{
    public class Book2Indexer : Indexer<Book2>
    {
        public override void EnsureIndex(ILiteCollection<Book2> col)
        {
            col.EnsureIndex(x => x.Name);
        }
    }

    public String Id { get; set; }

    public String Name { get; set; }
}