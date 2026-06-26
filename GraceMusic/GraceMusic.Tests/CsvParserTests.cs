using GraceMusic.Infrastructure;
using Xunit;

namespace GraceMusic.Tests.Infrastructure;

public class CsvParserTests
{
    [Fact]
    public void Escape_ShouldWrapInQuotes_WhenStringContainsComma()
    {
        // Arrange
        string input = "Doe, John";

        // Act
        string result = CsvParser.Escape(input);

        // Assert
        Assert.Equal("\"Doe, John\"", result);
    }

    [Fact]
    public void Escape_ShouldDoNothing_WhenStringIsNormal()
    {
        // Arrange
        string input = "Piano";

        // Act
        string result = CsvParser.Escape(input);

        // Assert
        Assert.Equal("Piano", result);
    }

    [Fact]
    public void Split_ShouldHandleStandardCommaSeparatedValues()
    {
        // Arrange
        string line = "STU-001,Charlie Brown,555-1234";

        // Act
        var result = CsvParser.Split(line);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("Charlie Brown", result[1]);
    }

    [Fact]
    public void Split_ShouldIgnoreCommasInsideQuotes()
    {
        // Arrange
        string line = "STU-001,\"Brown, Charlie\",555-1234";

        // Act
        var result = CsvParser.Split(line);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("Brown, Charlie", result[1]);
    }
}