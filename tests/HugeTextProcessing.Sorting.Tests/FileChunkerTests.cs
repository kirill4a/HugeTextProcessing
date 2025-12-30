using Bogus;
using FluentAssertions;
using FluentValidation;
using HugeTextProcessing.Abstractions;
using HugeTextProcessing.Sorting.Configuration;
using HugeTextProcessing.Tests.Fixtures;
using Microsoft.Extensions.Options;

namespace HugeTextProcessing.Sorting.Tests;

public class FileChunkerTests(FileSystemFixture fixture) : IClassFixture<FileSystemFixture>
{
    private static readonly string Separator = new(Delimiters.Default.Value);
    private static readonly Faker Faker = new();
    private readonly FileSystemFixture _fixture = fixture;
    private readonly IValidator<SortOptions> _optionsValidator = new SortOptionsValidator();

    public static TheoryData<string?, IOptions<SortOptions>?> IncorrectData =>
        new()
        {
            { null, null },
            { Faker.System.FilePath(), null },
            { null, Options.Create(new SortOptions { ChunkLimit = 1024, CommonSeparator = Separator}) },
        };

    public static TheoryData<IOptions<SortOptions>> IncorrectOptions =>
        [
            Options.Create(new SortOptions { ChunkLimit = long.MinValue, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = -1, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = 0, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = 1024 - 1, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = 1_073_741_824 + 1, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = long.MaxValue, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = 1024, CommonSeparator = null! }),
            Options.Create(new SortOptions { ChunkLimit = 1024, CommonSeparator=string.Empty, }),
            Options.Create(new SortOptions { ChunkLimit = 1024, CommonSeparator = "   " }),
        ];

    public static TheoryData<IOptions<SortOptions>> CorrectOptions =>
        [
            Options.Create(new SortOptions { ChunkLimit = 1024, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = 1_073_741_824, CommonSeparator = Separator }),
            Options.Create(new SortOptions { ChunkLimit = Faker.Random.Number(1024, 1_073_741_824), CommonSeparator = Separator }),
        ];

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public async Task When_IncorrectAgruments_ShouldFail(string? sourceFilePath, IOptions<SortOptions>? options)
    {
        // Act
        var action = async () => await CreateChunker().ChunkAsync(sourceFilePath!, options!, CancellationToken.None);

        // Assert        
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(IncorrectOptions))]
    public async Task When_IncorrectOptions_ShouldFail(IOptions<SortOptions> options)
    {
        // Arrange
        var command = CreateSourceFile();

        // Act
        var action = async () => await CreateChunker().ChunkAsync(command, options, CancellationToken.None);

        // Assert        
        await action.Should().ThrowExactlyAsync<FluentValidation.ValidationException>();
    }

    [Theory]
    [MemberData(nameof(CorrectOptions))]
    public async Task When_CorrectOptions_ShouldNotFail(IOptions<SortOptions> options)
    {
        // Arrange
        var command = CreateSourceFile();

        // Act
        var action = async () => await CreateChunker().ChunkAsync(command, options, CancellationToken.None);

        // Assert        
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task When_SortChunks_ShouldHaveSortedChunks()
    {
        // Arrange
        var lines = ArrangeSourceData(1_000).ToArray();
        var sourceFilePath = CreateSourceFile(lines);
        var options = Options.Create(new SortOptions { ChunkLimit = 1024, CommonSeparator = Separator });

        // Act
        var tempDir = await CreateChunker().ChunkAsync(
            sourceFilePath,
            options,
            CancellationToken.None);

        // Assert        
        var resultFiles = tempDir.GetFiles();
        resultFiles.Should().NotBeNullOrEmpty();
        await Task.WhenAll(resultFiles.Select(x => AssertFileSorted(x.FullName)));
    }

    private async Task AssertFileSorted(string filePath)
    {
        var textLines = await _fixture.FileSystem.File.ReadAllLinesAsync(filePath, CancellationToken.None);
        var lines = textLines.Select(x => Line.Parse(x, Separator)).ToArray();

        lines.Should().BeInAscendingOrder();
    }

    private static IEnumerable<Line> ArrangeSourceData(int itemsCount)
    {
        var texts = Enumerable.Range(1, itemsCount).Select(i => Faker.Random.String2(1, i));

        int i = 0;

        foreach (var text in texts)
        {
            if (i % 5 == 0)
            {
                yield return new Line(Faker.Random.Number(1, 101), text, Delimiters.Default);
                yield return new Line(Faker.Random.Number(1, 101), text, Delimiters.Default);
            }

            if (i % 10 == 0)
            {
                var index = Faker.Random.Number(1, 101);
                yield return new Line(index, text, Delimiters.Default);
                yield return new Line(index, text, Delimiters.Default);
            }

            yield return new Line(Faker.Random.Number(1, 101), text, Delimiters.Default);

            i++;
        }
    }

    private string CreateSourceFile(IEnumerable<Line>? lines = null)
    {
        var path = Faker.System.FilePath();
        if (lines is not null)
        {
            _fixture.AddFile(path, string.Join(Environment.NewLine, lines));
        }
        else
        {
            _fixture.AddEmptyFile(path);
        }

        return path;
    }

    private FileChunker CreateChunker() => new(_fixture.FileSystem, _optionsValidator);
}
