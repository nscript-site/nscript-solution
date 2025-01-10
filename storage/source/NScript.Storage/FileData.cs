using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.Storage;

public class FileData
{
    public String FileId { get; set; }
    public byte[] Data { get; set; }
    public long Length { get; set; }
}