namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class CreditCardExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error CreditCardError = new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.CreditCard, ExpectedValidatorErrorStrings.CreditCard, MyPropertyName);

    static readonly string MultiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
    static readonly Error MultiPartCreditCardError = new ValidationError(MultiPartPropertyName, ExpectedValidationErrorCodes.CreditCard, ExpectedValidatorErrorStrings.CreditCard, MultiPartPropertyName);

    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    [Theory]
    [InlineData("378282246310005")]  //American Express
    [InlineData("30569309025904")]   //Diners Club
    [InlineData("6011111111111117")] //Discover
    [InlineData("5555555555554444")] //Mastercard
    [InlineData("4111111111111111")] //Visa
    public void IsCreditCard_ValidNumber_Success(string creditCardNumber)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(creditCardNumber, MyPropertyName)
            .IsCreditCard(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, creditCardNumber);
    }

    [Fact]
    public void IsCreditCard_PreviousFailure_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .IsCreditCard(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("378282246310005")]  //American Express
    [InlineData("30569309025904")]   //Diners Club
    [InlineData("6011111111111117")] //Discover
    [InlineData("5555555555554444")] //Mastercard
    [InlineData("4111111111111111")] //Visa
    public void IsCreditCardMultiPart_ValidNumber_Success(string creditCardNumber)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(creditCardNumber, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsCreditCard(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, creditCardNumber);
    }

    [Fact]
    public void IsCreditCardMultiPart_PreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .IsCreditCard(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void IsCreditCard_LessThan16Digits_Failure()
    {
        //Arrange
        string creditCardNumber = "411111111111111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MyPropertyName)
            .IsCreditCard(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_LessThan16Digits_Failure()
    {
        //Arrange
        string creditCardNumber = "411111111111111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsCreditCard(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }

    [Fact]
    public void IsCreditCard_MoreThan16Digits_Failure()
    {
        //Arrange
        string creditCardNumber = "41111111111111111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MyPropertyName)
            .IsCreditCard(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_MoreThan16Digits_Failure()
    {
        //Arrange
        string creditCardNumber = "41111111111111111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsCreditCard(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }

    [Fact]
    public void IsCreditCard_ContainsNonDigits_Failure()
    {
        //Arrange
        string creditCardNumber = "4111111111a11111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MyPropertyName)
            .IsCreditCard(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_ContainsNonDigits_Failure()
    {
        //Arrange
        string creditCardNumber = "4111111111a11111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsCreditCard(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }

    [Fact]
    public void IsCreditCard_DoesntStartWithFourOrFiveOrSix_Failure()
    {
        //Arrange
        string creditCardNumber = "3111111111111111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MyPropertyName)
            .IsCreditCard(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_DoesntStartWithFourOrFiveOrSix_Failure()
    {
        //Arrange
        string creditCardNumber = "3111111111111111";

        //Act
        var result = Result.NotEmpty(creditCardNumber, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsCreditCard(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }
}
