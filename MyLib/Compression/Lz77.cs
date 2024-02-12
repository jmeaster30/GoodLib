using System.Text;
using MyLib.Compression.Interface;
using MyLib.Enumerables;
using MyLib.Math;

namespace MyLib.Compression;

public class Lz77 : ICompressionAlgorithm
{
    public int LookaheadSize { get; set; }
    public int DictionarySize { get; set; }

    private int OffsetBitLength => DictionarySize.BitLength();
    private int SizeBitLength => LookaheadSize.BitLength();

    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        var bytes = input.ToList();
        var encodedBytes = new BitList();
        var cursorIdx = 0;
        var dictionaryStartIdx = 0;
        while (cursorIdx < bytes.Count)
        {
            var (distance, length, nextChar) = FindMatch(bytes, dictionaryStartIdx, cursorIdx);
            encodedBytes.AppendBits(distance, OffsetBitLength);
            encodedBytes.AppendBits(length, SizeBitLength);
            encodedBytes.AppendBits(nextChar, 8);

            cursorIdx += length + 1;
            dictionaryStartIdx = (cursorIdx - DictionarySize).Max(0);
        }

        return encodedBytes.ToByteArray();
    }

    private (ushort, ushort, byte) FindMatch(List<byte> bytes, int dictionaryStartIdx, int cursorIdx)
    {
        // TODO implement a better string searching algorithm
        for (var matchSize = LookaheadSize; matchSize > 0; matchSize--)
        {
            var toMatch = bytes.Skip(cursorIdx).Take(matchSize);

            for (var dictionaryOffset = cursorIdx - 1; dictionaryOffset >= dictionaryStartIdx; dictionaryOffset--)
            {
                var searchBufferTest = bytes.Skip(dictionaryOffset).Take(matchSize);
                var (diffOffset, _, _) = toMatch.FirstDifference(searchBufferTest);
                if (diffOffset != -1) continue;
                // match
                byte nextChar = 0;
                if (cursorIdx + matchSize < bytes.Count) nextChar = bytes[cursorIdx + matchSize];
                return ((ushort)(cursorIdx - dictionaryOffset), (ushort)matchSize, nextChar);
            }
        }
        
        // no match
        return (0, 0, bytes[cursorIdx]);
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        var bytes = input.ToBitList();
        var decodedBytes = new List<byte>();
        
        var idx = 0;
        while (idx <= bytes.Count - (OffsetBitLength + SizeBitLength + 8))
        {
            var distance = bytes.ReadBitsAt(idx, OffsetBitLength).PadLeft(2, (byte)0).ToU16();
            var length = bytes.ReadBitsAt(idx + OffsetBitLength, SizeBitLength).PadLeft(2, (byte)0).ToU16();
            var lastByte = bytes.ReadBitsAt(idx + OffsetBitLength + SizeBitLength, 8).ToByte();

            if (length > 0 && distance > 0)
            {
                for (var i = 0; i < length; i++)
                    decodedBytes.Add(decodedBytes[^distance]);
            }
            
            decodedBytes.Add(lastByte);
            idx += OffsetBitLength + SizeBitLength + 8;
        }

        return decodedBytes;
    }
}