using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class GreaterThanOrEqualTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    #region T

    [Theory]
    [InlineData(-100001, -100001)]
    [InlineData(-100000, -100001)]
    [InlineData(-100, -101)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(101, 100)]
    [InlineData(100001, 100000)]
    [InlineData(100001, 100001)]
    public void GreaterThanOrEqual_ClassOfGreaterThanOrEqualValue_Success(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100001, -100001)]
    [InlineData(-100000, -100001)]
    [InlineData(-100, -101)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(101, 100)]
    [InlineData(100001, 100000)]
    [InlineData(100001, 100001)]
    public void GreaterThanOrEqualMultiPart_ClassOfGreaterThanOrEqualValue_Success(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100001, -100000)]
    [InlineData(-101, -100)]
    [InlineData(0, 1)]
    [InlineData(100, 101)]
    [InlineData(100000, 100001)]
    public void GreaterThanOrEqual_ClassOfLessThanValue_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    [Theory]
    [InlineData(-100001, -100000)]
    [InlineData(-101, -100)]
    [InlineData(0, 1)]
    [InlineData(100, 101)]
    [InlineData(100000, 100001)]
    public void GreaterThanOrEqualMultiPart_ClassOfLessThanValue_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("z", "z")]
    [InlineData("z", "a")]
    [InlineData("w", "b")]
    [InlineData("b", "b")]
    [InlineData("test", "test")]
    [InlineData("123", "123")]
    [InlineData("124", "123")]
    public void GreaterThanOrEqual_StringOfGreaterThanOrEqualValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("z", "z")]
    [InlineData("z", "a")]
    [InlineData("w", "b")]
    [InlineData("b", "b")]
    [InlineData("test", "test")]
    [InlineData("123", "123")]
    [InlineData("124", "123")]
    public void GreaterThanOrEqualMultiPart_StringOfGreaterThanOrEqualValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("123", "124")]
    public void GreaterThanOrEqual_StringOfLessThanValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("123", "124")]
    public void GreaterThanOrEqualMultiPart_StringOfLessThanValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(13, 12)]
    [InlineData(128, 127)]
    [InlineData(128, 128)]
    [InlineData(255, 254)]
    [InlineData(255, 255)]
    public void GreaterThanOrEqual_ByteOfGreaterThanOrEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(13, 12)]
    [InlineData(128, 127)]
    [InlineData(128, 128)]
    [InlineData(255, 254)]
    [InlineData(255, 255)]
    public void GreaterThanOrEqualMultiPart_ByteOfGreaterThanOrEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void GreaterThanOrEqual_ByteOfLessThanValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void GreaterThanOrEqualMultiPart_ByteOfLessThanValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-32_767, -32_768)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(257, 257)]
    [InlineData(1024, 1023)]
    [InlineData(32_767, 32_766)]
    [InlineData(32_767, 32_767)]
    public void GreaterThanOrEqual_ShortOfGreaterThanOrEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-32_767, -32_768)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(257, 257)]
    [InlineData(1024, 1023)]
    [InlineData(32_767, 32_766)]
    [InlineData(32_767, 32_767)]
    public void GreaterThanOrEqualMultiPart_ShortOfGreaterThanOrEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1, 0)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void GreaterThanOrEqual_ShortOfLessThanValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1, 0)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void GreaterThanOrEqualMultiPart_ShortOfLessThanValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_646)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    public void GreaterThanOrEqual_IntOfGreaterThanOrEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_646)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    public void GreaterThanOrEqualMultiPart_IntOfGreaterThanOrEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(255, 256)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void GreaterThanOrEqual_IntOfLessThanValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(255, 256)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void GreaterThanOrEqualMultiPart_IntOfLessThanValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region long

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_767)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1024, 1024)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_648, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_806)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_807)]
    public void GreaterThanOrEqual_LongOfGreaterThanOrEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_767)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1024, 1024)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_648, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_806)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_807)]
    public void GreaterThanOrEqualMultiPart_LongOfGreaterThanOrEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void GreaterThanOrEqual_LongOfLessThanValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void GreaterThanOrEqualMultiPart_LongOfLessThanValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    [InlineData(3.626, 3.626)]
    public void GreaterThanOrEqual_FloatOfGreaterThanOrEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    [InlineData(3.626, 3.626)]
    public void GreaterThanOrEqualMultiPart_FloatOfGreaterThanOrEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.15, -3.14)]
    [InlineData(-1.3334, -1.3333)]
    [InlineData(0, 1)]
    [InlineData(1.224, 1.225)]
    [InlineData(3.624, 3.625)]
    public void GreaterThanOrEqual_FloatOfLessThanValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.15, -3.14)]
    [InlineData(-1.3334, -1.3333)]
    [InlineData(0, 1)]
    [InlineData(1.224, 1.225)]
    [InlineData(3.624, 3.625)]
    public void GreaterThanOrEqualMultiPart_FloatOfLessThanValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.626)]
    [InlineData(1234.123456790, 1234.123456789)]
    [InlineData(1234.123456790, 1234.123456790)]
    public void GreaterThanOrEqual_DoubleOfGreaterThanOrEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.626)]
    [InlineData(1234.123456790, 1234.123456789)]
    [InlineData(1234.123456790, 1234.123456790)]
    public void GreaterThanOrEqualMultiPart_DoubleOfGreaterThanOrEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456790, -1234.123456789)]
    [InlineData(-3.15, -3.14)]
    [InlineData(-1.3334, -1.3333)]
    [InlineData(0, 1)]
    [InlineData(1.224, 1.225)]
    [InlineData(3.624, 3.625)]
    [InlineData(1234.123456788, 1234.123456789)]
    public void GreaterThanOrEqual_DoubleOfLessThanValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456790, -1234.123456789)]
    [InlineData(-3.15, -3.14)]
    [InlineData(-1.3334, -1.3333)]
    [InlineData(0, 1)]
    [InlineData(1.224, 1.225)]
    [InlineData(3.624, 3.625)]
    [InlineData(1234.123456788, 1234.123456789)]
    public void GreaterThanOrEqualMultiPart_DoubleOfLessThanValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    [InlineData(123456.123456790, 123456.123456789)]
    public void GreaterThanOrEqual_DecimalOfGreaterThanOrEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    [InlineData(123456.123456790, 123456.123456789)]
    public void GreaterThanOrEqualMultiPart_DecimalOfGreaterThanOrEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456790, -123456.123456789)]
    [InlineData(-1234.123456790, -1234.123456789)]
    [InlineData(-3.15, -3.14)]
    [InlineData(-1.3334, -1.3333)]
    [InlineData(0, 2)]
    [InlineData(1.224, 1.225)]
    [InlineData(3.624, 3.625)]
    [InlineData(1234.123456788, 1234.123456789)]
    [InlineData(123456.123456788, 123456.123456789)]
    public void GreaterThanOrEqual_DecimalOfLessThanValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-123456.123456790, -123456.123456789)]
    [InlineData(-1234.123456790, -1234.123456789)]
    [InlineData(-3.15, -3.14)]
    [InlineData(-1.3334, -1.3333)]
    [InlineData(0, 2)]
    [InlineData(1.224, 1.225)]
    [InlineData(3.624, 3.625)]
    [InlineData(1234.123456788, 1234.123456789)]
    [InlineData(123456.123456788, 123456.123456789)]
    public void GreaterThanOrEqualMultiPart_DecimalOfLessThanValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1022", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("1", "0")]
    [InlineData("2", "1")]
    [InlineData("1023", "1023")]
    [InlineData("123456790", "123456789")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890")]
    public void GreaterThanOrEqual_BigIntegerOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1022", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("1", "0")]
    [InlineData("2", "1")]
    [InlineData("1023", "1023")]
    [InlineData("123456790", "123456789")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890")]
    public void GreaterThanOrEqualMultiPart_BigIntegerOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567892", "-1234567890123456789012345678901234567891")]
    [InlineData("-123456781", "-123456780")]
    [InlineData("-1025", "-1024")]
    [InlineData("-3", "-2")]
    [InlineData("0", "1")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void GreaterThanOrEqual_BigIntegerOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567892", "-1234567890123456789012345678901234567891")]
    [InlineData("-123456781", "-123456780")]
    [InlineData("-1025", "-1024")]
    [InlineData("-3", "-2")]
    [InlineData("0", "1")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void GreaterThanOrEqualMultiPart_BigIntegerOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:02", "00:00:01")]
    [InlineData("00:00:11", "00:00:11")]
    [InlineData("00:11:00", "00:10:00")]
    [InlineData("10:00:02", "10:00:02")]
    [InlineData("01.00:00:02", "01.00:00:01")]
    public void GreaterThanOrEqual_TimeSpanOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01")]
    [InlineData("00:00:11", "00:00:11")]
    [InlineData("00:11:00", "00:10:00")]
    [InlineData("10:00:02", "10:00:02")]
    [InlineData("01.00:00:02", "01.00:00:01")]
    public void GreaterThanOrEqualMultiPart_TimeSpanOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    public void GreaterThanOrEqual_TimeSpanOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    public void GreaterThanOrEqualMultiPart_TimeSpanOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void GreaterThanOrEqual_DateTimeOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void GreaterThanOrEqualMultiPart_DateTimeOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:02")]
    public void GreaterThanOrEqual_DateTimeOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:02")]
    public void GreaterThanOrEqualMultiPart_DateTimeOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void GreaterThanOrEqual_DateTimeOffsetOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void GreaterThanOrEqualMultiPart_DateTimeOffsetOfGreaterThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void GreaterThanOrEqual_DateTimeOffsetOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void GreaterThanOrEqualMultiPart_DateTimeOffsetOfLessThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.GreaterThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion


    public class TestClass : IEquatable<TestClass>
    {
        public TestClass(long id) => Id = id;

        public long Id { get; }

        #region IEquatable

        public override bool Equals(object obj) => obj is TestClass testClass && Equals(testClass);
        public bool Equals(TestClass other) => other is not null && EqualsCore(other);
        bool EqualsCore(TestClass other) => Id == other.Id;

        public static bool operator ==(TestClass left, TestClass right) =>
            !(left is null ^ right is null) && (left is null || left.EqualsCore(right!));
        public static bool operator !=(TestClass left, TestClass right) => !(left == right);

        public override int GetHashCode() => HashCode.Combine(Id);

        #endregion
    }

    public class TestClassComparer : IComparer<TestClass>
    {
        public readonly static IComparer<TestClass> Default = new TestClassComparer();

        public int Compare(TestClass x, TestClass y)
        {
            if (x.Id < y.Id)
                return -1;
            else if (x.Id > y.Id)
                return 1;
            else
                return 0;
        }
    }
}
