

using NScript.LiteDB;
using NScript.LiteDB.Utils.Test.Data;

void TestTypedDataService()
{
    var service1 = new DataService<Book>();
    var service2 = new DataService<DefaultBook>();
    service1.Insert(new Book() { Name = "book1" });
    service2.Insert(new DefaultBook() { Name = "book2" });
    Console.WriteLine(service1.Count());
    Console.WriteLine(service2.Count());
}

TestTypedDataService();
