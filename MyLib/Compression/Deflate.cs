using System.Diagnostics;
using MyLib.Compression.Interface;
using MyLib.DataStructures;
using MyLib.Enumerables;
using MyLib.Math;

namespace MyLib.Compression;

// RFC 1951 https://www.ietf.org/rfc/rfc1951.txt
public class Deflate : ICompressionAlgorithm
{
    public int WindowSize { get; set; }

    private static HuffmanCodeTree _literalLengthCode = GenerateLiteralLengthCodeTree();
    private static HuffmanCodeTree _distanceCode = GenerateDistanceCodeTree();

    private static HuffmanCodeTree GenerateLiteralLengthCodeTree()
    {
        var codelens = new List<int>()
            .AddFill(0, 144, _ => 8)
            .AddFill(144, 256, _ => 9)
            .AddFill(256, 280, _ => 7)
            .AddFill(280, 288, _ => 8);
        return new HuffmanCodeTree(codelens.ToArray());
    }    
    
    private static HuffmanCodeTree GenerateDistanceCodeTree()
    {
        var codelens = new List<int>()
            .AddFill(0, 32, _ => 5);
        return new HuffmanCodeTree(codelens.ToArray());
    }    

    
    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        throw new NotImplementedException();
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        var bytes = input.ToBitList();
        var result = new BitList();
        var dictionary = new RingBuffer<byte>(32 * 1024);
        
        var lastStream = false;
        do
        {
            lastStream = bytes.ReadBits(1)[0] == 1;
            var encodingMethod = bytes.ReadBits(2)[0];
            switch (encodingMethod)
            {
                case 0:
                    DecodeUncompressedBlock(bytes, result, dictionary);
                    break;
                case 1:
                    DecodeHuffmanBlock(bytes, result, dictionary, _literalLengthCode, _distanceCode);
                    break;
                case 2:
                    var (lengthCodes, distanceCodes) = DecodeHuffmanCodes(bytes);
                    DecodeHuffmanBlock(bytes, result, dictionary, lengthCodes, distanceCodes);
                    break;
                case 3:
                    throw new InvalidDataException("Reserved block type :(");
                default:
                    throw new ArgumentException("Unreachable encoding method value");
            }
        } while (lastStream);

        return new List<byte>();
    }

    private static void DecodeUncompressedBlock(BitList bytes, BitList result, RingBuffer<byte> dictionary)
    {
        bytes.ConsumeToNextByteBoundary();

        var length = bytes.ReadBits(16).ToU16();
        var nlength = bytes.ReadBits(16).ToU16();

        if ((length ^ 0xFFFF) != nlength)
            throw new InvalidDataException("Invalid length of uncompressed block");

        var uncompressedBytes = bytes.ReadBytes(length);
        dictionary.PushMany(uncompressedBytes);
        result.AppendBytes(uncompressedBytes);
    }

    private static (HuffmanCodeTree, HuffmanCodeTree?) DecodeHuffmanCodes(BitList bytes)
    {
        var numLitLenCodes = bytes.ReadBits(5)[0] + 257;
        var numDistCodes = bytes.ReadBits(5)[0] + 1;
        
        var numCodeLenCodes = bytes.ReadBits(4)[0] + 4;
        var codeLenCodeLen = new int[19];
        codeLenCodeLen[16] = bytes.ReadBits(3)[0];
        codeLenCodeLen[17] = bytes.ReadBits(3)[0];
        codeLenCodeLen[18] = bytes.ReadBits(3)[0];
        codeLenCodeLen[ 0] = bytes.ReadBits(3)[0];
        for (var i = 0; i < numCodeLenCodes - 4; i++) {
            var j = (i % 2 == 0) ? (8 + i / 2) : (7 - i / 2);
            codeLenCodeLen[j] = bytes.ReadBits(3)[0];
        }

        var codeLenCode = new HuffmanCodeTree(codeLenCodeLen);

        var codeLens = new int[numLitLenCodes + numDistCodes];
        for (var codeLensIndex = 0; codeLensIndex < codeLens.Length; ) {
            var sym = codeLenCode.DecodeNextSymbol(bytes);
            if (sym is >= 0 and <= 15) {
                codeLens[codeLensIndex] = sym;
                codeLensIndex++;
            } else {
                int runLen;
                var runVal = 0;
                
                switch (sym)
                {
                    case 16 when codeLensIndex == 0:
                        throw new InvalidDataException("No code length value to copy");
                    case 16:
                        runLen = bytes.ReadBits(2)[0] + 3;
                        runVal = codeLens[codeLensIndex - 1];
                        break;
                    case 17:
                        runLen = bytes.ReadBits(3)[0] + 3;
                        break;
                    case 18:
                        runLen = bytes.ReadBits(7)[0] + 11;
                        break;
                    default:
                        throw new InvalidDataException("Symbol out of range");
                }
                
                var end = codeLensIndex + runLen;
                if (end > codeLens.Length)
                    throw new InvalidDataException("Run exceeds number of codes");
                
                Array.Fill(codeLens, runVal, codeLensIndex, end - codeLensIndex);
                codeLensIndex = end;
            }
        }
        
        // Create literal-length code tree
        var litLenCodeLen = new List<int>(codeLens).GetRange(0, numLitLenCodes).ToArray();
        if (litLenCodeLen[256] == 0)
            throw new InvalidDataException("End-of-block symbol has zero code length");
        var litLenCode = new HuffmanCodeTree(litLenCodeLen);
		
        // Create distance code tree with some extra processing
        var distCodeLens = codeLens.ToList().GetRange(numLitLenCodes, numDistCodes).ToArray();
        
        if (distCodeLens.Length == 1 && distCodeLens[0] == 0)
            return (litLenCode, null);  // Empty distance code; the block shall be all literal symbols
        
        // Get statistics for upcoming logic
        var oneCount = 0;
        var otherPositiveCount = 0;
        foreach (var x in distCodeLens) 
        {
            if (x == 1)
                oneCount++;
            else if (x > 1)
                otherPositiveCount++;
        }
			
        // Handle the case where only one distance code is defined
        if (oneCount == 1 && otherPositiveCount == 0) {
            // Add a dummy invalid code to make the Huffman tree complete
            distCodeLens = distCodeLens.ToList().GetRange(0, 32).ToArray();
            distCodeLens[31] = 1;
        }

        return (litLenCode, new HuffmanCodeTree(distCodeLens));
    }
    
    private static void DecodeHuffmanBlock(BitList bytes, BitList result, RingBuffer<byte> dictionary, HuffmanCodeTree literalLengthCodes, HuffmanCodeTree? distanceCodes)
    {
        while (true)
        {
            var symbol = literalLengthCodes.DecodeNextSymbol(bytes);
            if (symbol == 256)
                break;

            if (symbol < 255)
            {
                result.AppendBits(symbol, 8);
            }
            else
            {
                if (distanceCodes == null)
                    throw new InvalidDataException("Length symbol encountered with empty distance code");
                var runLength = DecodeRunLength(bytes, symbol);
                if (runLength is < 3 or > 258)
                    throw new InvalidDataException("Invalid run length");

                var distanceSymbol = distanceCodes.DecodeNextSymbol(bytes);
                var distance = DecodeDistance(bytes, distanceSymbol);
                if (distance is < 1 or > 32768)
                    throw new InvalidDataException("Invalid distance");
                
                result.AppendBytes(dictionary.GetRange(distance, runLength));
            }
        }
    }

    private static int DecodeRunLength(BitList bytes, int symbol)
    {
        switch (symbol)
        {
            case < 257 or > 287:
                throw new ArgumentException($"Invalid run length symbol: {symbol}");
            case <= 264:
                return symbol - 254;
            case <= 284:
            {
                var extraBits = (symbol - 261) / 4;
                return (((symbol - 265) % 4 + 4) << extraBits) + 3 + bytes.ReadBytes(extraBits).ToU16();
            }
            case 285:
                return 258;
            default:
                throw new InvalidDataException($"Reserved length symbol: {symbol}");
        }
    }

    private static int DecodeDistance(BitList bytes, int symbol)
    {
        switch (symbol)
        {
            case < 0 or > 32:
                throw new InvalidDataException($"Invalid distance symbol: {symbol}");
            case <= 3:
                return symbol + 1;
            case <= 29:
                var extraBits = symbol / 2 + 1;
                return ((symbol % 2 + 2) << extraBits) + 1 + bytes.ReadBytes(extraBits).ToU16();
            default:
                throw new InvalidDataException($"Reserved distance symbol: {symbol}");
        }
    }
}

internal class HuffmanCodeTree {
    const int MAX_CODE_LENGTH = 15;

    private int[] _codeBits;
    private int[] _codeValues;
    
    public HuffmanCodeTree(int[] codeLengths)
    {
        if (codeLengths.Any(x => x is > MAX_CODE_LENGTH or < 1))
            throw new ArgumentException("Maximum code length exceeded :(", nameof(codeLengths));

        var blCount = new int[MAX_CODE_LENGTH + 1];
        blCount[0] = 0;
        for (var i = 1; i <= MAX_CODE_LENGTH; i++)
        {
            blCount[i] = codeLengths.Count(x => x == i);
        }

        var nextCode = new int[MAX_CODE_LENGTH + 1];
        var code = 0;
        for (var bits = 1; bits <= MAX_CODE_LENGTH; bits++)
        {
            code = (code + blCount[bits - 1]) << 1;
            nextCode[bits] = code;
        }

        _codeBits = new int[codeLengths.Length];
        _codeValues = new int[codeLengths.Length];
        var numSymbolsAllocated = 0;
        foreach (var codeLength in codeLengths.Select((x, i) => (i, x)))
        {
            if (codeLength.Item2 == 0) continue;
            
            var startBit = 1 << codeLength.Item2;
            _codeBits[numSymbolsAllocated] = startBit | nextCode[codeLength.Item2];
            _codeValues[numSymbolsAllocated] = codeLength.Item1;
            numSymbolsAllocated += 1;
            nextCode[codeLength.Item2]++;
        }
        
        Array.Resize(ref _codeBits, numSymbolsAllocated);
        Array.Resize(ref _codeValues, numSymbolsAllocated);
    }

    public int DecodeNextSymbol(BitList bits)
    {
        if (bits == null) throw new ArgumentNullException(nameof(bits));
        
        var codeBits = 1;
        while (true) {
            codeBits = codeBits << 1 | bits.ReadBits(1)[0];
            var index = Array.BinarySearch(_codeBits, codeBits);
            if (index >= 0)
                return _codeValues[index];
        }
    }
}