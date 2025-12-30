using FluentValidation;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using HugeTextProcessing.Generating;
using HugeTextProcessing.Sorting.Configuration;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
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
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationToken,
        };


        await Parallel.ForEachAsync(
            ReadChunksAsync(sourcePath, chunkLimit, cancellationToken),
            options,
            (chunk, _) =>
            {
                var lines = chunk.Select(x => Line.Parse(x, separator)).ToArray();

                Array.Sort(lines);

                var tempFile = _fileSystem.Path.Combine(tempDirectory, _fileSystem.Path.GetRandomFileName());
                using var stream = _fileSystem.FileStream.New(tempFile, new FileStreamOptions
                {
                    Mode = FileMode.Create,
                    Access = FileAccess.Write,
                });

                ILinesWriter writer = LinesWriterFactory.Create();
                writer.WriteAsText(stream, lines);

                return ValueTask.CompletedTask;
            });
    }

    private async IAsyncEnumerable<string[]> ReadChunksAsync(
        string path,
        long chunkLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // TODO: try replace with array
        var chunk = new List<string>();
        long currentBytes = 0;

        await foreach (var textLine in _fileSystem.File.ReadLinesAsync(path, _utf8, cancellationToken))
        {
            long estimatedBytes = (long)textLine.Length * sizeof(char) + 64;

            // Ensures at least one line per chunk (handles giant lines)
            if (chunk.Count > 0
                && currentBytes + estimatedBytes >= chunkLimit)
            {
                yield return chunk.ToArray();
                ResetChunk();
            }

            chunk.Add(textLine);
            currentBytes += estimatedBytes;
        }

        yield return chunk.ToArray();

        void ResetChunk()
        {
            chunk.Clear();
            currentBytes = 0;
        }
    }

    private IDirectoryInfo CreateTempDirectory() => _fileSystem.Directory.CreateTempSubdirectory("file_sorter_");

    private void ValidateOptions(IOptions<SortOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _optionsValidator.ValidateAndThrow(options.Value);
    }
}
