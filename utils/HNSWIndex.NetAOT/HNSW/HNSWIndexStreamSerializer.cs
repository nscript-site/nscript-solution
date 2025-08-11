using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace HNSW;

[StructLayout(LayoutKind.Explicit)]
public struct SliceDataInfo
{
    // 0 - body, 1 - item slice, 2 - node slice
    [FieldOffset(0)]
    public byte SliceType;
    [FieldOffset(4)]
    public int Bytes;

    public void Serialize(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8]; // SliceType(1) + 3字节填充 + Bytes(4) = 8字节
        buffer[0] = SliceType;
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(4, 4), Bytes);
        stream.Write(buffer);
    }

    public static SliceDataInfo Deserialize(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8];
        int read = 0;
        while (read < 8)
        {
            int n = stream.Read(buffer.Slice(read, 8 - read));
            if (n == 0)
                throw new EndOfStreamException("Unexpected end of stream while reading SliceDataInfo.");
            read += n;
        }

        SliceDataInfo info = new SliceDataInfo
        {
            SliceType = buffer[0],
            Bytes = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(4, 4))
        };
        return info;
    }
}

public class SliceData
{
    public SliceDataInfo Info { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public class HNSWIndexStreamSerializer
{
    const string Header = "HNSWDATASLICE";

    public static void Serialize(HNSWIndexData indexData, Stream stream)
    {
        //先写头部: 32 字节
        var header = new byte[32];
        var expected = System.Text.Encoding.ASCII.GetBytes(Header);
        for (int i = 0; i < expected.Length; i++)
            header[i] = expected[i];
        stream.Write(header, 0, header.Length);

        // 获取各段数据的描述
        List<SliceData> slices = new List<SliceData>();
        slices.Add(new SliceData()
        {
            Info = new SliceDataInfo() { SliceType = 0, Bytes = indexData.Body.Length },
            Data = indexData.Body
        });
        foreach(var item in indexData.ItemSlices)
        {
            slices.Add(new SliceData()
            {
                Info = new SliceDataInfo() { SliceType = 1, Bytes = item.Length },
                Data = item
            });
        }
        foreach (var item in indexData.NodeSlices)
        {
            slices.Add(new SliceData()
            {
                Info = new SliceDataInfo() { SliceType = 2, Bytes = item.Length },
                Data = item
            });
        }

        // 序列化各段数据
        foreach(var slice in slices)
        {
            slice.Info.Serialize(stream);
            stream.Write(slice.Data);
        }
    }

    public static HNSWIndexData Deserialize(Stream stream)
    {
        // 1. 读取并校验头部
        var header = new byte[32];
        int read = 0;
        while (read < 32)
        {
            int n = stream.Read(header, read, 32 - read);
            if (n == 0)
                throw new EndOfStreamException("Unexpected end of stream while reading header.");
            read += n;
        }
        var expected = System.Text.Encoding.ASCII.GetBytes(Header);
        for (int i = 0; i < expected.Length; i++)
        {
            if (header[i] != expected[i])
                throw new InvalidDataException("Invalid header for HNSWIndexData stream.");
        }

        // 2. 依次读取 SliceDataInfo 和数据
        byte[]? body = null;
        var itemSlices = new List<byte[]>();
        var nodeSlices = new List<byte[]>();

        while (stream.Position < stream.Length)
        {
            // 2.1 读取 SliceDataInfo
            var info = SliceDataInfo.Deserialize(stream);

            // 2.2 读取数据
            var data = new byte[info.Bytes];
            int offset = 0;
            while (offset < info.Bytes)
            {
                int n = stream.Read(data, offset, info.Bytes - offset);
                if (n == 0)
                    throw new EndOfStreamException("Unexpected end of stream while reading slice data.");
                offset += n;
            }

            // 2.3 分类存储
            switch (info.SliceType)
            {
                case 0:
                    body = data;
                    break;
                case 1:
                    itemSlices.Add(data);
                    break;
                case 2:
                    nodeSlices.Add(data);
                    break;
                default:
                    throw new InvalidDataException($"Unknown SliceType: {info.SliceType}");
            }
        }

        if (body == null)
            throw new InvalidDataException("Missing body slice in stream.");

        return new HNSWIndexData
        {
            Body = body,
            ItemSlices = itemSlices,
            NodeSlices = nodeSlices
        };
    }
}
