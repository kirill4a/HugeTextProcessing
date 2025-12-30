using Bogus;
using FluentAssertions;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Generating.Commands;
using HugeTextProcessing.Generating.Generators;
using HugeTextProcessing.Generating.Tests.Fixtures;
using HugeTextProcessing.Generating.ValueObjects.Size;
using HugeTextProcessing.Tests.Fixtures;

namespace HugeTextProcessing.Generating.Tests;

[Collection(nameof(TempFilesCollection))]
public class SimpleFileGeneratorTests(TempDirectoryFixture fixture)
{
    // max diff in bytes between expected (specified) and actual file size
    private const long SizeDiffThreshold = 150;

    private static readonly Faker Faker = new();
    private readonly TempDirectoryFixture _fixture = fixture;

    /*
    Consider to leverage System.IO.Abstractions and System.IO.Abstractions.TestingHelpers 
    to replace accessing real file system in unit tests. 
    See more: https://github.com/TestableIO/System.IO.Abstractions

    Or write your own IO abstractions\interfaces for wrap File/Directory/FileStream and mock them in tests.
    */

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    [InlineData(1_000_000)]
    public async Task When_Generated_KiB_FileSizeShouldNotExceed(int itemsCount)
    {
        // Arrange
        var path = _fixture.GetTempFileName();
        const long size = 5;
        const FileSizeKind sizeKind = FileSizeKind.KiB;
        var fileSize = FileSize.From(size, sizeKind);

        var sourceData = ArrangeSourceData(itemsCount);

        var command = new GenerateFileCommand(path, fileSize, sourceData);
        var generator = new SimpleFileGenerator();

        // Act
        generator.Execute(command);

        // Assert        
        File.Exists(path).Should().BeTrue();
        await AssertFileInfo(new FileInfo(path), fileSize);
    }

    private static IEnumerable<Line> ArrangeSourceData(int itemsCount) =>
        Enumerable.Range(1, itemsCount)
                  .Select(i => new Line(Faker.Random.Number(1, 101), Faker.Random.String2(1, i), Delimiters.Default));

    private static async ValueTask AssertFileInfo(FileInfo fileInfo, FileSize fileSize)
    {
        fileInfo.Length.Should().BeLessThanOrEqualTo(fileSize.Bytes);
        fileInfo.Length.Should().BeCloseTo(fileSize.Bytes, SizeDiffThreshold);

        var lines = await File.ReadAllLinesAsync(fileInfo.FullName, CancellationToken.None);
        var distinctLines = lines.ToHashSet();
        distinctLines.Should().HaveCountLessThan(lines.Length);
    }
}