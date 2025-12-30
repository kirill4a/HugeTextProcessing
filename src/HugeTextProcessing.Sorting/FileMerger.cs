using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using HugeTextProcessing.Generating;
using HugeTextProcessing.Sorting.Configuration;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Text;

namespace HugeTextProcessing.Sorting;
internal class FileMerger(IFileSystem fileSystem)
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly IFileSystem _fileSystem = fileSystem;

    public void Merge(
        IEnumerable<IFileInfo> inputFiles,
        string outputFilePath,
        IOptions<SortOptions> options)
    {
        ArgumentNullException.ThrowIfNull(inputFiles);
        ArgumentNullException.ThrowIfNull(options);

        var readers = new List<StreamReader>();
        var pq = new PriorityQueue<(Line line, int fileIndex), Line>();// TODO: replace Tuple with record struct

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

            using var stream = _fileSystem.FileStream.New(outputFilePath, new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
            });
            ILinesWriter writer = LinesWriterFactory.Create();

            while (pq.Count > 0)
            {
                var (line, fileIndex) = pq.Dequeue();
                writer.WriteAsText(stream, line);

                var nextLine = readers[fileIndex].ReadLine();
                EnqueueLine(nextLine, fileIndex);
            }
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
            if (!string.IsNullOrWhiteSpace(lineText))
            {
                var line = Line.Parse(lineText, options.Value.CommonSeparator);
                pq.Enqueue((line, fileIndex), line);
            }
        }
    }

    private StreamReader CreateReader(IFileInfo fileInfo)
    {
        var stream = _fileSystem.FileStream.New(fileInfo.FullName, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
        });

        return new StreamReader(stream);
    }
}
