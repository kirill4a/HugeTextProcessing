using HugeTextProcessing.Abstractions.IO;

namespace HugeTextProcessing.Generating;

/// <summary>
/// Simulates DI for resolving <see cref="ILinesWriter"/>
/// </summary>
public static class LinesWriterFactory
{
    public static ILinesWriter Create() => new LinesWriter();
}
