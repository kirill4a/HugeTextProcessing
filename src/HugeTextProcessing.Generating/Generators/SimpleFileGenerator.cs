using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using System.Text;

namespace HugeTextProcessing.Generating.Generators;

internal class SimpleFileGenerator
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private static readonly string[] _indexes = [.. Enumerable.Range(1, 100).Select(i => i.ToString())];

    private readonly Delimiters _delimiters = Delimiters.Default;
    private readonly int _newLineSize = _utf8.GetByteCount(Environment.NewLine);
    private readonly int _delimitersSize;

    public SimpleFileGenerator()
    {
        _delimitersSize = _utf8.GetByteCount(_delimiters.Value);
    }

    public void Execute(FileGeneratingCommand command)
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
            foreach (var item in source)
            {
                var indexText = GetRandomIndexText();
                var lineSize = CalculateLineSize(indexText, item) * minDuplicateCount;

                if (currentSize + lineSize > fileSize.Bytes)
                {
                    stop = true;
                    break;
                }

                WriteItemLine(writer, indexText, item);
                while (minDuplicateCount > 1)
                {
                    WriteItemLine(writer, indexText, item);
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

    private void WriteItemLine(StreamWriter writer, string indexText, string item)
    {
        writer.Write(indexText);
        writer.Write(_delimiters.Value);
        writer.Write(item);
        writer.Write(Environment.NewLine);
    }
}
