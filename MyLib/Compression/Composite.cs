using MyLib.Compression.Interface;

namespace MyLib.Compression;

public class Composite : ICompressionAlgorithm
{
    private ICompressionAlgorithm[] Algorithms { get; set; }

    public Composite(params ICompressionAlgorithm[] algorithms)
    {
        Algorithms = algorithms;
    }

    public IEnumerable<byte> Encode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        return Algorithms.Aggregate(input, (current, algorithm) => algorithm.Encode(current));
    }

    public IEnumerable<byte> Decode(IEnumerable<byte> input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        return Algorithms.Reverse().Aggregate(input, (current, algorithm) => algorithm.Encode(current));
    }
}