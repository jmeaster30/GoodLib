using System.Text;
using MyLib.Compression.Interface;
using MyLib.Enumerables;
using Xunit;

namespace MyLib.Test.Compression;

public static class CompressionTestHelpers
{
    // The idea is if the encoders and decoders are working properly then:
    // value == decode(encode(value))
    public static void TestFilter(ICompressionAlgorithm algorithm, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        var encoded  = algorithm.Encode(bytes);
        var decoded = algorithm.Decode(encoded);
        var (diffOffset, leftDiff, rightDiff) = bytes.FirstDifference(decoded);
        Assert.True(diffOffset == -1, $"The encoder + decoder didn't produce the expected output at byte offset {diffOffset}. {leftDiff} != {rightDiff}");
    }
}