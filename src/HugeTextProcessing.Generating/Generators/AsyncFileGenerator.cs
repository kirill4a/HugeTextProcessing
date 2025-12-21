using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using System.Buffers.Text;
using System.Text;

namespace HugeTextProcessing.Generating.Generators;

internal class AsyncFileGenerator
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly int _newLineSize = _utf8.GetByteCount(Environment.NewLine);

    public async ValueTask ExecuteAsync(GenerateFileCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var (path, fileSize, source) = (command.Path, command.Size, command.Source);

        await using var writer = new StreamWriter(path, _utf8, new FileStreamOptions
        {
            PreallocationSize = fileSize.Bytes,
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        });

        long currentSize = 0;
        byte minDuplicateCount = 2;
        bool stop = false;

        while (!stop)
        {
            await GenerateFromSource();
        }

        return;

        async ValueTask GenerateFromSource()
        {
            foreach (var line in source)
            {
                var lineSize = CalculateLineSize(line) * minDuplicateCount;

                if (currentSize + lineSize >= fileSize.Bytes)
                {
                    stop = true;
                    break;
                }

                await WriteItemLine(writer, line, cancellationToken);
                while (minDuplicateCount > 1)
                {
                    await WriteItemLine(writer, line, cancellationToken);
                    minDuplicateCount--;
                }

                currentSize += lineSize;
            }
        }
    }

    private int CalculateLineSize(Line line)
    {
        Span<byte> buffer = stackalloc byte[11];
        Utf8Formatter.TryFormat(line.Index, buffer, out var indexBytesWritten);

        return indexBytesWritten
            + _utf8.GetByteCount(line.Delimiters.Value)
            + _utf8.GetByteCount(line.Value)
            + _newLineSize;
    }

    private static async ValueTask WriteItemLine(StreamWriter writer, Line line, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await writer.WriteAsync(line.Index.ToString());
        await writer.WriteAsync(line.Delimiters.Value.ToArray());
        await writer.WriteAsync(line.Value.ToString());
        await writer.WriteAsync(Environment.NewLine);
    }
}
