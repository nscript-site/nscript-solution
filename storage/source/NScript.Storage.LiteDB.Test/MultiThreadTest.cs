using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage.LiteDB.Test;

[TestClass]
public sealed class MultiThreadTest
{
    [TestMethod]
    public void TestSingleFileDataService()
    {
        var time = DateTime.Now.ToFileTime();
        var service1 = new SingleFileDataService<Book>($"single_{time}", "storage_single");
        service1.Insert(new Book() { Name = "book1", Id = Guid.NewGuid().ToString() });
        var service2 = new SingleFileDataService<Book2>($"single_{time}", "storage_single");
        service2.Insert(new Book2() { Name = "book2", Id = Guid.NewGuid().ToString() });

        bool ok = true;
        var name = "";
        Task.Run(() => {
            try
            {
                service1.UpdateOne(x => x.Name == "book1", x =>
                {
                    var find = service2.FindOne(x => x.Name == "book2");
                    x.Name = find.Name;
                    name = x.Name;
                });
            }
            catch (Exception ex)
            {
                ok = false;
            }
        }).Wait();
        Assert.IsTrue(ok);
        Assert.AreEqual("book2",name);
    }
}
