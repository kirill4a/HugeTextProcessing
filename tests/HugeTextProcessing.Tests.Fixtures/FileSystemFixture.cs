using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace HugeTextProcessing.Tests.Fixtures;
public class FileSystemFixture
{
    public FileSystemFixture()
    {
        FileSystem = new MockFileSystem();
    }

    public MockFileSystem FileSystem { get; }

    public void AddEmptyFile(string path) =>
        FileSystem.AddFile(path, new MockFileData([]));

    public void AddFile(string path, string content) =>
        FileSystem.AddFile(path, new MockFileData(content));

    public IDirectoryInfo AddTempDirectory(string? prefix = null) =>
        FileSystem.Directory.CreateTempSubdirectory(prefix);
}