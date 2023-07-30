using MyLib.Compression.Interface;
using MyLib.Enumerables;

namespace MyLib.Compression;

// RFC 1950 https://www.ietf.org/rfc/rfc1950.txt
public class Zlib : ICompressionAlgorithm
{
    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        var encoded = new List<byte>();
        
        encoded.Add(120);
        encoded.Add(1);

        // TODO make the 7 configurable
        var deflate = new Deflate { WindowSize = 1 << (7 + 8) };
        encoded.AddRange(deflate.Encode(input));
        return encoded;
    }

    //private static (int, int)? FindBestMatch()
    //{
    //    
    //}

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        if (input.Count() < 2) throw new ArgumentException("Unexpected length of zlib bytes", nameof(input));
        var flags = input.Take(2).ToList();

        var compressionMethod = flags[0] & 8;
        var compressionInfo = (flags[0] >> 4) & 15;
        var dictionaryFlag = (flags[1] >> 5) & 1;
        var compressionLevel = flags[1] >> 6 & 3;
        var check = flags[0] * 256 + flags[1];
        
        Console.WriteLine($"Method {compressionMethod}");
        Console.WriteLine($"CMINFO {compressionInfo}");
        Console.WriteLine($"Dictionary {dictionaryFlag}");
        Console.WriteLine($"CompressionLevel {compressionLevel}");
        Console.WriteLine($"Check Number {check} {check % 31}");
        
        if (check % 31 != 0) throw new ArgumentException("Bad zlib header", nameof(input));
        if (compressionMethod != 8)
            throw new ArgumentException("Unexpected compression method in zlib header", nameof(input));

        var deflate = new Deflate { WindowSize = 1 << (compressionInfo + 8) };
        return deflate.Decode(input.Skip(2));
    }
}