using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using System.Buffers.Text;
using System.Text;

namespace HugeTextProcessing.Generating.Generators;
internal class SpanFileGenerator
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private static readonly int[] _indexes = [.. Enumerable.Range(1, 100)];

    private readonly byte[] _newLineBytes = _utf8.GetBytes(Environment.NewLine);
    private readonly Delimiters _delimiters = Delimiters.Default;

    public void Execute(FileGeneratingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var (path, fileSize, source) = (command.Path, command.Size, command.Source);

        using var stream = new FileStream(path, new FileStreamOptions
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
            Span<byte> buffer = stackalloc byte[256];

            foreach (var item in source)
            {
                var line = new Line(GetRandomIndex(), item, _delimiters);
                var bytesToWrite = GetLineBytes(line, buffer);
                var lineSize = bytesToWrite.Length * minDuplicateCount;

                if (currentSize + lineSize > fileSize.Bytes)
                {
                    stop = true;
                    break;
                }

                stream.Write(bytesToWrite);

                while (minDuplicateCount > 1)
                {
                    stream.Write(bytesToWrite);
                    minDuplicateCount--;
                }

                currentSize += lineSize;
                buffer.Clear();
            }
        }
    }

    private static int GetRandomIndex() => _indexes[Random.Shared.Next(0, _indexes.Length - 1)];

    private Span<byte> GetLineBytes(Line line, Span<byte> buffer)
    {
        int bufferPosition = 0;

        // Write index as UTF-8 bytes
        Utf8Formatter.TryFormat(line.Index, buffer, out var sizeIndex);
        bufferPosition += sizeIndex;

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

        return buffer[..bufferPosition];
    }
}