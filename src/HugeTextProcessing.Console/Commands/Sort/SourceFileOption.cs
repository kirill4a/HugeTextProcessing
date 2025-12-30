using System.CommandLine;
using System.CommandLine.Parsing;

namespace HugeTextProcessing.Console.Commands.Sort;
internal class SourceFileOption : Option<string>
{
    public SourceFileOption() : base(name: "--source", aliases: "-s")
    {
        Description = "The full path to file being sorted.";
        Required = true;
        Arity = ArgumentArity.ExactlyOne;
        CustomParser = Parse;
    }

    private string? Parse(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            result.AddError("Source file path is mandatory");
            return null;
        }

        var path = result.Tokens.Single().Value;

        if (!File.Exists(path))
        {
            result.AddError($"File '{path}' does not exists");
            return null;
        }

        return path;
    }
}
