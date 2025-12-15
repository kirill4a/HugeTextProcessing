using HugeTextProcessing.Generating.ValueObjects.Size;

namespace HugeTextProcessing.Generating.Commands;

/// <summary>
/// The command to generate text file
/// </summary>
public sealed record FileGeneratingCommand
{
    public FileGeneratingCommand(string path, FileSize size, IEnumerable<string> source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(size);
        ArgumentNullException.ThrowIfNull(source);

        if(System.IO.Path.IsPathFullyQualified(path) is false)
        {
            throw new ArgumentException("File path should be fully qualified", nameof(path));
        }
         
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Size = size ?? throw new ArgumentNullException(nameof(size));
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// The full path to file being created
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The size of file being created
    /// </summary>
    /// <remarks>
    /// The <see cref="Size"/> is the approximate size on disk, not more than specified
    /// </remarks>
    public FileSize Size { get; }

    /// <summary>
    /// The source data from which file being created
    /// </summary>
    public IEnumerable<string> Source { get; }
};
