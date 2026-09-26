using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class ScalePrecisionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string ScalePrecisionErrorFormat = "'{0}' must not be more than {1} digits in total, with allowance for {2} decimals. {3} digits and {4} decimals were found.";

    [Fact]
    public void ScalePrecision_IsSameAsExpected_Success()
    {
        //Arrange
        decimal value = 101.12m;
        int expectedScale = 2;
        int expectedPrecision = 5;

        //Act
        var result = Result.ScalePrecision(value, expectedScale, expectedPrecision, false, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void ScalePrecision_IsLessThanExpected_Success()
    {
        //Arrange
        decimal value = 101.12m;
        int expectedScale = 3;
        int expectedPrecision = 8;

        //Act
        var result = Result.ScalePrecision(value, expectedScale, expectedPrecision, false, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void ScalePrecision_IsSameAsExpectedWithoutTrailingZeroes_Success()
    {
        //Arrange
        decimal value = 101.12000m;
        int expectedScale = 2;
        int expectedPrecision = 5;

        //Act
        var result = Result.ScalePrecision(value, expectedScale, expectedPrecision, true, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void ScalePrecisionMultiPart_IsSameAsExpected_Success()
    {
        //Arrange
        decimal value = 101.12m;
        int expectedScale = 2;
        int expectedPrecision = 5;

        //Act
        var result = Result.ScalePrecision(value, expectedScale, expectedPrecision, false, 
            MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void ScalePrecision_IsNotSameAsExpected_Failure()
    {
        //Arrange
        decimal value = 101.12m;
        int expectedScale = 2;
        int expectedPrecision = 4;

        ScalePrecisionHelpers.ValidateAndCalculateScaleAndPrecision(value, expectedScale, expectedPrecision, false,
            out int actualScale, out int actualPrecision, out int actualIntegerDigits, out int expectedIntegerDigits);

        int digits = actualIntegerDigits < 0 ? 0 : actualIntegerDigits;

        //Act
        var result = Result.ScalePrecision(value, expectedScale, expectedPrecision, false, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ScalePrecisionErrorFormat, MyPropertyName, expectedPrecision, expectedScale, digits, actualScale));
    }

    [Fact]
    public void ScalePrecisionMultiPart_IsNotSameAsExpected_Failure()
    {
        //Arrange
        decimal value = 101.12m;
        int expectedScale = 2;
        int expectedPrecision = 4;

        ScalePrecisionHelpers.ValidateAndCalculateScaleAndPrecision(value, expectedScale, expectedPrecision, false,
            out int actualScale, out int actualPrecision, out int actualIntegerDigits, out int expectedIntegerDigits);

        int digits = actualIntegerDigits < 0 ? 0 : actualIntegerDigits;

        //Act
        var result = Result.ScalePrecision(value, expectedScale, expectedPrecision, false, 
            MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ScalePrecisionErrorFormat, multiPartPropertyName, expectedPrecision, expectedScale, digits, actualScale));
    }
}
