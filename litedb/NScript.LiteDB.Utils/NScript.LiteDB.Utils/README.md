# NScript.LiteDB.Utils

在 LiteDB 基础上，提供了 DataService<T> 泛型类，方便进行操作: 

```csharp
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

var service1 = new DataService<Book>();
var service2 = new DataService<DefaultBook>();
var service3 = new DataService<Book>("bucketA");
service1.Insert(new Book() { Name = "book1" });
service2.Insert(new DefaultBook() { Name = "book2" });
service3.Insert(new Book() { Name = "book3" });
Console.WriteLine(service1.Count());
Console.WriteLine(service2.Count());
Console.WriteLine(service3.Count());

```

提供了接口 `IFileStorageService`，在 LiteDB 基础上，提供了 `FileStorageService`，可以根据日期，将文件分片存储。

`IFileStorageService` 接口定义为：

```csharp
public interface IFileStorageService
{
    /// <summary>
    /// 生成下一个文件的 Id。文件名称为 8字符日期+UUID+扩展名
    /// </summary>
    /// <param name="fileExtention"></param>
    /// <returns></returns>
    public string NextFileId(String fileExtention = "");

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public bool Delete(String fileId);

    /// <summary>
    /// 保存文件，返回文件 fileId
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileExtention"></param>
    /// <returns></returns>
    public String Save(Byte[] data, String fileExtention);

    /// <summary>
    /// 保存文件到指定 fileId
    /// </summary>
    /// <param name="fileId"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Save(String fileId, Byte[] data);

    /// <summary>
    /// 根据文件Id查找文件
    /// </summary>
    /// <param name="fileId"></param>
    /// <returns></returns>
    public byte[]? Find(String fileId);
}
```

相关操作:

```csharp
void TestFileStorageService(String? dir = null)
{
    var storage = new FileStorageService();
    if (dir != null) storage.BaseDir = dir;

    byte[] data = new byte[10];
    var fileId = storage.Save(data, ".dat");
    Console.WriteLine(fileId);
    var item = storage.Find(fileId);
    Console.WriteLine(item.Length);
    var rtn = storage.Delete(fileId);
    Console.WriteLine(rtn);
    item = storage.Find(fileId);
    if (item == null) Console.WriteLine("Delete OK!");
}
```