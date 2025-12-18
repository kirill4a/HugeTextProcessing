namespace HugeTextProcessing.Abstractions;

/// <summary>
/// Represents the record of multiline text file
/// </summary>
public readonly record struct Line : IComparable<Line>
{
    public Line(int index, string value, Delimiters delimiters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (delimiters == default)
        {
            throw new ArgumentException("The value should not be default", nameof(delimiters));
        }

        Index = index;
        Value = value;
        Delimiters = delimiters;
    }

    /// <summary>
    /// The number part
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// The text part
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The delimiters part
    /// </summary>
    public Delimiters Delimiters { get; }

    public int CompareTo(Line other)
    {
        if (other == default)
        {
            return this == default ? 0 : 1;
        }

        int valueComparison = Value.CompareTo(other.Value);
        if (valueComparison != 0)
        {
            return valueComparison;
        }

        return Index.CompareTo(other.Index);
    }

    public override string ToString() => string.Concat(Index, Delimiters, Value);

    #region comparison operators

    public static bool operator >(Line left, Line right) =>
        left.CompareTo(right) > 0;

    public static bool operator <(Line left, Line right) =>
        left.CompareTo(right) < 0;

    public static bool operator >=(Line left, Line right) =>
        left.CompareTo(right) >= 0;

    public static bool operator <=(Line left, Line right) =>
        left.CompareTo(right) <= 0;
    #endregion
}
