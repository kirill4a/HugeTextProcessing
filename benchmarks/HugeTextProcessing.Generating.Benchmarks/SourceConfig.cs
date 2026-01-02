using HugeTextProcessing.Abstractions;

namespace HugeTextProcessing.Benchmarks;
public readonly record struct SourceConfig(string Name, IEnumerable<Line> Data)
{
    public override string ToString() => Name;
}
