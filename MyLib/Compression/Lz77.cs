using MyLib.Compression.Interface;

namespace MyLib.Compression;

public class Lz77 : ICompressionAlgorithm
{
    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        throw new NotImplementedException();
    }
}