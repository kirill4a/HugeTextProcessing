using HugeTextProcessing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace HugeTextProcessing.Sorting.Configuration;

/// <summary>
/// The options for sorting file
/// </summary>
public record SortOptions
{
    internal const long MinChunkSize = 1024;
    internal const long MaxChunkSize = 1L << 30;
    internal const string ChunkErrorMessage = $"{nameof(ChunkLimit)} must be between 1024 and 1_073_741_824.";

    /// <summary>
    /// Maximum bytes per chunk when splitting source file (default 256 MiB)
    /// </summary>
    [Range(MinChunkSize, MaxChunkSize, ErrorMessage = ChunkErrorMessage)]
    public long ChunkLimit { get; init; } = 256L << 20;

    /// <summary>
    /// The delimiter to parse each line in text file to <see cref="Line"/>
    /// </summary>
    /// <remarks>
    /// Requires all lines in text file should have the same delimiter.
    /// </remarks>
    [Required(AllowEmptyStrings = false)]
    [MinLength(1)]
    public required string CommonSeparator { get; init; }
}
