using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class EmailTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error EmailError = new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Email, ExpectedValidatorErrorStrings.Email, MyPropertyName);

    static readonly string MultiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
    static readonly Error MultiPartEmailError = new ValidationError(MultiPartPropertyName, ExpectedValidationErrorCodes.Email, ExpectedValidatorErrorStrings.Email, MultiPartPropertyName);

    [Fact]
    public void IsEmail_SimpleAddress_Success()
    {
        //Arrange
        var emailAddress = "simple@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_SimpleAddress_Success()
    {
        //Arrange
        var emailAddress = "simple@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_AddressWithPeriod_Success()
    {
        //Arrange
        var emailAddress = "very.common@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithPeriod_Success()
    {
        //Arrange
        var emailAddress = "very.common@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_SingleCharBeforeAtSymbol_Success()
    {
        //Arrange
        var emailAddress = "a@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_SingleCharBeforeAtSymbol_Success()
    {
        //Arrange
        var emailAddress = "a@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_AddressWithTag_Success()
    {
        //Arrange
        var emailAddress = "user.name+tag+sorting@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithTag_Success()
    {
        //Arrange
        var emailAddress = "user.name+tag+sorting@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_AddressWithSlash_Success()
    {
        //Arrange
        var emailAddress = "name/surname@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithSlash_Success()
    {
        //Arrange
        var emailAddress = "name/surname@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_LongAddressWithHyphens_Success()
    {
        //Arrange
        var emailAddress = "long.email-address-with-hyphens@and.subdomains.example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_LongAddressWithHyphens_Success()
    {
        //Arrange
        var emailAddress = "long.email-address-with-hyphens@and.subdomains.example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_AddressWithSingleLetterAfterAtSymbol_Success()
    {
        //Arrange
        var emailAddress = "example@s.example";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithSingleLetterAfterAtSymbol_Success()
    {
        //Arrange
        var emailAddress = "example@s.example";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_AddressWithSpaceBetweenQuotes_Success()
    {
        //Arrange
        var emailAddress = "\" \"@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithSpaceBetweenQuotes_Success()
    {
        //Arrange
        var emailAddress = "\" \"@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_AddressWithQuoteDoubleDot_Success()
    {
        //Arrange
        var emailAddress = "\"john..doe\"@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithQuoteDoubleDot_Success()
    {
        //Arrange
        var emailAddress = "\"john..doe\"@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_BangifiedAddress_Success()
    {
        //Arrange
        var emailAddress = "mailhost!username@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_BangifiedAddress_Success()
    {
        //Arrange
        var emailAddress = "mailhost!username@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_EscapedAddress_Success()
    {
        //Arrange
        var emailAddress = "user%example.com@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_EscapedAddress_Success()
    {
        //Arrange
        var emailAddress = "user%example.com@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_LocalPartEndingInNonAlphanumericChar_Success()
    {
        //Arrange
        var emailAddress = "user-@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmailMultiPart_LocalPartEndingInNonAlphanumericChar_Success()
    {
        //Arrange
        var emailAddress = "user-@example.org";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, emailAddress);
    }

    [Fact]
    public void IsEmail_IPv4AddressInsteadOfDomain_Failure()
    {
        //Technically, is valid but not supported by current regex.

        //Arrange
        var emailAddress = "postmaster@[123.123.123.123]";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, EmailError);
    }

    [Fact]
    public void IsEmailMultiPart_IPv4AddressInsteadOfDomain_Failure()
    {
        //Technically, is valid but not supported by current regex.

        //Arrange
        var emailAddress = "postmaster@[123.123.123.123]";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartEmailError);
    }

    [Fact]
    public void IsEmail_IPv6AddressInsteadOfDomain_Failure()
    {
        //Technically, is valid but not supported by current regex.

        //Arrange
        var emailAddress = "postmaster@[IPv6:2001:0db8:85a3:0000:0000:8a2e:0370:7334]";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, EmailError);
    }

    [Fact]
    public void IsEmailMultiPart_IPv6AddressInsteadOfDomain_Failure()
    {
        //Technically, is valid but not supported by current regex.

        //Arrange
        var emailAddress = "postmaster@[IPv6:2001:0db8:85a3:0000:0000:8a2e:0370:7334]";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartEmailError);
    }

    [Fact]
    public void IsEmail_DoubleQuotedAddress_Failure()
    {
        //Technically, is valid but not supported by current regex.

        //Arrange
        var emailAddress = "\"very.(),:;<> []\".VERY.\"very@\\ \"very\".unusual\"@strange.example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, EmailError);
    }

    [Fact]
    public void IsEmailMultiPart_DoubleQuotedAddress_Failure()
    {
        //Technically, is valid but not supported by current regex.

        //Arrange
        var emailAddress = "\"very.(),:;<> []\".VERY.\"very@\\ \"very\".unusual\"@strange.example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartEmailError);
    }

    [Fact]
    public void IsEmail_AddressWithLocalDomainNameAndNoTLD_Failure()
    {
        //Technically, this is valid but highly discouraged. So, we do not allow it.

        //Arrange
        var emailAddress = "admin@example";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, EmailError);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithLocalDomainNameAndNoTLD_Failure()
    {
        //Technically, this is valid but highly discouraged. So, we do not allow it.

        //Arrange
        var emailAddress = "admin@example";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartEmailError);
    }

    [Fact]
    public void IsEmail_AddressWithNoAtSymbol_Failure()
    {
        //Arrange
        var emailAddress = "admin.example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, EmailError);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithNoAtSymbol_Failure()
    {
        //Arrange
        var emailAddress = "admin.example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartEmailError);
    }

    [Fact]
    public void IsEmail_AddressWithMoreThanOneAtSymbol_Failure()
    {
        //Arrange
        var emailAddress = "admin@abc@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, EmailError);
    }

    [Fact]
    public void IsEmailMultiPart_AddressWithMoreThanOneAtSymbol_Failure()
    {
        //Arrange
        var emailAddress = "admin@abc@example.com";

        //Act
        var result = Result.IsEmail(emailAddress, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartEmailError);
    }
}
