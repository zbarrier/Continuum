using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class ZipCodeExtensionsTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string ZipCodeErrorFormat = "'{0}' is not a valid US zip code.";

    [Theory]
    [InlineData("55555")]
    [InlineData("12345-1234")]
    public void IsUSPostalZipCode_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .IsUSZipCode(MyPropertyName);

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
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsUSZipCode(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("111")]
    [InlineData("5555")]
    [InlineData("123451234")]
    public void IsUSPostalZipCode_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .IsUSZipCode(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ZipCodeErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("111")]
    [InlineData("5555")]
    [InlineData("123451234")]
    public void IsUSPostalZipCodeMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsUSZipCode(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ZipCodeErrorFormat, multiPartPropertyName));
    }
}
