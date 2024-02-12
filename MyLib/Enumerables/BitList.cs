using System.Collections;
using MyLib.Math;

namespace MyLib.Enumerables;

// TODO this needs to work better the api is a little weird
public class BitList : IReadOnlyList<bool>
{
    private List<bool> Contents { get; set; } = new();
    private int Index { get; set; }

    public BitList()
    {}
    
    public BitList(IEnumerable<byte> contents)
    {
        Contents = contents.SelectMany(x =>
        {
            var res = new List<bool>();
            for (var i = 7; i >= 0; i--)
            {
                res.Add(((x >> i) & 1) == 1);
            }
            return res;
        }).ToList();
    }

    public int Count => Contents.Count;

    IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
    {
        return Contents.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return Contents.GetEnumerator();
    }

    public void AppendBits(int value, int bitLength)
    {
        for (var shift = bitLength - 1; shift >= 0; shift--)
        {
            Contents.Add(((value >> shift) & 1) == 1);
        }
    }

    public void AppendBytes(IEnumerable<byte> bytes)
    {
        foreach (var b in bytes)
        {
            AppendBits(b, 8);
        }
    }

    public void RemoveBits(int amount)
    {
        Contents.RemoveRange(Contents.Count - amount, amount);
    }

    public void ConsumeToNextByteBoundary()
    {
        Index = Index + 8 - Index % 8;
    }

    public void ConsumeBits(int count)
    {
        Index += count;
    }
    
    public byte[] PeekBits(int count)
    {
        var value = Contents.GetRange(Index, count);
        return ToByteArray(value.PadLeft((value.Count / 8.0).Ceiling() * 8, false));
    }
    
    public byte[] PeekBitsAt(int index, int count)
    {
        try
        {
            var value = Contents.GetRange(index, count);
            return ToByteArray(value.PadLeft((value.Count / 8.0).Ceiling() * 8, false));
        }
        catch
        {
            Console.WriteLine($"Count: {Contents.Count} Offset: {index} Amount: {count}");
            throw;
        }
    }
    
    public byte[] ReadBits(int count)
    {
        var value = Contents.GetRange(Index, count);
        Index += value.Count;
        return ToByteArray(value.PadLeft((value.Count / 8.0).Ceiling() * 8, false));
    }

    public byte[] ReadBytes(int count)
    {
        var value = Contents.GetRange(Index, count * 8);
        Index += value.Count;
        return ToByteArray(value); // don't need the padding since it should always be byte aligned 
    }

    public byte[] ReadBitsAt(int index, int count)
    {
        try
        {
            var value = Contents.GetRange(index, count);
            Index = index + value.Count;
            return ToByteArray(value.PadLeft((value.Count / 8.0).Ceiling() * 8, false));
        }
        catch
        {
            Console.WriteLine($"Count: {Contents.Count} Offset: {index} Amount: {count}");
            throw;
        }
    }

    public byte[] ToByteArray()
    {
        return ToByteArray(Contents);
    }
    
    private static byte[] ToByteArray(IEnumerable<bool> value)
    {
        return value.PadRight((value.Count() / 8.0).Ceiling() * 8, false)
            .Chunk(8).Select(source =>
            {
                byte result = 0;
                var index = 8 - source.Count();
                foreach (var b in source)
                {
                    if (b) result |= (byte)(1 << (7 - index));
                    index++;
                }
                return result;
            }).ToArray();
    }

    public bool this[int index] => Contents[index];
}

public static class BitListExtensions {
    public static BitList ToBitList(this IEnumerable<byte> contents)
    {
        return new BitList(contents);
    }
}