using System.Text;
using MyLib.Compression;
using MyLib.Enumerables;

var algorithm = new Lz77
{
    LookaheadSize = 4,
    DictionarySize = 6
};

var value = "aacaacabcabaaac";

var bytes = Encoding.ASCII.GetBytes(value);
var encoded  = algorithm.Encode(bytes);
var decoded = algorithm.Decode(encoded);
var (diffOffset, leftDiff, rightDiff) = bytes.FirstDifference(decoded);
Console.WriteLine($"Size: {bytes.Count()} Encoded Size: {encoded.Count()} Ratio {(float)encoded.Count() / (float)decoded.Count()}");
Console.WriteLine($"Diff {diffOffset} Left Diff {leftDiff} Right Diff {rightDiff}");