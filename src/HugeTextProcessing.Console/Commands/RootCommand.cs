using HugeTextProcessing.Console.Commands.Generate;
using HugeTextProcessing.Console.Commands.Sort;

namespace HugeTextProcessing.Console.Commands;

internal class RootCommand : System.CommandLine.RootCommand
{
    public RootCommand()
        : base("Huge text generator CLI")
    {
        this.Subcommands.Add(new GenerateCommand());
        this.Subcommands.Add(new SortCommand());
    }
}
