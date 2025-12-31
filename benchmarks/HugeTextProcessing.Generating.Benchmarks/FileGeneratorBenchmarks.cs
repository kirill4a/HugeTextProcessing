using BenchmarkDotNet.Attributes;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using HugeTextProcessing.Generating.Generators;
using HugeTextProcessing.Generating.ValueObjects.Size;
using System.IO.Abstractions;

namespace HugeTextProcessing.Generating.Benchmarks;

[MemoryDiagnoser]
public class FileGeneratorBenchmarks
{
    private const long SizeLimit = 10;
    private const FileSizeKind SizeKind = FileSizeKind.MiB;
    private static readonly string HugeString = new('A', 1_000);

    private readonly FileSystem _fileSystem = new();
    private string? _tempDir;
    private GenerateFileCommand _command = null!;

    public static IEnumerable<SourceConfig> SourceConfigs =>
    [
        new SourceConfig("Small", EnumerateSource()),
        new SourceConfig("Medium", [.. Enumerable.Range(0, 1_000).SelectMany(_ => EnumerateSource().Select(x => x))]),
        new SourceConfig("Big", [.. Enumerable.Range(0, 1_000_000).SelectMany(_ => EnumerateSource().Select(x => x))]),
    ];

    [GlobalSetup]
    public void Setup()
    {
        _tempDir = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), Guid.NewGuid().ToString());
        _fileSystem.Directory.CreateDirectory(_tempDir);

        _command = new(
            path: Path.Combine(_tempDir, Guid.NewGuid() + ".txt"),
            size: FileSize.From(SizeLimit, SizeKind),
            source: Config.Data);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_fileSystem.Directory.Exists(_tempDir))
            _fileSystem.Directory.Delete(_tempDir, recursive: true);
    }

    [ParamsSource(nameof(SourceConfigs))]
    public SourceConfig Config { get; set; }

    [Benchmark]
    public void SimpleFileGenerator_Benchmark()
    {
        var generator = new SimpleFileGenerator(_fileSystem);
        generator.Execute(_command);
    }

    [Benchmark]
    public void SpanFileGenerator_Benchmark()
    {
        var generator = new SpanFileGenerator(_fileSystem);
        generator.Execute(_command);
    }

    [Benchmark]
    public async Task AsyncFileGenerator_Benchmark()
    {
        var generator = new AsyncFileGenerator(_fileSystem);
        await generator.ExecuteAsync(_command, CancellationToken.None);
    }

    private static IEnumerable<Line> EnumerateSource()
    {
        yield return StringToLine("Apple");
        yield return StringToLine("Something something something");
        yield return StringToLine("Cherry is the best");
        yield return StringToLine("Banana is yellow");
        yield return StringToLine(HugeString);
    }

    private static Line StringToLine(string value) => new(Random.Shared.Next(), value, Delimiters.Default);
}
