namespace HugeTextProcessing.Sorting.Commands;
/// <summary>
/// The command to sort text file
/// </summary>
public sealed record SortFileCommand
{
    public SortFileCommand(string sourceFilePath, string destinationFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFilePath);

        SourceFilePath = sourceFilePath;
        DestinationFilePath = destinationFilePath;
    }

    /// <summary>
    /// The full path to file being sorted
    /// </summary>
    public string SourceFilePath { get; }

    /// <summary>
    /// The full path to result sorted file
    /// </summary>
    public string DestinationFilePath { get; }
}