using HugeTextProcessing.Generating.Commands;
using HugeTextProcessing.Generating.Generators;
using System.Diagnostics;

namespace HugeTextProcessing.Console.Commands.Generate;
internal class GenerateCommand : System.CommandLine.Command
{
    private readonly string[] sourceData =
        [
            "Apple",
            "Something something something",
            "Cherry is the best",
            "Banana is yellow",
        ];

    public GenerateCommand()
        : base("generate", "Generates a multiline text file")
    {
        var sizeOption = new FileSizeOption();
        this.Add(sizeOption);

        this.SetAction(parseRsult =>
        {
            var fileSize = parseRsult.GetValue(sizeOption);
            if (fileSize is null)
            {
                return;
            }

            var command = new FileGeneratingCommand(
                Path.GetTempFileName(),
                fileSize,
                sourceData);

            ExecuteWithLog(command);
        });
    }

    private static void ExecuteWithLog(FileGeneratingCommand command)
    {
        // TODO: replace console output with logging and StopWatch with metrics
        System.Console.WriteLine($"Starting to generate file of size {command.Size.Bytes} bytes at path: {command.Path}");
        var sw = Stopwatch.StartNew();

        new SpanFileGenerator().Execute(command);

        sw.Stop();
        System.Console.WriteLine($"File has been created in {sw.Elapsed} at path: {command.Path}");
    }
}
