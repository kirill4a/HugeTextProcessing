using Bogus;
using FluentAssertions;

namespace HugeTextProcessing.Abstractions.Tests;

public class DelimiterTests
{
    private static readonly Faker Faker = new();

    public static TheoryData<char[]?> IncorrectData =>
        [
            null,
            [.. string.Empty],
            []
        ];

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void When_IncorrectConstructorParameters_ShouldFail(char[]? delimiters)
    {
        // Act
        var action = () => new Delimiters(delimiters);

        // Assert
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void When_CorrectConstructorParameters_ShouldCreateValid()
    {
        // Arrange
        var delimiters = Faker.Random.String(minLength: 1, maxLength: 10);

        // Act
        var result = new Delimiters(delimiters);

        // Assert
        result.Should().NotBeNull();
        result.ToString().Should().Be(delimiters);
    }
}