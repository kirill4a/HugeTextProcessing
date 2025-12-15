namespace HugeTextProcessing.Generating.ValueObjects.Size;

public sealed record FileSize
{
    private FileSize(long bytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bytes, 1);
        Bytes = bytes;
    }

    public long Bytes { get; }

    public static FileSize From(long value, FileSizeKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

        long size = checked(value * kind.Factor());
        return new FileSize(size);
    }
}
