namespace Continuum.CSharpFunctionalExtensions.Validators.US.Tests;

public class StateAbbreviationExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    [Theory]
    [InlineData("NC")]
    [InlineData("SC")]
    public void IsUSPostalStateAbbreviation_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .IsUSPostalStateAbbreviation(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("NC")]
    [InlineData("SC")]
    public void IsUSPostalStateAbbreviationMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsUSPostalStateAbbreviation(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ZZ")]
    public void IsUSPostalStateAbbreviation_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .IsUSPostalStateAbbreviation(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedUSValidationErrorCodes.StateAbbreviation, ExpectedUSValidatorErrorStrings.StateAbbreviation, MyPropertyName));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ZZ")]
    public void IsUSPostalStateAbbreviationMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsUSPostalStateAbbreviation(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedUSValidationErrorCodes.StateAbbreviation, ExpectedUSValidatorErrorStrings.StateAbbreviation, multiPartPropertyName));
    }
}
