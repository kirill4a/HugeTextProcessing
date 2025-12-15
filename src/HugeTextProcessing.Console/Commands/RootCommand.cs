using HugeTextProcessing.Console.Commands.Generate;

namespace HugeTextProcessing.Console.Commands;

internal class RootCommand : System.CommandLine.RootCommand
{
    public RootCommand()
        : base("Huge text generator CLI")
    {
        this.Subcommands.Add(new GenerateCommand());
    }
}