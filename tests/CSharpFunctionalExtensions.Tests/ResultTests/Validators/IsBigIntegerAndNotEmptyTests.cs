using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class IsBigIntegerAndNotEmptyTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string IsBigIntegerErrorFormat = "'{0}' is not in the correct format.";

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890")]
    [InlineData("-123456788")]
    [InlineData("-1022")]
    [InlineData("1")]
    [InlineData("1024")]
    [InlineData("123456790")]
    [InlineData("1234567890123456789012345678901234567892")]
    public void IsBigInteger_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsBigIntegerAndNotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890")]
    [InlineData("-123456788")]
    [InlineData("-1022")]
    [InlineData("1")]
    [InlineData("1024")]
    [InlineData("123456790")]
    [InlineData("1234567890123456789012345678901234567892")]
    public void IsBigIntegerMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsBigIntegerAndNotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Fact]
    public void IsBigInteger_EmptyValue_Failure()
    {
        //Arrange
        var value = BigInteger.Zero.ToString();

        //Act
        var result = Result.IsBigIntegerAndNotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, IsBigIntegerErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890a")]
    [InlineData("-12345z6788")]
    [InlineData("-a1022")]
    [InlineData("aaa")]
    [InlineData("a1")]
    [InlineData("0")]
    [InlineData("1024z")]
    [InlineData("12345s6790")]
    [InlineData("1234567890123456789012345678901234567892s")]
    public void IsBigInteger_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsBigIntegerAndNotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, IsBigIntegerErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890a")]
    [InlineData("-12345z6788")]
    [InlineData("-a1022")]
    [InlineData("aaa")]
    [InlineData("a1")]
    [InlineData("0")]
    [InlineData("1024z")]
    [InlineData("12345s6790")]
    [InlineData("1234567890123456789012345678901234567892s")]
    public void IsBigIntegerMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsBigIntegerAndNotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, IsBigIntegerErrorFormat, multiPartPropertyName));
    }
}
