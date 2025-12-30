using HugeTextProcessing.Sorting.Commands;
using HugeTextProcessing.Sorting.Configuration;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace HugeTextProcessing.Sorting;

public class FileSorter(IFileSystem fileSystem, IOptions<SortOptions> options)
{
    public async ValueTask SortAsync(SortFileCommand command, CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        FileChunker chunker = new(fileSystem, new SortOptionsValidator());
        FileMerger merger = new(fileSystem);

        var chunkDirectory = await chunker.ChunkAsync(command.SourceFilePath, options, cancellationToken);
        try
        {
            var chunks = chunkDirectory.EnumerateFiles()
                ?? throw new Exception($"Directory '{chunkDirectory.FullName}' has no files");

            merger.Merge(chunks, command.DestinationFilePath, options);
        }
        finally
        {
            if (chunkDirectory.Exists)
                chunkDirectory.Delete(recursive: true);
        }
    }

    private void ValidateCommand(SortFileCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var path = command.SourceFilePath;

        if (!fileSystem.File.Exists(path))
        {
            throw new ArgumentException($"File '{path}' does not exists");
        }
    }

}