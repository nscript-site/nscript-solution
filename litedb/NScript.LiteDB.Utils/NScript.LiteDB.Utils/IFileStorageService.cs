using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.LiteDB.Utils;

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
}
