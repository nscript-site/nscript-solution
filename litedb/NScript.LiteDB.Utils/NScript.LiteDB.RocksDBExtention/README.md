# NScript.LiteDB.RocksDBExtention

本库是 `NScript.LiteDB.Utils` 的扩展。LiteDB 存大量的文件数据时，性能比较底下，本扩展在 RocksDB 之上，提供了 `RocksDBFileStorageService` 类，实现了接口 `RocksDBFileStorageService`，提供了和 `NScript.LiteDB.Utils` 中 `FileStorageService` 相似的方式来存储文件。

可以指定存储的根目录。IFileStorageService 将在根目录下，根据文件名称的前 8 个字符，寻找相应的桶，进行相关的操作。通过 NextFileId 方法可以生成 [8字符日期+UUID+扩展名] 格式的文件名，这样存储时将按照日期分片保存。 

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
void TestRocksDBFileStorageService(String? dir = null)
{
    var storage = new RocksDBFileStorageService();
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