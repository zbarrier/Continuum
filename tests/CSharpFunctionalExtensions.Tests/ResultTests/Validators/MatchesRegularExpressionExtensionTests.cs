using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class MatchesRegularExpressionExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string RegularExpressionErrorFormat = "'{0}' is not in the correct format.";

    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    [Theory]
    [InlineData("Test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("Test", "^Test")] //The word Test
    [InlineData("Test123", "^[a-zA-Z0-9]+")]
    public void MatchesRegularExpression_MatchingValue_Success(string value, string pattern)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MatchesRegularExpression(pattern, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }


    [Theory]
    [InlineData("Test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("Test", "^Test")] //The word Test
    [InlineData("Test123", "^[a-zA-Z0-9]+")]
    public void MatchesRegularExpression_PreviousError_Failure(string value, string pattern)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .MatchesRegularExpression(pattern, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("Test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("Test", "^Test")] //The word Test
    [InlineData("Test123", "^[a-zA-Z0-9]+")]
    public void MatchesRegularExpression_MatchingRegexValue_Success(string value, string pattern)
    {
        //Arrange
        var regex = new Regex(pattern);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MatchesRegularExpression(regex, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("Test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("Test", "^Test")] //The word Test
    [InlineData("Test123", "^[a-zA-Z0-9]+")]
    public void MatchesRegularExpression_RegexPreviousError_Failure(string value, string pattern)
    {
        //Arrange
        var regex = new Regex(pattern);

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .MatchesRegularExpression(regex, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("Test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("Test", "^Test")] //The word Test
    [InlineData("Test123", "^[a-zA-Z0-9]+")]
    public void MatchesRegularExpressionMultiPart_MatchingValue_Success(string value, string pattern)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MatchesRegularExpression(pattern, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("Test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("Test", "^Test")] //The word Test
    [InlineData("Test123", "^[a-zA-Z0-9]+")]
    public void MatchesRegularExpressionMultiPart_MatchingRegexValue_Success(string value, string pattern)
    {
        //Arrange
        var regex = new Regex(pattern);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MatchesRegularExpression(regex, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("test", "^Test")] //The word Test
    [InlineData("Test123*%;", "^[^<>'\"/;`%]*$")]
    public void MatchesRegularExpression_NotAMatchingValue_Failure(string value, string pattern)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MatchesRegularExpression(pattern, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, RegularExpressionErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("test", "^Test")] //The word Test
    [InlineData("Test123*%;", "^[^<>'\"/;`%]*$")]
    public void MatchesRegularExpression_NotAMatchingRegexValue_Failure(string value, string pattern)
    {
        //Arrange
        var regex = new Regex(pattern);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MatchesRegularExpression(regex, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, RegularExpressionErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("test", "^Test")] //The word Test
    [InlineData("Test123*%;", "^[^<>'\"/;`%]*$")]
    public void MatchesRegularExpressionMultiPart_NotAMatchingValue_Failure(string value, string pattern)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MatchesRegularExpression(pattern, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, RegularExpressionErrorFormat, multiPartPropertyName));
    }

    [Theory]
    [InlineData("test", "^T[a-zA-Z0-9]+")] //Word that starts with letter T
    [InlineData("test", "^Test")] //The word Test
    [InlineData("Test123*%;", "^[^<>'\"/;`%]*$")]
    public void MatchesRegularExpressionMultiPart_NotAMatchingRegexValue_Failure(string value, string pattern)
    {
        //Arrange
        var regex = new Regex(pattern);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MatchesRegularExpression(regex, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, RegularExpressionErrorFormat, multiPartPropertyName));
    }
}
