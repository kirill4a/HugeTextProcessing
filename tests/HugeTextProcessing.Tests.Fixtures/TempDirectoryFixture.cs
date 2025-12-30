namespace HugeTextProcessing.Tests.Fixtures;

public class TempDirectoryFixture : IDisposable
{
    private const string TxtExtension = ".txt";
    private readonly string _tempDirectory;

    public TempDirectoryFixture()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public string GetTempFileName() => Path.Combine(_tempDirectory, $"{Guid.NewGuid()}{TxtExtension}");

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }
}
