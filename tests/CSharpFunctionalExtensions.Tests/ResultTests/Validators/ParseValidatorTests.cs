namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class ParseValidatorTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";
    const string MultiPartPropertyNameFormat = "{0} {1}";
    const string MultiPartPropertyName = MyPropertyName + " " + MyPropertyNameTwo;

    static ValidationError InvalidFormat(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, propertyName);

    static ValidationError NotEmpty(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, propertyName);

    static ValidationError Email(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.Email, ExpectedValidatorErrorStrings.Email, propertyName);

    static readonly ValidationError OtherError = new("other", "other_code", "'{0}' other.", "other");

    // IsDateTime

    [Fact]
    public void IsDateTime_ValidValue_Success()
    {
        var result = Result.IsDateTime("2024-01-15T10:30:00", MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0), result.Value);
    }

    [Fact]
    public void IsDateTime_InvalidValue_InvalidFormatError()
    {
        var result = Result.IsDateTime("not a date", MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsDateTime_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsDateTime(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsDateTimeMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsDateTime("not a date", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsDateTimeMultiPart_ValidValue_Success()
    {
        var result = Result.IsDateTime("2024-01-15", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(new DateTime(2024, 1, 15), result.Value);
    }

    [Fact]
    public void IsDateTimeChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateTime(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateTime(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Error);
    }

    // IsInt32

    [Theory]
    [InlineData("0", 0)]
    [InlineData("-42", -42)]
    [InlineData("2147483647", int.MaxValue)]
    public void IsInt32_ValidValue_Success(string value, int expected)
    {
        var result = Result.IsInt32(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    public void IsInt32_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsInt32(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsInt32_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsInt32(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsInt32MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsInt32("abc", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsInt32MultiPart_ValidValue_Success()
    {
        var result = Result.IsInt32("7", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void IsInt32Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt32(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt32(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Error);
    }

    // IsDateOnly (chained)

    [Fact]
    public void IsDateOnlyChained_ValidValue_Success()
    {
        var result = Result.Success("2024-01-15").IsDateOnly(MyPropertyName);

        Assert.Equal(new DateOnly(2024, 1, 15), result.Value);
    }

    [Fact]
    public void IsDateOnlyChained_InvalidValue_InvalidFormatError()
    {
        var result = Result.Success("not a date").IsDateOnly(MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsDateOnlyChainedMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.Success("not a date").IsDateOnly(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsDateOnlyChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateOnly(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateOnly(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Error);
    }

    // IsAspNetCoreCompatibleEmail

    [Theory]
    [InlineData("a@b")]
    [InlineData("user@example.com")]
    [InlineData("first.last+tag@sub.example.org")]
    public void IsAspNetCoreCompatibleEmail_ValidValue_Success(string value)
    {
        Assert.Equal(value, Result.IsAspNetCoreCompatibleEmail(value, MyPropertyName).Value);
        Assert.Equal(value, Result.IsAspNetCoreCompatibleEmail(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).IsAspNetCoreCompatibleEmail(MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).IsAspNetCoreCompatibleEmail(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("userexample.com")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("a@b@c")]
    public void IsAspNetCoreCompatibleEmail_InvalidValue_EmailError(string value)
    {
        Assert.Equal(Email(MyPropertyName), Result.IsAspNetCoreCompatibleEmail(value, MyPropertyName).Error);
        Assert.Equal(Email(MultiPartPropertyName), Result.IsAspNetCoreCompatibleEmail(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Error);
        Assert.Equal(Email(MyPropertyName), Result.Success(value).IsAspNetCoreCompatibleEmail(MyPropertyName).Error);
        Assert.Equal(Email(MultiPartPropertyName), Result.Success(value).IsAspNetCoreCompatibleEmail(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo).Error);
    }

    [Fact]
    public void IsAspNetCoreCompatibleEmailChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsAspNetCoreCompatibleEmail(MyPropertyName).Error);
    }
}
