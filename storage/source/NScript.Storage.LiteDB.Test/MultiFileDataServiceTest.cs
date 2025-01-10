using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage.LiteDB.Test;

[TestClass]
public sealed class MultiFileDataServiceTest
{
    [TestMethod]
    public void TestCRUD()
    {
        var time = DateTime.Now.ToFileTime();
        var service1 = new MultiFileDataService<Book>($"multi_{time}", "storage_multi");
        service1.Insert(new Book() { Name = "book1", Id = Guid.NewGuid().ToString() });
        var service2 = new MultiFileDataService<Book2>($"multi_{time}", "storage_multi");
        service2.Insert(new Book2() { Name = "book2", Id = Guid.NewGuid().ToString() });

        service1.UpdateOne(x => x.Name == "book1", x => { 
            // update your data
        });

        var find = service1.FindOne(x => x.Name == "book1");
        bool deleteResult = service1.Delete(find.Id);
        var find2 = service2.FindOne(x => x.Name == "book2");
        bool deleteResult2 = service2.Delete(find2.Id);
        Assert.IsTrue(deleteResult);
        Assert.IsTrue(deleteResult2);
    }
}
