using HugeTextProcessing.Generating.ValueObjects.Size;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace HugeTextProcessing.Console.Commands.Generate;

internal class FileSizeOption : Option<FileSize>
{
    private const int DefaultSizeValue = 1;
    private const FileSizeKind DefaultSizeKind = FileSizeKind.MiB;

    public FileSizeOption() : base(name: "--size", aliases: "-s")
    {
        Description = "File size (e.g. 1GB, 500MB, 10KB); default 1MB in case of not specified.";
        Arity = ArgumentArity.ExactlyOne;
        DefaultValueFactory = _ => DefaultValue;

        CustomParser = Parse;
    }

    public static FileSize DefaultValue => FileSize.From(DefaultSizeValue, DefaultSizeKind);

    private FileSize? Parse(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            return DefaultValue;
        }

        var text = result.Tokens.Single().Value.ToUpperInvariant();

        long multiplier = 1;
        if (text.EndsWith("GB")) multiplier = 1L << 30;
        else if (text.EndsWith("MB")) multiplier = 1L << 20;
        else if (text.EndsWith("KB")) multiplier = 1L << 10;

        if (long.TryParse([.. text.TakeWhile(char.IsDigit)], out var number) is false
            ||
            number < 1)
        {
            result.AddError("Incorrect file size argument. Should be positive integer with postfix");
            return null;
        }

        return FileSize.From(number * multiplier, FileSizeKind.Bytes);
    }
}
