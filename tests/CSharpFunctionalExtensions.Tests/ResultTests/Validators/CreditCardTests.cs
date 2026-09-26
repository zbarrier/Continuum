namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class CreditCardTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string CreditCardErrorFormat = "'{0}' is not a valid credit card number.";

    static readonly Error CreditCardError = new ValidationError(MyPropertyName, CreditCardErrorFormat, MyPropertyName);

    static readonly string MultiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
    static readonly Error MultiPartCreditCardError = new ValidationError(MultiPartPropertyName, CreditCardErrorFormat, MultiPartPropertyName);

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
        var result = Result.IsCreditCard(creditCardNumber, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, creditCardNumber);
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
        var result = Result.IsCreditCard(creditCardNumber, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, creditCardNumber);
    }

    [Fact]
    public void IsCreditCard_LessThan16Digits_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("411111111111111", MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_LessThan16Digits_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("411111111111111", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }

    [Fact]
    public void IsCreditCard_MoreThan16Digits_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("41111111111111111", MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_MoreThan16Digits_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("41111111111111111", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }

    [Fact]
    public void IsCreditCard_ContainsNonDigits_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("4111111111a11111", MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_ContainsNonDigits_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("4111111111a11111", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }

    [Fact]
    public void IsCreditCard_DoesntStartWithFourOrFiveOrSix_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("3111111111111111", MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, CreditCardError);
    }

    [Fact]
    public void IsCreditCardMultiPart_DoesntStartWithFourOrFiveOrSix_Failure()
    {
        //Arrange

        //Act
        var result = Result.IsCreditCard("3111111111111111", MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, MultiPartCreditCardError);
    }
}
