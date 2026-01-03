using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using HugeTextProcessing.Generating;
using HugeTextProcessing.Sorting.Configuration;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace HugeTextProcessing.Sorting;
internal class FileMerger(IFileSystem fileSystem)
{
    private const int ReaderBufferSize = 1 << 20;
    private const int OutputBufferSize = 5 << 20;
    private readonly IFileSystem _fileSystem = fileSystem;

    public void Merge(
        IEnumerable<IFileInfo> inputFiles,
        string outputFilePath,
        IOptions<SortOptions> options)
    {
        ArgumentNullException.ThrowIfNull(inputFiles);
        ArgumentNullException.ThrowIfNull(options);

        var delimiters = new Delimiters(options.Value.CommonSeparator);

        var readers = new List<StreamReader>();
        var pq = new PriorityQueue<LineWithFileIndex, Line>();
        var outputBuffer = new Queue<Line>(OutputBufferSize);

        using var stream = _fileSystem.FileStream.New(outputFilePath, new FileStreamOptions
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Options = FileOptions.SequentialScan,
        });
        ILinesWriter writer = LinesWriterFactory.Create();

        try
        {
            int index = 0;
            foreach (var file in inputFiles)
            {
                var reader = CreateReader(file);
                readers.Add(reader);

                EnqueueLine(reader.ReadLine(), index);
                index++;
            }

            if (readers.Count < 1)
            {
                // TODO: log warning
                return;
            }

            while (pq.Count > 0)
            {
                if (outputBuffer.Count >= OutputBufferSize)
                {
                    FlushBuffer();
                }

                var (line, fileIndex) = pq.Dequeue();
                outputBuffer.Enqueue(line);

                var nextLine = readers[fileIndex].ReadLine();
                EnqueueLine(nextLine, fileIndex);
            }

            // final flush of the buffer
            FlushBuffer();
        }
        finally
        {
            foreach (var readre in readers)
            {
                readre.Dispose();
            }
        }

        return;

        void EnqueueLine(string? lineText, int fileIndex)
        {
            if (!string.IsNullOrEmpty(lineText))
            {
                var line = Line.Parse(lineText, delimiters);
                pq.Enqueue(new(line, fileIndex), line);
            }
        }

        void FlushBuffer()
        {
            writer.WriteAsText(stream, outputBuffer);
            outputBuffer.Clear();
        }
    }

    private StreamReader CreateReader(IFileInfo fileInfo)
    {
        var stream = _fileSystem.FileStream.New(fileInfo.FullName, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            BufferSize = ReaderBufferSize,
        });

        return new StreamReader(stream, detectEncodingFromByteOrderMarks: true, bufferSize: ReaderBufferSize);
    }

    private readonly record struct LineWithFileIndex(Line Line, int FileIndex);
}