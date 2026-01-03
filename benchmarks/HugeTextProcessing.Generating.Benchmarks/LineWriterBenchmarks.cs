using BenchmarkDotNet.Attributes;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating;
using System.IO.Abstractions;

namespace HugeTextProcessing.Benchmarks;

[MemoryDiagnoser]
public class LineWriterBenchmarks
{
    private static readonly string HugeString = new('A', 1_000);

    private readonly FileSystem _fileSystem = new();
    private FileSystemStream? _stream;
    private string? _tempDir;

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

        var tempFile = _fileSystem.Path.Combine(_tempDir!, _fileSystem.Path.GetRandomFileName());
        _stream = _fileSystem.FileStream.New(tempFile, new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Options = FileOptions.SequentialScan,
        });
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _stream?.Dispose();

        if (_fileSystem.Directory.Exists(_tempDir))
            _fileSystem.Directory.Delete(_tempDir, recursive: true);
    }

    [ParamsSource(nameof(SourceConfigs))]
    public SourceConfig Config { get; set; }

    [Benchmark]
    public void LinesWriter_Benchmark()
    {
        _stream!.Position = 0;
        LinesWriterFactory.Create().WriteAsText(_stream, Config.Data);
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
