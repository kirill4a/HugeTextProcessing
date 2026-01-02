using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using System.Buffers.Text;
using System.Text;

namespace HugeTextProcessing.Generating;

internal class LinesWriter : ILinesWriter
{
    // The maximum bytes using to write Int32 as UTF-8 string
    private const int MaxIndexBytes = 10;
    private const int MaxLineBytes = 4096;
    private const int BatchSize = 64 * 1024;

    private static readonly Encoding _utf8 = Encoding.UTF8;
    private readonly byte[] _newLineBytes = _utf8.GetBytes(Environment.NewLine);

    /// <inheritdoc/>
    public long WriteAsText(Stream stream, Line line)
    {
        ArgumentNullException.ThrowIfNull(stream);

        long totalWritten = 0;
        int batchPosition = 0;
        Span<byte> batchBuffer = stackalloc byte[BatchSize];
        Span<byte> lineBuffer = stackalloc byte[MaxLineBytes];

        var lineBytes = WriteLineToBuffer(line, lineBuffer);
        lineBuffer[..lineBytes].CopyTo(batchBuffer[batchPosition..]);
        batchPosition += lineBytes;
        FlushBatch(stream, batchBuffer, ref batchPosition, ref totalWritten);
        return totalWritten;
    }

    /// <inheritdoc/>
    public long WriteAsText(Stream stream, IEnumerable<Line> lines)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(lines);

        Span<byte> batchBuffer = stackalloc byte[BatchSize];
        Span<byte> lineBuffer = stackalloc byte[MaxLineBytes];

        long totalWritten = 0;
        int batchPosition = 0;

        foreach (var line in lines)
        {
            int lineBytes = WriteLineToBuffer(line, lineBuffer);

            // Line too big for batch → flush batch + write current line directly
            if (lineBytes > batchBuffer.Length)
            {
                FlushBatch(stream, batchBuffer, ref batchPosition, ref totalWritten);
                stream.Write(lineBuffer[..lineBytes]);
                totalWritten += lineBytes;
                continue;
            }

            // Not enough space → flush batch
            if (batchPosition + lineBytes > batchBuffer.Length)
            {
                FlushBatch(stream, batchBuffer, ref batchPosition, ref totalWritten);
            }

            // Copy line into batch
            lineBuffer[..lineBytes].CopyTo(batchBuffer[batchPosition..]);
            batchPosition += lineBytes;
        }

        // Final flush
        FlushBatch(stream, batchBuffer, ref batchPosition, ref totalWritten);

        return totalWritten;
    }

    private int WriteLineToBuffer(Line line, Span<byte> buffer)
    {
        int pos = 0;

        if (!Utf8Formatter.TryFormat(line.Index, buffer, out var written))
        {
            throw new InvalidOperationException(
                "Line index formatting failed. " +
                $"The destination buffer of {buffer.Length} bytes is too small to write {line.Index} as UTF-8 string");
        }

        pos += written;

        foreach (var delimiter in line.Delimiters.Value)
            buffer[pos++] = (byte)delimiter;

        pos += _utf8.GetBytes(line.Value, buffer[pos..]);

        foreach (var b in _newLineBytes)
            buffer[pos++] = b;

        return pos;
    }

    private static void FlushBatch(
        Stream stream,
        Span<byte> batchBuffer,
        ref int batchPosition,
        ref long totalWritten)
    {
        if (batchPosition == 0)
            return;

        stream.Write(batchBuffer[..batchPosition]);
        totalWritten += batchPosition;
        batchPosition = 0;
    }
}
