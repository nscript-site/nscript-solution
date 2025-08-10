using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNSW;

public class HNSWIndexData
{
    public byte[] Body { get; set; } = Array.Empty<byte>();

    public List<byte[]> ItemSlices { get; set; } = new List<byte[]>();

    public List<byte[]> NodeSlices { get; set; } = new List<byte[]>();
}
