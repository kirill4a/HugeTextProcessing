using HugeTextProcessing.Generating.Commands;
using System.Text;

namespace HugeTextProcessing.Generating.Generators;

internal class AsyncFileGenerator
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private static readonly string[] _indexes = [.. Enumerable.Range(1, 100).Select(i => i.ToString())];

    private readonly char[] _delimiters = ['.', ' '];
    private readonly int _newLineSize = _utf8.GetByteCount(Environment.NewLine);
    private readonly int _delimitersSize;

    public AsyncFileGenerator()
    {
        _delimitersSize = _utf8.GetByteCount(_delimiters);
    }

    public async ValueTask ExecuteAsync(FileGeneratingCommand command, CancellationToken cancellationToken)
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
            foreach (var item in source)
            {
                var indexText = GetRandomIndexText();
                var lineSize = CalculateLineSize(indexText, item) * minDuplicateCount;

                if (currentSize + lineSize > fileSize.Bytes)
                {
                    stop = true;
                    break;
                }

                await WriteItemLine(writer, indexText, item, cancellationToken);
                while (minDuplicateCount > 1)
                {
                    await WriteItemLine(writer, indexText, item, cancellationToken);
                    minDuplicateCount--;
                }

                currentSize += lineSize;
            }
        }
    }

    private static string GetRandomIndexText() => _indexes[Random.Shared.Next(0, _indexes.Length - 1)];

    private int CalculateLineSize(string indexText, string item) =>
        _utf8.GetByteCount(indexText)
        + _delimitersSize
        + _utf8.GetByteCount(item)
        + _newLineSize;

    private async ValueTask WriteItemLine(StreamWriter writer, string indexText, string item, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await writer.WriteAsync(indexText);
        await writer.WriteAsync(_delimiters);
        await writer.WriteAsync(item);
        await writer.WriteAsync(Environment.NewLine);
    }
}
