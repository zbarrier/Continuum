using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class LessThanOrEqualTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    #region T

    [Theory]
    [InlineData(-100001, -100000)]
    [InlineData(-101, -100)]
    [InlineData(-50, -50)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(50, 50)]
    [InlineData(100, 101)]
    [InlineData(100000, 100001)]
    public void LessThanOrEqual_ClassOfLessThanOrEqualValue_Success(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100001, -100000)]
    [InlineData(-101, -100)]
    [InlineData(-50, -50)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(50, 50)]
    [InlineData(100, 101)]
    [InlineData(100000, 100001)]
    public void LessThanOrEqualMultiPart_ClassOfLessThanOrEqualValue_Success(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100000, -100001)]
    [InlineData(-100, -101)]
    [InlineData(1, 0)]
    [InlineData(101, 100)]
    [InlineData(100001, 100000)]
    public void LessThanOrEqual_ClassOfGreaterThanValue_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    [Theory]
    [InlineData(-100000, -100001)]
    [InlineData(-100, -101)]
    [InlineData(1, 0)]
    [InlineData(101, 100)]
    [InlineData(100001, 100000)]
    public void LessThanOrEqualMultiPart_ClassOfGreaterThanValue_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("a", "a")]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("test", "test")]
    [InlineData("123", "123")]
    [InlineData("123", "1234")]
    [InlineData("123.3", "123.4")]
    public void LessThanOrEqual_StringOfLessThanOrEqualValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("test", "test")]
    [InlineData("123", "123")]
    [InlineData("123", "1234")]
    [InlineData("123.3", "123.4")]
    public void LessThanOrEqualMultiPart_StringOfLessThanOrEqualValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("z", "a")]
    [InlineData("w", "b")]
    [InlineData("1234", "123")]
    [InlineData("123.4", "123.3")]
    public void LessThanOrEqual_StringOfGreaterThanValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("z", "a")]
    [InlineData("w", "b")]
    [InlineData("1234", "123")]
    [InlineData("123.4", "123.3")]
    public void LessThanOrEqualMultiPart_StringOfGreaterThanValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(50, 50)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void LessThanOrEqual_ByteOfLessThanOrEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(50, 50)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void LessThanOrEqualMultiPart_ByteOfLessThanOrEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(13, 12)]
    [InlineData(128, 127)]
    [InlineData(255, 254)]
    public void LessThanOrEqual_ByteOfGreaterThanValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(13, 12)]
    [InlineData(128, 127)]
    [InlineData(255, 254)]
    public void LessThanOrEqualMultiPart_ByteOfGreaterThanValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(500, 500)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    [InlineData(32_767, 32_767)]
    public void LessThanOrEqual_ShortOfLessThanOrEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(500, 500)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    [InlineData(32_767, 32_767)]
    public void LessThanOrEqualMultiPart_ShortOfLessThanOrEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(32_767, 32_766)]
    public void LessThanOrEqual_ShortOfGreaterThanValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(32_767, 32_766)]
    public void LessThanOrEqualMultiPart_ShortOfGreaterThanValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(5000, 5000)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void LessThanOrEqual_IntOfLessThanOrEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(5000, 5000)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void LessThanOrEqualMultiPart_IntOfLessThanOrEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_646)]
    public void LessThanOrEqual_IntOfGreaterThanValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_646)]
    public void LessThanOrEqualMultiPart_IntOfGreaterThanValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region long

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void LessThanOrEqual_LongOfLessThanOrEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void LessThanOrEqualMultiPart_LongOfLessThanOrEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1024, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_648, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_806)]
    public void LessThanOrEqual_LongOfGreaterThanValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1024, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_648, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_806)]
    public void LessThanOrEqualMultiPart_LongOfGreaterThanValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(1.226, 1.226)]
    [InlineData(3.625, 3.626)]
    public void LessThanOrEqual_FloatOfLessThanOrEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(1.226, 1.226)]
    [InlineData(3.625, 3.626)]
    public void LessThanOrEqualMultiPart_FloatOfLessThanOrEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    public void LessThanOrEqual_FloatOfGreaterThanValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    public void LessThanOrEqualMultiPart_FloatOfGreaterThanValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.626, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    public void LessThanOrEqual_DoubleOfLessThanOrEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.626, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    public void LessThanOrEqualMultiPart_DoubleOfLessThanOrEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    [InlineData(1234.123456790, 1234.123456789)]
    public void LessThanOrEqual_DoubleOfGreaterThanValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    [InlineData(1234.123456790, 1234.123456789)]
    public void LessThanOrEqualMultiPart_DoubleOfGreaterThanValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789)]
    [InlineData(-123456.123456789, -123456.123456788)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456790)]
    [InlineData(123456.123456789, 123456.123456790)]
    public void LessThanOrEqual_DecimalOfLessThanOrEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789)]
    [InlineData(-123456.123456789, -123456.123456788)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456790)]
    [InlineData(123456.123456789, 123456.123456790)]
    public void LessThanOrEqualMultiPart_DecimalOfLessThanOrEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789)]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    [InlineData(1234.123456790, 1234.123456789)]
    [InlineData(123456.123456790, 123456.123456789)]
    public void LessThanOrEqual_DecimalOfGreaterThanValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789)]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.625)]
    [InlineData(1234.123456790, 1234.123456789)]
    [InlineData(123456.123456790, 123456.123456789)]
    public void LessThanOrEqualMultiPart_DecimalOfGreaterThanValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567889")]
    [InlineData("-123456789", "-123456788")]
    [InlineData("-1023", "-1022")]
    [InlineData("-1", "0")]
    [InlineData("0", "0")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("123456790", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567891")]
    public void LessThanOrEqual_BigIntegerOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567889")]
    [InlineData("-123456789", "-123456788")]
    [InlineData("-1023", "-1022")]
    [InlineData("-1", "0")]
    [InlineData("0", "0")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("123456790", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567891")]
    public void LessThanOrEqualMultiPart_BigIntegerOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456788", "-123456789")]
    [InlineData("-1022", "-1023")]
    [InlineData("0", "-1")]
    [InlineData("2", "1")]
    [InlineData("1024", "1023")]
    [InlineData("123456791", "123456790")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890")]
    public void LessThanOrEqual_BigIntegerOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456788", "-123456789")]
    [InlineData("-1022", "-1023")]
    [InlineData("0", "-1")]
    [InlineData("2", "1")]
    [InlineData("1024", "1023")]
    [InlineData("123456791", "123456790")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890")]
    public void LessThanOrEqualMultiPart_BigIntegerOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("00:11:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    [InlineData("01.00:00:02", "01.00:00:02")]
    public void LessThanOrEqual_TimeSpanOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("00:11:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    [InlineData("01.00:00:02", "01.00:00:02")]
    public void LessThanOrEqualMultiPart_TimeSpanOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01")]
    [InlineData("00:00:11", "00:00:10")]
    [InlineData("00:11:00", "00:10:00")]
    [InlineData("10:00:02", "10:00:01")]
    [InlineData("01.00:00:02", "01.00:00:01")]
    public void LessThanOrEqual_TimeSpanOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01")]
    [InlineData("00:00:11", "00:00:10")]
    [InlineData("00:11:00", "00:10:00")]
    [InlineData("10:00:02", "10:00:01")]
    [InlineData("01.00:00:02", "01.00:00:01")]
    public void LessThanOrEqualMultiPart_TimeSpanOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void LessThanOrEqual_DateTimeOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void LessThanOrEqualMultiPart_DateTimeOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01")]
    public void LessThanOrEqual_DateTimeOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01")]
    public void LessThanOrEqualMultiPart_DateTimeOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void LessThanOrEqual_DateTimeOffsetOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void LessThanOrEqualMultiPart_DateTimeOffsetOfLessThanOrEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void LessThanOrEqual_DateTimeOffsetOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void LessThanOrEqualMultiPart_DateTimeOffsetOfGreaterThanValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.LessThanOrEqual(value, expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, multiPartPropertyName, expectedValue));
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
