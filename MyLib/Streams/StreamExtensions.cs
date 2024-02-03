using System.Text;
using MyLib.Bytes;

namespace MyLib.Streams;

public static class StreamExtensions
{
    private static void InternalWrite<T>(this Stream stream, T value) where T : struct
    {
        var bytes = value.ToBytes();
        if (BitConverter.IsLittleEndian)
        {
            bytes = bytes.Reverse().ToArray();
        }
        stream.Write(bytes);
    }
    
    public static ushort ReadU16(this Stream stream, bool skipEndianCheck = false)
    {
        var bytes = stream.ReadBytes(2);
        if (BitConverter.IsLittleEndian && !skipEndianCheck)
        {
            bytes = bytes.Reverse().ToArray();
        }
        return BitConverter.ToUInt16(bytes);
    }
    
    public static void WriteU16(this Stream stream, ushort value)
    {
        stream.InternalWrite(value);
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
    
    public static void WriteU32(this Stream stream, uint value)
    {
        stream.InternalWrite(value);
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
    
    public static void WriteS16(this Stream stream, short value)
    {
        stream.InternalWrite(value);
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
    
    public static void WriteS32(this Stream stream, int value)
    {
        stream.InternalWrite(value);
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
    
    public static byte[] ReadBytesAt(this Stream stream, long fileOffset, int length)
    {
        var buffer = new byte[length];
        stream.Seek(fileOffset, SeekOrigin.Begin);
        var bytesRead = stream.Read(buffer, 0, length);
        if (bytesRead != length)
            throw new IndexOutOfRangeException();
        return buffer;
    }
    
    public static string ReadString(this Stream stream, int length)
    {
        var bytes = stream.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }
    
    public static void WriteString(this Stream stream, string value)
    {
        stream.Write(Encoding.UTF8.GetBytes(value));
    }
}