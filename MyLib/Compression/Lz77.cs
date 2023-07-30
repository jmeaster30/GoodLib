using System.Text;
using MyLib.Compression.Interface;
using MyLib.Numbers;

namespace MyLib.Compression;

public class Lz77 : ICompressionAlgorithm
{
    public int LookaheadSize { get; set; }
    public int DictionarySize { get; set; }

    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        var bytes = input.ToList();
        var encodedBytes = new List<byte>();
        var cursorIdx = 0;
        var dictionaryStartIdx = 0;
        while (cursorIdx < bytes.Count)
        {
            var (distance, length, nextChar) = FindMatch(bytes, dictionaryStartIdx, cursorIdx);

            Console.WriteLine($"Dist {distance} Len {length} NextByte {Encoding.ASCII.GetString(new [] {nextChar})}");
            
            encodedBytes.AddRange(new[]
            {
                (byte)((distance >> 8) & 255),
                (byte)(distance & 255),
                (byte)((length >> 8) & 255),
                (byte)(length & 255),
                nextChar
            });

            cursorIdx += length + 1;
            dictionaryStartIdx = (cursorIdx - DictionarySize).Max(0);
        }

        return encodedBytes;
    }

    private (ushort, ushort, byte) FindMatch(List<byte> bytes, int dictionaryStartIdx, int cursorIdx)
    {
        // TODO implement a better string searching algorithm
        for (var searchSize = LookaheadSize.Min(bytes.Count - cursorIdx - 1); searchSize > 0; searchSize--)
        {
            for (var dictionaryOffset = cursorIdx - dictionaryStartIdx - 1; dictionaryOffset >= 0; dictionaryOffset--)
            {
                var isMatch = true;
                for (var matchIdx = 0; matchIdx < searchSize; matchIdx++)
                {
                    if (bytes[dictionaryStartIdx + dictionaryOffset + matchIdx] == bytes[cursorIdx + matchIdx])
                        continue;
                    isMatch = false;
                }

                if (isMatch)
                {
                    return ((ushort)dictionaryOffset, (ushort)searchSize, bytes[cursorIdx + searchSize]);
                }
            }
        }
        
        return (0, 0, bytes[cursorIdx]);
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        var bytes = input.ToList();
        var decodedBytes = new List<byte>();
        
        var idx = 0;
        var cursorIdx = 0;
        var dictionaryStartIdx = 0;
        while (idx <= bytes.Count - 5)
        {
            var distance = bytes[idx] * 256 + bytes[idx + 1];
            var length = bytes[idx + 2] * 256 + bytes[idx + 3];
            var lastByte = bytes[idx + 4];
            //Console.WriteLine($"Dist {distance} Len {length} LastByte {lastByte}");

            if (length == 0)
            {
                decodedBytes.Add(lastByte);
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    decodedBytes.Add(decodedBytes[dictionaryStartIdx + distance + i]);
                }
                decodedBytes.Add(lastByte);
            }

            idx += 5;
            cursorIdx += length + 1;
            dictionaryStartIdx = (cursorIdx - DictionarySize).Max(0);
        }

        return decodedBytes;
    }
}