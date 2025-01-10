# NScript.Storage.LiteDB

提供基于 LiteDB 的操作类，方便进行数据库操作，适合实体与实体之间没有复杂关联关系的数据存储。

## 数据建模

数据建模示例如下，其中，Id 项以 guid 方式存储主键。BookIndexer 的 EnsureIndex 约定了索引项。FileName 项元数据规定了该类实体存储的数据库文件（根据存储策略的不同，也可能是 collection）的名称。

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

```

## 存储策略

提供了三类存储策略：

- SingleFileDataService: 所有的实体类存储在一个数据库文件中。实体类存储在 FileName 元数据所指定的 collection 中。
- ShardingSingleFileDataService: 所有的实体类分片存储，每片存储在一个数据库文件中。实体类存储在对应分片的 FileName 元数据所指定的 collection 中。
- MultiFileDataService：每个实体类单独存储在一个数据库文件中。文件名称为 FileName 元数据所指定的名称。

### SingleFileDataService 示例

```csharp
var time = DateTime.Now.ToFileTime();
var service1 = new SingleFileDataService<Book>($"single_{time}","storage_single");
service1.Insert(new Book() { Name = "book1", Id = Guid.NewGuid().ToString() });
var service2 = new SingleFileDataService<Book2>($"single_{time}", "storage_single");
service2.Insert(new Book2() { Name = "book2", Id = Guid.NewGuid().ToString() });
var find = service1.FindOne(x => x.Name == "book1");
bool deleteResult = service1.Delete(find.Id);
var find2 = service2.FindOne(x => x.Name == "book2");
bool deleteResult2 = service2.Delete(find2.Id);
```

在 `storage_single` 目录下可以看到文件：

```bash
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---           2025/1/10    17:15          73728 single_133809741190933375.db
```

数据都存储在该数据库文件中。

### ShardingSingleFileDataService 示例

```csharp
var now = DateTime.Now;
var time = now.ToFileTime();
var service1 = new ShardingSingleFileDataService<Book>($"sharding_{time}", ShardingStrategy.ByDay, now, "storage_sharding");
service1.Insert(new Book() { Name = "book1", Id = Guid.NewGuid().ToString() });
var service2 = new ShardingSingleFileDataService<Book2>($"sharding_{time}", ShardingStrategy.ByDay, now, "storage_sharding");
service2.Insert(new Book2() { Name = "book2", Id = Guid.NewGuid().ToString() });
var find = service1.FindOne(x => x.Name == "book1");
bool deleteResult = service1.Delete(find.Id);
var find2 = service2.FindOne(x => x.Name == "book2");
bool deleteResult2 = service2.Delete(find2.Id);
```

在 `storage_sharding` 目录下可以看到文件：

```bash
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---           2025/1/10    16:46          73728 sharding_133809723850606572#d#20250110.db
```

数据都存储在该数据库文件中。#d 表示是按天分片，20250110 表示分片id是 20250110。

### MultiFileDataService 示例

```csharp
var time = DateTime.Now.ToFileTime();
var service1 = new MultiFileDataService<Book>($"multi_{time}", "storage_multi");
service1.Insert(new Book() { Name = "book1", Id = Guid.NewGuid().ToString() });
var service2 = new MultiFileDataService<Book2>($"multi_{time}", "storage_multi");
service2.Insert(new Book2() { Name = "book2", Id = Guid.NewGuid().ToString() });
var find = service1.FindOne(x => x.Name == "book1");
bool deleteResult = service1.Delete(find.Id);
var find2 = service2.FindOne(x => x.Name == "book2");
bool deleteResult2 = service2.Delete(find2.Id);
```


在 `storage_multi` 目录下可以看到文件：

```bash
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---           2025/1/10    16:42          40960 books#multi_133809721266276099.db
-a---           2025/1/10    16:42          40960 books2#multi_133809721266276099.db
```

可以看到，不同的类的实例，放在不同的文件中。

### 数据的更新

数据更新代码示例：

```csharp
service1.UpdateOne(x => x.Name == "book1", x => { 
    // update your data
});
```

## 并发说明

LiteDB 不支持多线程访问同一个数据库文件，遇到这种情况，请自行加锁。