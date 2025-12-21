using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using System.Buffers.Text;
using System.Text;

namespace HugeTextProcessing.Generating.Generators;

internal class SimpleFileGenerator
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly int _newLineSize = _utf8.GetByteCount(Environment.NewLine);

    public void Execute(GenerateFileCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var (path, fileSize, source) = (command.Path, command.Size, command.Source);

        using var writer = new StreamWriter(path, _utf8, new FileStreamOptions
        {
            PreallocationSize = fileSize.Bytes,
            Mode = FileMode.Create,
            Access = FileAccess.Write,
        });

        long currentSize = 0;
        byte minDuplicateCount = 2;
        bool stop = false;

        while (!stop)
        {
            GenerateFromSource();
        }

        return;

        void GenerateFromSource()
        {
            foreach (var line in source)
            {
                var lineSize = CalculateLineSize(line) * minDuplicateCount;

                if (currentSize + lineSize >= fileSize.Bytes)
                {
                    stop = true;
                    break;
                }

                WriteItemLine(writer, line);
                while (minDuplicateCount > 1)
                {
                    WriteItemLine(writer, line);
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

    private static void WriteItemLine(StreamWriter writer, Line line)
    {
        Span<byte> buffer = stackalloc byte[11];
        Utf8Formatter.TryFormat(line.Index, buffer, out var indexBytesWritten);

        writer.BaseStream.Write(buffer[..indexBytesWritten]);
        writer.Write(line.Delimiters.Value);
        writer.Write(line.Value);
        writer.Write(Environment.NewLine);
    }
}