namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

public class ZipCodeTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";



    [Theory]
    [InlineData("55555")]
    [InlineData("12345-1234")]
    public void IsUSPostalZipCode_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsUSZipCode(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("55555")]
    [InlineData("12345-1234")]
    public void IsUSPostalZipCodeMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsUSZipCode(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("111")]
    [InlineData("5555")]
    [InlineData("123451234")]
    [InlineData("１２３４５")]
    public void IsUSPostalZipCode_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsUSZipCode(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedUSValidationErrorCodes.ZipCode, ExpectedUSValidatorErrorStrings.ZipCode, MyPropertyName));
    }

    [Theory]
    [InlineData("111")]
    [InlineData("5555")]
    [InlineData("123451234")]
    [InlineData("１２３４５")]
    public void IsUSPostalZipCodeMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsUSZipCode(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedUSValidationErrorCodes.ZipCode, ExpectedUSValidatorErrorStrings.ZipCode, multiPartPropertyName));
    }
}
