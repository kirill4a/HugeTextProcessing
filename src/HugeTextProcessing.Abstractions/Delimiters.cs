namespace HugeTextProcessing.Abstractions;

/// <summary>
/// Represents the delimiters for <see cref="Line"> parts
/// </summary>
public readonly record struct Delimiters
{
    private readonly string _value;

    public Delimiters(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
            throw new ArgumentException("Delimiters cannot be empty.", nameof(value));

        _value = value.ToString();
    }

    public ReadOnlySpan<char> Value => _value;

    public static Delimiters Default { get; } = new(['.', ' ']);

    public override string ToString() => _value;
}
