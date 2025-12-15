namespace HugeTextProcessing.Generating.Benchmarks;
public readonly record struct SourceConfig(string Name, IEnumerable<string> Data)
{
    public override string ToString() => Name;
}
