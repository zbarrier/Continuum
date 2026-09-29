namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

public class PhoneNumberExtensionValidatorTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";
    const string MultiPartPropertyNameFormat = "{0} {1}";
    const string MultiPartPropertyName = MyPropertyName + " " + MyPropertyNameTwo;

    static ValidationError Expected(string propertyName) =>
        new(propertyName, ExpectedUSValidationErrorCodes.PhoneNumberExtension, ExpectedUSValidatorErrorStrings.PhoneNumberExtension, propertyName);

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("12345")]
    public void IsUSPhoneNumberExtension_ValidValue_Success(string value)
    {
        var result = Result.IsUSPhoneNumberExtension(value, MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12a")]
    [InlineData("x123")]
    public void IsUSPhoneNumberExtension_InvalidValue_Failure(string value)
    {
        var result = Result.IsUSPhoneNumberExtension(value, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(Expected(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12a")]
    public void IsUSPhoneNumberExtensionMultiPart_InvalidValue_UsesFormattedPropertyName(string value)
    {
        var result = Result.IsUSPhoneNumberExtension(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName), result.Error);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    public void IsUSPhoneNumberExtensionChained_ValidValue_Success(string value)
    {
        var result = Result.Success(value).IsUSPhoneNumberExtension(MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12a")]
    public void IsUSPhoneNumberExtensionChained_InvalidValue_Failure(string value)
    {
        var result = Result.Success(value).IsUSPhoneNumberExtension(MyPropertyName);

        Assert.Equal(Expected(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsUSPhoneNumberExtensionChainedMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.Success("12a").IsUSPhoneNumberExtension(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsUSPhoneNumberExtensionChained_FailedResult_ReturnsOriginalError()
    {
        var error = new ValidationError("other", "other_code", "'{0}' other.", "other");

        var result = Result.Failure<string>(error).IsUSPhoneNumberExtension(MyPropertyName);

        Assert.Same(error, result.Error);
    }
}
