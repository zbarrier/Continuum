using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class IsDecimalTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    [Theory]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("-123456788.111334")]
    [InlineData("-1022.001")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("1024.001")]
    [InlineData("123456790.111334")]
    [InlineData("79228162514264337593543950335")]
    public void IsDecimal_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDecimal(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("-79,228,162,514,264,337,593,543,950,335")]
    [InlineData("-123,456,788.111334")]
    [InlineData("-1,022.001")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("1,024.001")]
    [InlineData("123,456,790.111334")]
    [InlineData("79,228,162,514,264,337,593,543,950,335")]
    public void IsDecimal_ValidValueWithCommas_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDecimal(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString("#,##0.#########"), value);
    }

    [Theory]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("-123456788.111334")]
    [InlineData("-1022.001")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("1024.001")]
    [InlineData("123456790.111334")]
    [InlineData("79228162514264337593543950335")]
    public void IsDecimalMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDecimal(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("-79,228,162,514,264,337,593,543,950,335")]
    [InlineData("-123,456,788.111334")]
    [InlineData("-1,022.001")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("1,024.001")]
    [InlineData("123,456,790.111334")]
    [InlineData("79,228,162,514,264,337,593,543,950,335")]
    public void IsDecimalMultiPart_ValidValueWithCommas_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDecimal(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString("#,##0.#########"), value);
    }

    [Fact]
    public void IsDecimal_EmptyValue_Success()
    {
        //Arrange
        var value = decimal.Zero.ToString();

        //Act
        var result = Result.IsDecimal(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("-79,228,162,514,264,337,593,543,950,aaa")]
    [InlineData("-123456788.z111334")]
    [InlineData("-1022.z001")]
    [InlineData("aaa")]
    [InlineData("a1")]
    [InlineData("1024.001z")]
    [InlineData("123456s790.111334")]
    [InlineData("79,228,162,514,264,337,593,543,950,33s")]
    public void IsBigInteger_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDecimal(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("-79,228,162,514,264,337,593,543,950,aaa")]
    [InlineData("-123456788.z111334")]
    [InlineData("-1022.z001")]
    [InlineData("aaa")]
    [InlineData("a1")]
    [InlineData("1024.001z")]
    [InlineData("123456s790.111334")]
    [InlineData("79,228,162,514,264,337,593,543,950,33s")]
    public void IsDecimalMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDecimal(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, multiPartPropertyName));
    }
}
