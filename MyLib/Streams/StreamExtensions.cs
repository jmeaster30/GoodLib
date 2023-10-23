using System.Text;

namespace MyLib.Streams;

public static class StreamExtensions
{
    public static ushort ReadU16(this Stream stream, bool skipEndianCheck = false)
    {
        var bytes = stream.ReadBytes(2);
        if (BitConverter.IsLittleEndian && !skipEndianCheck)
        {
            bytes = bytes.Reverse().ToArray();
        }
        return BitConverter.ToUInt16(bytes);
    }
    
    public static uint ReadU32(this Stream stream, bool skipEndianCheck = false)
    {
        var bytes = stream.ReadBytes(4);
        if (BitConverter.IsLittleEndian && !skipEndianCheck)
        {
            bytes = bytes.Reverse().ToArray();
        }
        return BitConverter.ToUInt32(bytes);
    }
    
    public static short ReadS16(this Stream stream, bool skipEndianCheck = false)
    {
        var bytes = stream.ReadBytes(2);
        if (BitConverter.IsLittleEndian && !skipEndianCheck)
        {
            bytes = bytes.Reverse().ToArray();
        }
        return BitConverter.ToInt16(bytes);
    }
    
    public static int ReadS32(this Stream stream, bool skipEndianCheck = false)
    {
        var bytes = stream.ReadBytes(4);
        if (BitConverter.IsLittleEndian && !skipEndianCheck)
        {
            bytes = bytes.Reverse().ToArray();
        }
        return BitConverter.ToInt32(bytes);
    }

    public static byte ReadByte(this Stream stream)
    {
        return stream.ReadBytes(1)[0];
    }

    public static byte[] ReadBytes(this Stream stream, int length)
    {
        var bytes = new byte[length];
        var bytesRead = stream.Read(bytes);
        if (bytesRead != length) throw new IndexOutOfRangeException();
        return bytes;
    }

    public static string ReadString(this Stream stream, int length)
    {
        var bytes = stream.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }
}