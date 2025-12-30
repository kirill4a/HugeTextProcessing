using System.CommandLine;
using System.CommandLine.Parsing;

namespace HugeTextProcessing.Console.Commands.Sort;
internal class DestinationFileOption : Option<string>
{
    public DestinationFileOption() : base(name: "--destination", aliases: "-d")
    {
        Description = "The full path to result sorted file.";
        Required = true;
        Arity = ArgumentArity.ExactlyOne;
        CustomParser = Parse;
    }

    private string? Parse(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            result.AddError("Destination file path is mandatory");
            return null;
        }

        return result.Tokens.Single().Value;
    }
}
