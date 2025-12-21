using HugeTextProcessing.Abstractions;

namespace HugeTextProcessing.Generating.Benchmarks;
public readonly record struct SourceConfig(string Name, IEnumerable<Line> Data)
{
    public override string ToString() => Name;
}
