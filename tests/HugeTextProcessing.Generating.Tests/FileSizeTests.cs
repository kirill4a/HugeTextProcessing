using FluentAssertions;
using HugeTextProcessing.Generating.ValueObjects.Size;

namespace HugeTextProcessing.Generating.Tests;
public class FileSizeTests
{
    public static TheoryData<long, FileSizeKind> IncorrectSizes =>
        GetSizes(
            [
                -1,
                0
            ]);

    public static TheoryData<long, FileSizeKind> CorrectSizes =>
        GetSizes(
            [
                Random.Shared.NextInt64(1, 100)
            ]);

    [Theory]
    [MemberData(nameof(IncorrectSizes))]
    public void When_SizeIsIncorrect_ShouldFail(long size, FileSizeKind kind)
    {
        // Act
        var action = () => FileSize.From(size, kind);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void When_SizeIsOverflow_ShouldFail()
    {
        //Arrange
        const long fileSize = long.MaxValue;
        const FileSizeKind kind = FileSizeKind.KiB;

        // Act
        var action = () => FileSize.From(fileSize, kind);

        // Assert
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [MemberData(nameof(CorrectSizes))]
    public void When_SizeIsCorrect_ShouldCreateValid(long size, FileSizeKind kind)
    {
        // Arrange
        var expectedBytes = size * kind.Factor();

        // Act
        var fileSize = FileSize.From(size, kind);

        // Assert
        fileSize.Should().NotBeNull();
        fileSize.Bytes.Should().Be(expectedBytes);
    }

    private static TheoryData<long, FileSizeKind> GetSizes(IEnumerable<long> sizes)
    {
        ArgumentNullException.ThrowIfNull(sizes);

        var data = new TheoryData<long, FileSizeKind>();
        var kinds = Enum.GetValues<FileSizeKind>();

        foreach (var size in sizes)
        {
            foreach (var kind in kinds)
            {
                data.Add(size, kind);
            }
        }
        return data;
    }
}
