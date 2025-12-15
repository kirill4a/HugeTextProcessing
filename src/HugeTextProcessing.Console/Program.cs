var rootCommand = new HugeTextProcessing.Console.Commands.RootCommand();

if (args.Length == 0)
{
    return await rootCommand.Parse("-h").InvokeAsync();
}

var executionTimeout = TimeSpan.FromHours(1);
var cancellationTokenSource = new CancellationTokenSource(executionTimeout, TimeProvider.System);

return await rootCommand.Parse(args).InvokeAsync(cancellationToken: cancellationTokenSource.Token);
