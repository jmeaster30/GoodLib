using MyLib.Compression.Interface;

namespace MyLib.Compression;

// RFC 1951 https://www.ietf.org/rfc/rfc1951.txt
public class Deflate : ICompressionAlgorithm
{
    public int WindowSize { get; set; }

    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        throw new NotImplementedException();
    }
}