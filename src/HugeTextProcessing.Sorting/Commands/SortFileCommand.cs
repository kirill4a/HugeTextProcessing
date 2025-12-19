namespace HugeTextProcessing.Sorting.Commands;

public sealed record class SortFileCommand
{
    public SortFileCommand(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (System.IO.File.Exists(path) is false)
        {
            throw new ArgumentException($"File '{path}' does not exists", nameof(path));
        }

        Path = path;
    }

    /// <summary>
    /// The full path to file being created
    /// </summary>
    public string Path { get; }
}
