using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage.LiteDB.Test;

[TestClass]
public sealed class ShardingSingleFileDataServiceTest
{
    [TestMethod]
    public void TestCRUD()
    {
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
        Assert.IsTrue(deleteResult);
        Assert.IsTrue(deleteResult2);
    }
}