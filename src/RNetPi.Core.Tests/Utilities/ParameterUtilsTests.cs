using RNetPi.Core.Utilities;

namespace RNetPi.Core.Tests.Utilities;

public class ParameterUtilsTests
{
    [Theory]
    [InlineData(0, true)]  // Bass
    [InlineData(1, true)]  // Treble
    [InlineData(3, true)]  // Balance
    [InlineData(2, false)] // Loudness
    [InlineData(4, false)] // Turn on Volume
    [InlineData(5, false)] // Background Color
    [InlineData(6, false)] // Do Not Disturb
    [InlineData(7, false)] // Party Mode
    [InlineData(8, false)] // Front A/V Enable
    public void IsParameterSigned_ShouldReturnCorrectValue(byte parameterID, bool expected)
    {
        // Act
        var result = ParameterUtils.IsParameterSigned(parameterID);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2, true)]  // Loudness
    [InlineData(6, true)]  // Do Not Disturb
    [InlineData(8, true)]  // Front A/V Enable
    [InlineData(0, false)] // Bass
    [InlineData(1, false)] // Treble
    [InlineData(3, false)] // Balance
    [InlineData(4, false)] // Turn on Volume
    [InlineData(5, false)] // Background Color
    [InlineData(7, false)] // Party Mode
    public void IsParameterBoolean_ShouldReturnCorrectValue(byte parameterID, bool expected)
    {
        // Act
        var result = ParameterUtils.IsParameterBoolean(parameterID);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsParameterSigned_ShouldThrowArgumentException_ForInvalidParameterID()
    {
        // Arrange
        byte invalidParameterID = 255;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ParameterUtils.IsParameterSigned(invalidParameterID));
    }
}