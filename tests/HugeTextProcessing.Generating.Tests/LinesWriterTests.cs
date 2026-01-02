using Bogus;
using FluentAssertions;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Abstractions.IO;
using HugeTextProcessing.Tests.Fixtures;
using System.IO.Abstractions;

namespace HugeTextProcessing.Generating.Tests;

public class LinesWriterTests(FileSystemFixture fixture) : IClassFixture<FileSystemFixture>
{
    private static readonly Faker Faker = new();
    private readonly FileSystemFixture _fixture = fixture;

    public static TheoryData<Stream?, IEnumerable<Line>?> IncorrectData =>
        new()
        {
            { null, null },
            { new MemoryStream(), null },
            { null, [] },
        };

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void When_IncorrectAgruments_ShouldFail(Stream? stream, IEnumerable<Line>? lines)
    {
        // Act
        var action = () => new LinesWriter().WriteAsText(stream!, lines!);

        // Assert        
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void When_SourceLinesEmpty_ShouldWriteEmpty()
    {
        // Arrange
        using var stream = new MemoryStream();
        IEnumerable<Line> lines = [];
        ILinesWriter writer = new LinesWriter();

        // Act
        var bytesWritten = writer.WriteAsText(stream, lines);

        // Assert        
        bytesWritten.Should().Be(0);
        stream.Length.Should().Be(0);
    }

    [Fact]
    public void When_SourceLinesHuge_ShouldWriteAndNotFail()
    {
        // Arrange
        var hugeText = new string('A', 1_000);
        using var stream = new MemoryStream();
        IEnumerable<Line> lines = [new Line(1, hugeText, Delimiters.Default)];
        ILinesWriter writer = new LinesWriter();

        // Act
        var bytesWritten = writer.WriteAsText(stream, lines);

        // Assert        
        bytesWritten.Should().BeGreaterThan(0);
    }

    [Fact]
    public void When_SourceLineSingle_ShouldWriteAndNotFail()
    {
        // Arrange
        var hugeText = new string('B', 1_000);
        using var stream = new MemoryStream();
        Line line = new(1, hugeText, Delimiters.Default);
        ILinesWriter writer = new LinesWriter();

        // Act
        var bytesWritten = writer.WriteAsText(stream, line);

        // Assert        
        bytesWritten.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task When_SourceLinesNotEmpty_ShouldWriteInSourceLinesOrder()
    {
        // Arrange
        Line[] lines = [.. ArrangeSourceData(100)];
        ILinesWriter writer = new LinesWriter();

        long bytesWritten = 0;
        var path = _fixture.GetRandomTempFileName();
        using (var stream = _fixture.FileSystem.File.Create(path))
        {
            // Act
            bytesWritten = writer.WriteAsText(stream, lines);
            await stream.FlushAsync(CancellationToken.None);

            // Assert        
            bytesWritten.Should().BeGreaterThan(0);
            stream.Length.Should().Be(bytesWritten);
        }

        _fixture.FileSystem.FileExists(path).Should().BeTrue();
        await AssertLinesOrder(_fixture.FileSystem.FileInfo.New(path), bytesWritten, lines);
    }

    private static IEnumerable<Line> ArrangeSourceData(int itemsCount) =>
        Enumerable.Range(1, itemsCount)
                  .Select(i => new Line(Faker.Random.Number(1, 101), Faker.Random.String2(1, i), Delimiters.Default));

    private async ValueTask AssertLinesOrder(IFileInfo fileInfo, long bytesWritten, Line[] sourceLInes)
    {
        fileInfo.Length.Should().Be(bytesWritten);

        var linesAsText = sourceLInes.Select(x => x.ToString()).ToArray();
        var lines = await _fixture.FileSystem.File.ReadAllLinesAsync(fileInfo.FullName, CancellationToken.None);

        lines.Should().HaveSameCount(linesAsText);
        lines.Should().ContainInConsecutiveOrder(linesAsText);
    }
}
