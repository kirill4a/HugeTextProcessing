using Bogus;
using FluentAssertions;

namespace HugeTextProcessing.Abstractions.Tests;

public class LineTests
{
    private static readonly Faker Faker = new();

    public static TheoryData<string?, Delimiters> IncorrectData =>
        new()
        {
            { null, default },
            { string.Empty, Delimiters.Default },
            { "   ", Delimiters.Default },
            { "qwerty", default }
        };

    public static IEnumerable<object[]> DataToCompare =>
    [
        [
            new Line(0, "abcd", Delimiters.Default),
            new Line(0, "efgh", Delimiters.Default),
            new Func<Line, Line, bool>((x, y) => x < y),
            true
        ],
        [
            new Line(0, "abcd", Delimiters.Default),
            new Line(100, "efgh", Delimiters.Default),
            new Func<Line, Line, bool>((x, y) => x < y),
            true
        ],
        [
            new Line(0, "abcd", Delimiters.Default),
            new Line(1, "abcd", Delimiters.Default),
            new Func<Line, Line, bool>((x, y) => x < y),
            true
        ],
        [
            new Line(0, "abcd", Delimiters.Default),
            new Line(0, "abcd", Delimiters.Default),
            new Func<Line, Line, bool>((x, y) => x < y || x > y),
            false
        ]
    ];

    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void When_IncorrectConstructorParameters_ShouldFail(string? value, Delimiters delimiters)
    {
        // Arrange
        const int index = 0;

        // Act
        var action = () => new Line(index, value!, delimiters);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void When_CorrectConstructorParameters_ShouldCreateValid()
    {
        // Arrange
        var index = Faker.Random.Number();
        var value = Faker.Random.String2(minLength: 1, maxLength: 1_000);
        var delimiters = Faker.Random.String(minLength: 1, maxLength: 10);

        // Act
        var result = new Line(index, value, new Delimiters(delimiters));

        // Assert
        result.Should().NotBeNull();
        result.Index.Should().Be(index);
        result.Value.ToString().Should().Be(value);
        result.Delimiters.ToString().Should().Be(delimiters);
    }

    [Theory]
    [MemberData(nameof(DataToCompare))]
    public void When_Compare_ShouldCorrectOrder(
        Line left,
        Line right,
        Func<Line, Line, bool> compareAction,
        bool expectedResult)
    {
        // Act
        var compareResult = compareAction.Invoke(left, right);

        // Assert
        compareResult.Should().Be(expectedResult);
    }
}
