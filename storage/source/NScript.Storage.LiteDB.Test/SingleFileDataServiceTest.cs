namespace NScript.Storage.LiteDB.Test;

[TestClass]
public sealed class SingleFileDataServiceTest
{
    [TestMethod]
    public void TestCRUD()
    {
        var time = DateTime.Now.ToFileTime();
        var service1 = new SingleFileDataService<Book>($"single_{time}","storage_single");
        service1.Insert(new Book() { Name = "book1", Id = Guid.NewGuid().ToString() });
        var service2 = new SingleFileDataService<Book2>($"single_{time}", "storage_single");
        service2.Insert(new Book2() { Name = "book2", Id = Guid.NewGuid().ToString() });
        var find = service1.FindOne(x => x.Name == "book1");
        bool deleteResult = service1.Delete(find.Id);
        var find2 = service2.FindOne(x => x.Name == "book2");
        bool deleteResult2 = service2.Delete(find2.Id);
        Assert.IsTrue(deleteResult);
        Assert.IsTrue(deleteResult2);
    }
}
