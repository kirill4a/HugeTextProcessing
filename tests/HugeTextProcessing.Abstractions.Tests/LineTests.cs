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

    public static TheoryData<Line, Line, int> DataToCompare =>
        new()
        {
            {
                new Line(0, "abcd", Delimiters.Default),
                new Line(0, "efgh", Delimiters.Default),
                -1
            },
            {
                new Line(0, "abcd", Delimiters.Default),
                new Line(100, "efgh", Delimiters.Default),
                -1
            },
            {
                new Line(1, "abcd", Delimiters.Default),
                new Line(0, "abcd", Delimiters.Default),
                1
            },
            {
                new Line(0, "abcd", Delimiters.Default),
                new Line(0, "abcd", Delimiters.Default),
                0
            }
        };

    public static TheoryData<Line, Line, CompareResults> DataToCompareOperators =>
        new()
        {
            {
                new Line(0, "abcd", Delimiters.Default),
                new Line(0, "efgh", Delimiters.Default),
                new CompareResults(Lower: true, Greater: false, LowerOrEqual: true, GreaterOrEqual: false)
            },
            {
                new Line(0, "abcd", Delimiters.Default),
                new Line(100, "efgh", Delimiters.Default),
                new CompareResults(Lower: true, Greater: false, LowerOrEqual: true, GreaterOrEqual: false)
            },
            {
                new Line(1, "abcd", Delimiters.Default),
                new Line(0, "abcd", Delimiters.Default),
                new CompareResults(Lower: false, Greater: true, LowerOrEqual: false, GreaterOrEqual: true)
            },
            {
                new Line(0, "abcd", Delimiters.Default),
                new Line(0, "abcd", Delimiters.Default),
                new CompareResults(Lower: false, Greater: false, LowerOrEqual: true, GreaterOrEqual: true)
            }
        };

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
        var index = Faker.Random.Number(1, 101);
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
    public void CompareLines_ShouldHaveExpectedOrder(Line left, Line right, int expectedResult)
    {
        // Act
        var compareResult = Math.Sign(left.CompareTo(right));

        // Assert
        compareResult.Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(DataToCompareOperators))]
    public void CompareLinesWithOperators_ShouldHaveExpectedOrder(
        Line left,
        Line right,
        CompareResults expectedResult)
    {
        // Act
        var result = new CompareResults(
            Lower: left < right,
            Greater: left > right,
            LowerOrEqual: left <= right,
            GreaterOrEqual: left >= right);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void CompareLinesEquality_ShouldBeEqual()
    {
        // Arrange
        var index = Faker.Random.Number(1, 101);
        var value = Faker.Random.String(1, 101);
        var left = new Line(index, value, Delimiters.Default);
        var right = new Line(index, value, Delimiters.Default);
        var leftText = left.ToString();
        var rightText = right.ToString();

        // Act
        var resultEqual = Equals(left, right);
        var resultExact = left == right;
        var resultGreaterOrEqual = left >= right;
        var resultLowerOrEqual = left <= right;
        var resultGreater = left > right;
        var resultLower = left < right;

        // Assert
        resultEqual.Should().BeTrue();
        resultExact.Should().BeTrue();
        resultGreaterOrEqual.Should().BeTrue();
        resultLowerOrEqual.Should().BeTrue();
        resultGreater.Should().BeFalse();
        resultLower.Should().BeFalse();

        left.Index.Should().Be(right.Index).And.Be(index);
        left.Delimiters.Should().Be(right.Delimiters).And.Be(Delimiters.Default);
        left.Value.ToString().Should().Be(right.Value.ToString()).And.Be(value);
        leftText.ToString().Should().Be(rightText);
    }

    public readonly record struct CompareResults(
        bool Lower,
        bool Greater,
        bool LowerOrEqual,
        bool GreaterOrEqual);
}
