using FluentValidation;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using HugeTextProcessing.Generating;
using HugeTextProcessing.Sorting.Configuration;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace HugeTextProcessing.Sorting;

internal class FileChunker(IFileSystem fileSystem, IValidator<SortOptions> optionsValidator)
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly IValidator<SortOptions> _optionsValidator = optionsValidator;

    public async ValueTask<IDirectoryInfo> ChunkAsync(string sourceFilePath, IOptions<SortOptions> options, CancellationToken cancellationToken)
    {
        ValidateOptions(options);

        var (chunkLimit, separator) = (options.Value.ChunkLimit, options.Value.CommonSeparator);
        var tempDir = CreateTempDirectory();
        await SortChunksAsync(sourceFilePath, chunkLimit, separator, tempDir.FullName, cancellationToken);

        return tempDir;
    }

    private async ValueTask SortChunksAsync(
        string sourcePath,
        long chunkLimit,
        string separator,
        string tempDirectory,
        CancellationToken cancellationToken)
    {
        var delimiters = new Delimiters(separator);
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationToken,
        };

        ILinesWriter writer = LinesWriterFactory.Create();

        await Parallel.ForEachAsync(
            ReadChunksAsync(sourcePath, chunkLimit, delimiters, cancellationToken),
            options,
            (chunk, _) =>
            {
                CollectionsMarshal.AsSpan(chunk).Sort();

                var tempFile = _fileSystem.Path.Combine(tempDirectory, _fileSystem.Path.GetRandomFileName());
                using var stream = _fileSystem.FileStream.New(tempFile, new FileStreamOptions
                {
                    Mode = FileMode.Create,
                    Access = FileAccess.Write,
                    Options = FileOptions.SequentialScan,
                    BufferSize = 1 << 20,
                });

                //TODO: (de)serialize chunks in binary format
                writer.WriteAsText(stream, chunk);

                return ValueTask.CompletedTask;
            });
    }

    private async IAsyncEnumerable<List<Line>> ReadChunksAsync(
        string path,
        long chunkLimit,
        Delimiters delimiters,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const int chunkCapacity = 8192;
        const int bytesReservation = 64;

        var chunk = new List<Line>(chunkCapacity);
        long currentBytes = 0;

        await foreach (var textLine in _fileSystem.File.ReadLinesAsync(path, _utf8, cancellationToken))
        {
            long estimatedBytes = (long)textLine.Length * sizeof(char) + bytesReservation;

            // Ensures at least one line per chunk (handles giant lines)
            if (chunk.Count > 0
                && currentBytes + estimatedBytes >= chunkLimit)
            {
                yield return chunk;
                chunk = new List<Line>(chunkCapacity);
                currentBytes = 0;
            }

            chunk.Add(Line.Parse(textLine, delimiters));
            currentBytes += estimatedBytes;
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    private IDirectoryInfo CreateTempDirectory() => _fileSystem.Directory.CreateTempSubdirectory("file_sorter_");

    private void ValidateOptions(IOptions<SortOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _optionsValidator.ValidateAndThrow(options.Value);
    }
}
