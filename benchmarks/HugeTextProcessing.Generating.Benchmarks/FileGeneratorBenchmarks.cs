using BenchmarkDotNet.Attributes;
using HugeTextProcessing.Generating.Commands;
using HugeTextProcessing.Generating.Generators;
using HugeTextProcessing.Generating.ValueObjects.Size;

namespace HugeTextProcessing.Generating.Benchmarks;

[MemoryDiagnoser]
public class FileGeneratorBenchmarks
{
    const long SizeLimit = 10;
    const FileSizeKind SizeKind = FileSizeKind.MiB;

    private string? _tempDir;
    private FileGeneratingCommand _command = null!;

    public static IEnumerable<SourceConfig> SourceConfigs =>
    [
        new SourceConfig("Small", EnumerateSource()),
        new SourceConfig("Medium", [.. Enumerable.Range(0, 1_000).SelectMany(_ => EnumerateSource().Select(x => x))]),
        new SourceConfig("Big", [.. Enumerable.Range(0, 1_000_000).SelectMany(_ => EnumerateSource().Select(x => x))]),
    ];

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _command = new(
            path: Path.Combine(_tempDir, Guid.NewGuid() + ".txt"),
            size: FileSize.From(SizeLimit, SizeKind),
            source: Config.Data);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [ParamsSource(nameof(SourceConfigs))]
    public SourceConfig Config { get; set; }

    [Benchmark]
    public void SimpleFileGenerator_Benchmark()
    {
        var generator = new SimpleFileGenerator();
        generator.Execute(_command);
    }

    [Benchmark]
    public void SpanFileGenerator_Benchmark()
    {
        var generator = new SpanFileGenerator();
        generator.Execute(_command);
    }

    [Benchmark]
    public async Task AsyncFileGenerator_Benchmark()
    {
        var generator = new AsyncFileGenerator();
        await generator.ExecuteAsync(_command, CancellationToken.None);
    }

    private static IEnumerable<string> EnumerateSource()
    {
        yield return "Apple";
        yield return "Something something something";
        yield return "Cherry is the best";
        yield return "Banana is yellow";
    }
}
