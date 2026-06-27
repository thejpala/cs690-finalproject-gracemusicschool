using GraceMusic.Infrastructure;
using Xunit;

namespace GraceMusic.Tests.Infrastructure;

public class IdGeneratorTests
{
    [Fact]
    public void Generate_ShouldFormatCorrectlyWithPrefixAndCount()
    {
        // Arrange
        string prefix = "TCH";
        int currentCount = 5;

        // Act
        string result = IdGenerator.Generate(prefix, currentCount);

        // Assert
        Assert.Equal("TCH-006", result); // Because 5 + 1 formatted to 3 digits is 006
    }
}