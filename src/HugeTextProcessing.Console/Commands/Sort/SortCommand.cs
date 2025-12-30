using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Sorting;
using HugeTextProcessing.Sorting.Commands;
using HugeTextProcessing.Sorting.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Abstractions;

namespace HugeTextProcessing.Console.Commands.Sort;
internal class SortCommand : System.CommandLine.Command
{
    public SortCommand()
        : base("sort", "Sorts a multiline text file")
    {
        var srcOption = new SourceFileOption();
        var dstOption = new DestinationFileOption();

        this.Add(srcOption);
        this.Add(dstOption);

        this.SetAction(async (parseResult, ct) =>
        {
            var srcPath = parseResult.GetValue(srcOption);
            var dstPath = parseResult.GetValue(dstOption);

            if (string.IsNullOrWhiteSpace(srcPath) || string.IsNullOrWhiteSpace(dstPath))
            {
                return;
            }

            var command = new SortFileCommand(srcPath, dstPath);
            await ExecuteWithLog(command, ct);
        });
    }

    private static async ValueTask ExecuteWithLog(SortFileCommand command, CancellationToken cancellationToken)
    {
        var options = GetDefaultSortOptions();

        // TODO: replace console output with logging and StopWatch with metrics
        System.Console.WriteLine($"Starting to sort file : {command.SourceFilePath}");
        var sw = Stopwatch.StartNew();

        var fileSystem = new FileSystem();
        await new FileSorter(fileSystem, options).SortAsync(command, cancellationToken);

        sw.Stop();
        System.Console.WriteLine($"File has been sorted in {sw.Elapsed} to: {command.DestinationFilePath}");
    }

    private static IOptions<SortOptions> GetDefaultSortOptions()
        =>
        Microsoft.Extensions.Options.Options.Create(new SortOptions
        {
            ChunkLimit = 256L << 20,
            CommonSeparator = new(Delimiters.Default.Value)
        });
}
