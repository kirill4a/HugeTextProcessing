namespace HugeTextProcessing.Abstractions;

/// <summary>
/// Represents the record of multiline text file
/// </summary>
/// <param name="Index">The number part</param>
/// <param name="Value">The text part</param>
/// <param name="Value">The delimiters part</param>
public readonly record struct Line(int Index, string Value, Delimiters Delimiters)
{
    public override string ToString() => string.Concat(Index, Delimiters, Value);
}