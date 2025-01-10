using NScript.LiteDB.Services;

namespace NScript.LiteDB.Utils.UnitTest
{
    [TestClass]
    public sealed class LocalShardingOnTimeFileStorageServiceTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestBaseDir()
        {
            var dir = new DirectoryInfo(DateTime.Now.ToFileTime().ToString());

            TestContext.WriteLine("!!");

            var service = new LocalShardingOnTimeFileStorageService(ShardingOnTimeStrategy.ByDay,16,2,dir.FullName);
            var bytes1 = new byte[1024];
            bytes1[0] = 1;
            var bytes2 = new byte[1024*1024*24];
            bytes2[0] = 2;

            var fileId1 = service.NextFileId();
            var fileId2 = service.NextFileId();

            service.Save(fileId1, bytes1);
            service.Save(fileId2, bytes2);

            bytes1 = service.Find(fileId1);
            bytes2 = service.Find(fileId2);

            Assert.IsNotNull(bytes1);
            Assert.IsNotNull(bytes2);
            Assert.AreEqual(1024, bytes1.Length);
            Assert.AreEqual(1024*1024*24, bytes2.Length);
            Assert.AreEqual(1, bytes1[0]);
            Assert.AreEqual(2, bytes2[0]);
            Assert.AreEqual(true, Directory.Exists(Path.Combine(dir.FullName, "large_files")));
        }
    }
}
