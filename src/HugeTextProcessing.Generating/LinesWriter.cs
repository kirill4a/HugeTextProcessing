using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using System.Buffers.Text;
using System.Text;

namespace HugeTextProcessing.Generating;

internal class LinesWriter : ILinesWriter
{
    // The maximum bytes using to write Int32 as UTF-8 string
    private const int MaxIndexBytes = 10;

    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly byte[] _newLineBytes = _utf8.GetBytes(Environment.NewLine);

    /// <inheritdoc/>
    public long WriteAsText(Stream stream, Line line)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // TODO: need workaround to handle huge strings (more 4096 bytes in UTF-8)
        Span<byte> buffer = stackalloc byte[4096];
        return WriteLineBytes(stream, line, buffer);
    }

    /// <inheritdoc/>
    public long WriteAsText(Stream stream, IEnumerable<Line> lines)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(lines);

        // TODO: need workaround to handle huge strings (more 4096 bytes in UTF-8)
        Span<byte> buffer = stackalloc byte[4096];
        var bytesWritten = 0;

        foreach (var line in lines)
        {
            bytesWritten += WriteLineBytes(stream, line, buffer);
        }

        return bytesWritten;
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
}
