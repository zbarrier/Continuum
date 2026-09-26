using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class NotNullOrWhitespaceTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string NotNullOrEmptyErrorFormat = "'{0}' must not be empty.";

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotNullOrWhitespace_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrWhitespace(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotNullOrWhitespaceMultiPart_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrWhitespace(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrWhitespace_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.NotNullOrWhitespace(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, NotNullOrEmptyErrorFormat, MyPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.NotNullOrWhitespace(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, NotNullOrEmptyErrorFormat, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespace_StringWithWhitespace_Failure()
    {
        //Arrange
        var value = "     ";

        //Act
        var result = Result.NotNullOrWhitespace(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, NotNullOrEmptyErrorFormat, MyPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_StringWithWhitespace_Failure()
    {
        //Arrange
        var value = "     ";

        //Act
        var result = Result.NotNullOrWhitespace(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, NotNullOrEmptyErrorFormat, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespace_NullString_Failure()
    {
        //Arrange
        string value = null;

        //Act
        var result = Result.NotNullOrWhitespace(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, NotNullOrEmptyErrorFormat, MyPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_NullString_Failure()
    {
        //Arrange
        string value = null;

        //Act
        var result = Result.NotNullOrWhitespace(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, NotNullOrEmptyErrorFormat, multiPartPropertyName));
    }
}
