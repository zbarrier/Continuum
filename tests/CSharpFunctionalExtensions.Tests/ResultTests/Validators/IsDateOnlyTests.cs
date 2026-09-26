using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class IsDateOnlyTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string IsGuidErrorFormat = "'{0}' is not in the correct format.";

    [Theory]
    [InlineData("2023-05-01")]
    [InlineData("2023-05-02")]
    [InlineData("2023-05-03")]
    [InlineData("2023-05-04")]
    public void IsDateOnly_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDateOnly(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString("yyyy-MM-dd"), value);
    }

    [Theory]
    [InlineData("2023-05-01")]
    [InlineData("2023-05-02")]
    [InlineData("2023-05-03")]
    [InlineData("2023-05-04")]
    public void IsDateOnlyMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDateOnly(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString("yyyy-MM-dd"), value);
    }

    [Theory]
    [InlineData("2023-05-02 10")]
    [InlineData("test")]
    [InlineData("20230502")]
    public void IsDateOnly_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDateOnly(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, IsGuidErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("2023-05-02 10")]
    [InlineData("test")]
    [InlineData("20230502")]
    public void IsDateOnlyMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.IsDateOnly(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, IsGuidErrorFormat, multiPartPropertyName));
    }
}
