namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

public class TaxIdTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";
    const string MultiPartPropertyNameFormat = "{0} {1}";
    const string MultiPartPropertyName = MyPropertyName + " " + MyPropertyNameTwo;

    static ValidationError Expected(string propertyName, string code, string template) =>
        new(propertyName, code, template, propertyName);

    // IsUSTaxId

    [Theory]
    [InlineData("12-3456789")]
    [InlineData("123-45-6789")]
    [InlineData("912-34-5678")]
    public void IsUSTaxId_ValidValue_Success(string value)
    {
        var result = Result.IsUSTaxId(value, MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("123-45-67890")]
    public void IsUSTaxId_InvalidLength_TaxIdError(string value)
    {
        var result = Result.IsUSTaxId(value, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.TaxId, ExpectedUSValidatorErrorStrings.TaxId), result.Error);
    }

    [Theory]
    [InlineData("123-456789")]
    [InlineData("12-34567-8")]
    [InlineData("12-345678a")]
    [InlineData("1234-56-789")]
    [InlineData("123-456-789")]
    [InlineData("123-45-678a")]
    public void IsUSTaxId_InvalidFormat_TaxIdFormatError(string value)
    {
        var result = Result.IsUSTaxId(value, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.TaxIdFormat, ExpectedUSValidatorErrorStrings.TaxIdFormat), result.Error);
    }

    [Fact]
    public void IsUSTaxIdMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsUSTaxId("12-345678a", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.TaxIdFormat, ExpectedUSValidatorErrorStrings.TaxIdFormat), result.Error);
    }

    [Theory]
    [InlineData("12-3456789")]
    [InlineData("123-45-6789")]
    public void IsUSTaxIdExtension_ValidValue_Success(string value)
    {
        var result = Result.Success(value).IsUSTaxId(MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void IsUSTaxIdExtension_InvalidLength_TaxIdError()
    {
        var result = Result.Success("123").IsUSTaxId(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.TaxId, ExpectedUSValidatorErrorStrings.TaxId), result.Error);
    }

    [Fact]
    public void IsUSTaxIdExtension_InvalidFormat_TaxIdFormatError()
    {
        var result = Result.Success("123-456-789").IsUSTaxId(MyPropertyName);

        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.TaxIdFormat, ExpectedUSValidatorErrorStrings.TaxIdFormat), result.Error);
    }

    // IsUSEinTaxId

    [Fact]
    public void IsUSEinTaxId_ValidValue_Success()
    {
        var result = Result.IsUSEinTaxId("12-3456789", MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal("12-3456789", result.Value);
    }

    [Theory]
    [InlineData("123-45-6789")]
    [InlineData("123456789")]
    [InlineData("123-456789")]
    [InlineData("12-34567-8")]
    [InlineData("12-345678a")]
    public void IsUSEinTaxId_InvalidValue_EinFormatError(string value)
    {
        var result = Result.IsUSEinTaxId(value, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.EinFormat, ExpectedUSValidatorErrorStrings.EinFormat), result.Error);
    }

    [Fact]
    public void IsUSEinTaxIdMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsUSEinTaxId("12-34567-8", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.EinFormat, ExpectedUSValidatorErrorStrings.EinFormat), result.Error);
    }

    [Theory]
    [InlineData("12-3456789", true)]
    [InlineData("12-345678a", false)]
    public void IsUSEinTaxIdExtension_Value_ExpectedOutcome(string value, bool isSuccess)
    {
        var result = Result.Success(value).IsUSEinTaxId(MyPropertyName);

        Assert.Equal(isSuccess, result.IsSuccess);
        if (!isSuccess)
        {
            Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.EinFormat, ExpectedUSValidatorErrorStrings.EinFormat), result.Error);
        }
    }

    [Fact]
    public void IsUSEinTaxIdExtensionMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.Success("1").IsUSEinTaxId(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.EinFormat, ExpectedUSValidatorErrorStrings.EinFormat), result.Error);
    }

    // IsUSSsnTaxId

    [Fact]
    public void IsUSSsnTaxId_ValidValue_Success()
    {
        var result = Result.IsUSSsnTaxId("123-45-6789", MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal("123-45-6789", result.Value);
    }

    [Theory]
    [InlineData("12-3456789")]
    [InlineData("1234-5-6789")]
    [InlineData("123-456-789")]
    [InlineData("123-45-678a")]
    public void IsUSSsnTaxId_InvalidFormat_SsnFormatError(string value)
    {
        var result = Result.IsUSSsnTaxId(value, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.SsnFormat, ExpectedUSValidatorErrorStrings.SsnFormat), result.Error);
    }

    [Fact]
    public void IsUSSsnTaxId_StartsWithNine_SsnError()
    {
        var result = Result.IsUSSsnTaxId("912-34-5678", MyPropertyName);

        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.Ssn, ExpectedUSValidatorErrorStrings.Ssn), result.Error);
    }

    [Fact]
    public void IsUSSsnTaxIdMultiPart_StartsWithNine_UsesFormattedPropertyName()
    {
        var result = Result.IsUSSsnTaxId("912-34-5678", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.Ssn, ExpectedUSValidatorErrorStrings.Ssn), result.Error);
    }

    [Theory]
    [InlineData("123-45-6789", null)]
    [InlineData("912-34-5678", ExpectedUSValidationErrorCodes.Ssn)]
    [InlineData("123-45-678a", ExpectedUSValidationErrorCodes.SsnFormat)]
    public void IsUSSsnTaxIdExtension_Value_ExpectedOutcome(string value, string? expectedCode)
    {
        var result = Result.Success(value).IsUSSsnTaxId(MyPropertyName);

        if (expectedCode is null)
        {
            Assert.True(result.IsSuccess);
            return;
        }

        var template = expectedCode == ExpectedUSValidationErrorCodes.Ssn ? ExpectedUSValidatorErrorStrings.Ssn : ExpectedUSValidatorErrorStrings.SsnFormat;
        Assert.Equal(Expected(MyPropertyName, expectedCode, template), result.Error);
    }

    [Fact]
    public void IsUSSsnTaxIdExtensionMultiPart_InvalidLength_UsesFormattedPropertyName()
    {
        var result = Result.Success("123").IsUSSsnTaxId(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.SsnFormat, ExpectedUSValidatorErrorStrings.SsnFormat), result.Error);
    }

    // IsUSItinTaxId

    [Fact]
    public void IsUSItinTaxId_ValidValue_Success()
    {
        var result = Result.IsUSItinTaxId("912-34-5678", MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal("912-34-5678", result.Value);
    }

    [Theory]
    [InlineData("12-3456789")]
    [InlineData("9123-4-5678")]
    [InlineData("912-345-678")]
    [InlineData("912-34-567a")]
    public void IsUSItinTaxId_InvalidFormat_ItinFormatError(string value)
    {
        var result = Result.IsUSItinTaxId(value, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.ItinFormat, ExpectedUSValidatorErrorStrings.ItinFormat), result.Error);
    }

    [Fact]
    public void IsUSItinTaxId_DoesNotStartWithNine_ItinError()
    {
        var result = Result.IsUSItinTaxId("123-45-6789", MyPropertyName);

        Assert.Equal(Expected(MyPropertyName, ExpectedUSValidationErrorCodes.Itin, ExpectedUSValidatorErrorStrings.Itin), result.Error);
    }

    [Fact]
    public void IsUSItinTaxIdMultiPart_DoesNotStartWithNine_UsesFormattedPropertyName()
    {
        var result = Result.IsUSItinTaxId("123-45-6789", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.Itin, ExpectedUSValidatorErrorStrings.Itin), result.Error);
    }

    [Theory]
    [InlineData("912-34-5678", null)]
    [InlineData("123-45-6789", ExpectedUSValidationErrorCodes.Itin)]
    [InlineData("912-34-567a", ExpectedUSValidationErrorCodes.ItinFormat)]
    public void IsUSItinTaxIdExtension_Value_ExpectedOutcome(string value, string? expectedCode)
    {
        var result = Result.Success(value).IsUSItinTaxId(MyPropertyName);

        if (expectedCode is null)
        {
            Assert.True(result.IsSuccess);
            return;
        }

        var template = expectedCode == ExpectedUSValidationErrorCodes.Itin ? ExpectedUSValidatorErrorStrings.Itin : ExpectedUSValidatorErrorStrings.ItinFormat;
        Assert.Equal(Expected(MyPropertyName, expectedCode, template), result.Error);
    }

    [Fact]
    public void IsUSItinTaxIdExtensionMultiPart_InvalidLength_UsesFormattedPropertyName()
    {
        var result = Result.Success("9").IsUSItinTaxId(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        Assert.Equal(Expected(MultiPartPropertyName, ExpectedUSValidationErrorCodes.ItinFormat, ExpectedUSValidatorErrorStrings.ItinFormat), result.Error);
    }

    // Failure propagation

    [Fact]
    public void Extensions_FailedResult_ReturnOriginalError()
    {
        var error = new ValidationError("other", "other_code", "'{0}' other.", "other");
        var failed = Result.Failure<string>(error);

        Assert.Same(error, failed.IsUSTaxId(MyPropertyName).Error);
        Assert.Same(error, failed.IsUSEinTaxId(MyPropertyName).Error);
        Assert.Same(error, failed.IsUSSsnTaxId(MyPropertyName).Error);
        Assert.Same(error, failed.IsUSItinTaxId(MyPropertyName).Error);
    }
}
