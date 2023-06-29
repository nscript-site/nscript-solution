

using NScript.LiteDB;
using NScript.LiteDB.Utils.Test.Data;

void TestTypedDataService()
{
    var service1 = new DataService<Book>();
    var service2 = new DataService<DefaultBook>();
    var service3 = new DataService<Book>("bucketA");
    service1.Insert(new Book() { Name = "book1" });
    service2.Insert(new DefaultBook() { Name = "book2" });
    service3.Insert(new Book() { Name = "book3" });
    Console.WriteLine(service1.Count());
    Console.WriteLine(service2.Count());
    Console.WriteLine(service3.Count());
}

TestTypedDataService();
