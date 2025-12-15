namespace HugeTextProcessing.Generating.ValueObjects.Size;

internal static class FileSizeKindExtensions
{
    public static long Factor(this FileSizeKind kind) =>
        kind switch
        {
            FileSizeKind.Bytes => 1L,
            FileSizeKind.KiB => 1L << 10,
            FileSizeKind.MiB => 1L << 20,
            FileSizeKind.GiB => 1L << 30,
            _ => throw new NotSupportedException($"{nameof(FileSizeKind)} '{kind}' is not supported")
        };
}
