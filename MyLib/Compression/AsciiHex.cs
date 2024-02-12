using MyLib.Compression.Interface;

namespace MyLib.Compression;

public class AsciiHex : ICompressionAlgorithm
{
    private static byte ToHex(int b)
    {
        if (b is < 0 or > 15)
            throw new ArgumentOutOfRangeException(nameof(b), b, "Value must be greater than 0 and less than 16");
        return (byte)(b < 10 ? b + 48 : b + 55);
    }

    private static byte FromHex(byte b)
    {
        return b switch
        {
            <= 57 and >= 48 => (byte) (b - 48),
            <= 70 and >= 65 => (byte) (b - 55),
            <= 102 and >= 97 => (byte) (b - 87),
            _ => throw new ArgumentException($"Unexpected value of argument: '{Convert.ToInt32(b)}'", nameof(b))
        };
    }

    private static byte FromHex(byte top, byte bottom) => (byte)((FromHex(top) << 4) + FromHex(bottom));
    
    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        return input.SelectMany(x => new []{ToHex(x / 16), ToHex(x % 16)});
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        var filtered = input.Where(x => !char.IsWhiteSpace((char)x)).ToList();
        if (filtered.Any(x => x is not ((>= 48 and <= 57) or (>= 65 and <= 70) or (>= 97 and <= 102))))
        {
            throw new ArgumentException("Sequence must contain only characters that match regex: '[0-9A-Za-z]|\\w'");
        }

        return filtered.Chunk(2).Select(x => FromHex(x[0], x.Length == 1 ? (byte) 0 : x[1]));
    }
}