using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

public class PhoneNumberExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";



    [Theory]
    [InlineData("3365551234")]
    [InlineData("+3365551234")]
    [InlineData("336-555-1234")]
    [InlineData("336 555-1234")]
    [InlineData("(336) 555-1234")]
    public void IsUSPhoneNumber_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .IsUSPhoneNumber(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);

        string strippedValue = Regex.Replace(value, @"\D+", "");
        Assert.Equal(result.Value, strippedValue);
    }

    [Theory]
    [InlineData("3365551234")]
    [InlineData("+3365551234")]
    [InlineData("336-555-1234")]
    [InlineData("336 555-1234")]
    [InlineData("(336) 555-1234")]
    public void IsUSPhoneNumberMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .IsUSPhoneNumber(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);

        string strippedValue = Regex.Replace(value, @"\D+", "");
        Assert.Equal(result.Value, strippedValue);
    }

    [Theory]
    [InlineData("111")]
    [InlineData("(336) 555--123)")]
    [InlineData("336555123")]
    [InlineData("336555123a")]
    [InlineData("３３６５５５１２３４")]
    public void IsUSPhoneNumber_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .IsUSPhoneNumber(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedUSValidationErrorCodes.PhoneNumber, ExpectedUSValidatorErrorStrings.PhoneNumber, MyPropertyName));
    }

    [Theory]
    [InlineData("111")]
    [InlineData("(336) 555--123)")]
    [InlineData("336555123")]
    [InlineData("336555123a")]
    [InlineData("３３６５５５１２３４")]
    public void IsUSPhoneNumberMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .IsUSPhoneNumber(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedUSValidationErrorCodes.PhoneNumber, ExpectedUSValidatorErrorStrings.PhoneNumber, multiPartPropertyName));
    }
}
