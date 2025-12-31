using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using System.Buffers.Text;
using System.IO.Abstractions;
using System.Text;

namespace HugeTextProcessing.Generating.Generators;
internal class SpanFileGenerator(IFileSystem fileSystem)
{
    // The maximum bytes using to write Int32 as UTF-8 string
    private const int MaxIndexBytes = 10;

    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly byte[] _newLineBytes = _utf8.GetBytes(Environment.NewLine);
    private readonly IFileSystem _fileSystem = fileSystem;

    public void Execute(GenerateFileCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var (path, fileSize, source) = (command.Path, command.Size, command.Source);

        using var stream = _fileSystem.FileStream.New(path, new FileStreamOptions
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
            // TODO: need workaround to handle huge strings (more 4096 bytes in UTF-8)
            Span<byte> buffer = stackalloc byte[4096];

            foreach (var line in source)
            {
                int lineSize = CalculateLineBytes(line) * minDuplicateCount;
                if (currentSize + lineSize >= fileSize.Bytes)
                {
                    stop = true;
                    break;
                }

                while (minDuplicateCount > 1)
                {
                    currentSize += WriteLineBytes(stream, line, buffer);
                    minDuplicateCount--;
                }

                currentSize += WriteLineBytes(stream, line, buffer);
            }
        }
    }

    private int WriteLineBytes(Stream stream, Line line, Span<byte> buffer)
    {
        int bufferPosition = 0;

        // Write index as UTF-8 bytes
        if (!Utf8Formatter.TryFormat(line.Index, buffer, out var bytesWritten))
        {
            throw new InvalidOperationException(
                "Line index formatting failed. " +
                $"The destination buffer of {buffer.Length} bytes is too small to write {line.Index} as UTF-8 string");
        }
        bufferPosition += bytesWritten;

        // Write delimiters
        foreach (var delimiter in line.Delimiters.Value)
        {
            buffer[bufferPosition++] = (byte)delimiter;
        }

        // Write text
        bufferPosition += _utf8.GetBytes(line.Value, buffer[bufferPosition..]);

        // Write newline
        foreach (var newLineByte in _newLineBytes)
        {
            buffer[bufferPosition++] = newLineByte;
        }

        var bytesToWrite = buffer[..bufferPosition];
        stream.Write(bytesToWrite);

        return bytesToWrite.Length;
    }

    private int CalculateLineBytes(Line line) =>
          MaxIndexBytes
        + line.Delimiters.Value.Length
        + _utf8.GetByteCount(line.Value)
        + _newLineBytes.Length;
}