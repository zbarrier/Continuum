using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class NotNullOrWhitespaceExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

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
        var result = Result.Success(value)
            .NotNullOrWhitespace(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrWhitespace_StringPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .NotNullOrWhitespace(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
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
        var result = Result.Success(value)
            .NotNullOrWhitespace(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_StringPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .NotNullOrWhitespace(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNullOrWhitespace_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.Success(value)
            .NotNullOrWhitespace(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.Success(value)
            .NotNullOrWhitespace(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespace_StringWithWhitespaces_Failure()
    {
        //Arrange
        var value = "     ";

        //Act
        var result = Result.Success(value)
            .NotNullOrWhitespace(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_StringWithWhitespaces_Failure()
    {
        //Arrange
        var value = "     ";

        //Act
        var result = Result.Success(value)
            .NotNullOrWhitespace(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespace_NullString_Failure()
    {
        //Arrange
        string value = null;

        //Act
        var result = Result.Success(value)
            .NotNullOrWhitespace(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrWhitespaceMultiPart_NullString_Failure()
    {
        //Arrange
        string value = null;

        //Act
        var result = Result.Success(value)
            .NotNullOrWhitespace(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }
}
