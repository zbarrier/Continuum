using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class LessThanExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string LessThanErrorFormat = "'{0}' must be less than '{1}'.";

    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region T

    [Theory]
    [InlineData(-100001, -100000)]
    [InlineData(-101, -100)]
    [InlineData(0, 1)]
    [InlineData(100, 101)]
    [InlineData(100000, 100001)]
    public void LessThan_ClassOfLessThanValue_Success(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, TestClassComparer.Default, MyPropertyName);

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
    public void LessThan_ClassOfPreviousError_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .LessThan(expectedValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-100001, -100000)]
    [InlineData(-101, -100)]
    [InlineData(0, 1)]
    [InlineData(100, 101)]
    [InlineData(100000, 100001)]
    public void LessThanMultiPart_ClassOfLessThanValue_Success(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void LessThanMultiPart_ClassOfPreviousError_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .LessThan(expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-100000, -100001)]
    [InlineData(-100, -100)]
    [InlineData(1, 0)]
    [InlineData(100, 100)]
    [InlineData(100001, 100000)]
    public void LessThan_ClassOfGreaterThanOrEqualValue_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-100000, -100001)]
    [InlineData(-100, -100)]
    [InlineData(1, 0)]
    [InlineData(100, 100)]
    [InlineData(100001, 100000)]
    public void LessThanMultiPart_ClassOfGreaterThanOrEqualValue_Failure(long id, long expectedId)
    {
        //Arrange
        var value = new TestClass(id);
        var expectedValue = new TestClass(expectedId);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("123", "1234")]
    [InlineData("123.3", "123.4")]
    public void LessThan_StringOfLessThanValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("123", "1234")]
    [InlineData("123.3", "123.4")]
    public void LessThan_StringOfPreviousError_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("123", "1234")]
    [InlineData("123.3", "123.4")]
    public void LessThanMultiPart_StringOfLessThanValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("a", "z")]
    [InlineData("b", "w")]
    [InlineData("123", "1234")]
    [InlineData("123.3", "123.4")]
    public void LessThanMultiPart_StringOfPreviousError_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("z", "a")]
    [InlineData("w", "b")]
    [InlineData("123", "123")]
    [InlineData("1234", "123")]
    [InlineData("123.4", "123.3")]
    public void LessThan_StringOfGreaterThanOrEqualValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("a", "a")]
    [InlineData("z", "a")]
    [InlineData("w", "b")]
    [InlineData("123", "123")]
    [InlineData("1234", "123")]
    [InlineData("123.4", "123.3")]
    public void LessThanMultiPart_StringOfGreaterThanOrEqualValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void LessThan_ByteOfLessThanValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

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
    public void LessThan_ByteOfPreviousError_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void LessThanMultiPart_ByteOfLessThanValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void LessThanMultiPart_ByteOfPreviousError_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 2)]
    [InlineData(13, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 254)]
    public void LessThan_ByteOfGreaterThanOrEqualValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 2)]
    [InlineData(13, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 254)]
    public void LessThanMultiPart_ByteOfGreaterThanOrEqualValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void LessThan_ShortOfLessThanValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void LessThan_ShortOfPreviousError_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void LessThanMultiPart_ShortOfLessThanValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void LessThanMultiPart_ShortOfPreviousError_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_766)]
    public void LessThan_ShortOfGreaterThanOrEqualValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_766)]
    public void LessThanMultiPart_ShortOfGreaterThanOrEqualValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void LessThan_IntOfLessThanValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void LessThan_IntOfPreviousError_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void LessThanMultiPart_IntOfLessThanValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void LessThanMultiPart_IntOfPreviousError_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_646)]
    public void LessThan_IntOfGreaterThanOrEqualValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_768)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_646)]
    public void LessThanMultiPart_IntOfGreaterThanOrEqualValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region long

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
    public void LessThan_LongOfLessThanValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

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
    public void LessThan_LongOfPreviousError_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
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
    public void LessThanMultiPart_LongOfLessThanValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void LessThanMultiPart_LongOfPreviousError_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_767)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_648, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_806)]
    public void LessThan_LongOfGreaterThanOrEqualValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_647, -2_147_483_648)]
    [InlineData(-32_767, -32_767)]
    [InlineData(1, 0)]
    [InlineData(257, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_768, 32_767)]
    [InlineData(2_147_483_648, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_806)]
    public void LessThanMultiPart_LongOfGreaterThanOrEqualValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void LessThan_FloatOfLessThanValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void LessThan_FloatOfPreviousError_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void LessThanMultiPart_FloatOfLessThanValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void LessThanMultiPart_FloatOfPreviousError_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.626, 3.625)]
    public void LessThan_FloatOfGreaterThanOrEqualValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.626, 3.625)]
    public void LessThanMultiPart_FloatOfGreaterThanOrEqualValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    public void LessThan_DoubleOfLessThanValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    public void LessThan_DoubleOfPreviousError_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    public void LessThanMultiPart_DoubleOfLessThanValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    public void LessThanMultiPart_DoubleOfPreviousError_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.626)]
    [InlineData(1234.123456790, 1234.123456789)]
    public void LessThan_DoubleOfGreaterThanOrEqualValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456788, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.626, 3.626)]
    [InlineData(1234.123456790, 1234.123456789)]
    public void LessThanMultiPart_DoubleOfGreaterThanOrEqualValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456789, -123456.123456788)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    [InlineData(123456.123456789, 123456.123456790)]
    public void LessThan_DecimalOfLessThanValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456788)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    [InlineData(123456.123456789, 123456.123456790)]
    public void LessThan_DecimalOfPreviousError_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456788)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    [InlineData(123456.123456789, 123456.123456790)]
    public void LessThanMultiPart_DecimalOfLessThanValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456788)]
    [InlineData(-1234.123456789, -1234.123456788)]
    [InlineData(-3.14, -3.13)]
    [InlineData(-1.3333, -1.3332)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456790)]
    [InlineData(123456.123456789, 123456.123456790)]
    public void LessThanMultiPart_DecimalOfPreviousError_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456790, 1234.123456789)]
    [InlineData(123456.123456790, 123456.123456789)]
    public void LessThan_DecimalOfGreaterThanOrEqualValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.13, -3.14)]
    [InlineData(-1.3332, -1.3333)]
    [InlineData(1, 0)]
    [InlineData(1.226, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456790, 1234.123456789)]
    [InlineData(123456.123456790, 123456.123456789)]
    public void LessThanMultiPart_DecimalOfGreaterThanOrEqualValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567889")]
    [InlineData("-123456789", "-123456788")]
    [InlineData("-1023", "-1022")]
    [InlineData("-1", "0")]
    [InlineData("0", "1")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void LessThan_BigIntegerOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567889")]
    [InlineData("-123456789", "-123456788")]
    [InlineData("-1023", "-1022")]
    [InlineData("-1", "0")]
    [InlineData("0", "1")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void LessThan_BigIntegerOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567889")]
    [InlineData("-123456789", "-123456788")]
    [InlineData("-1023", "-1022")]
    [InlineData("-1", "0")]
    [InlineData("0", "1")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void LessThanMultiPart_BigIntegerOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567889")]
    [InlineData("-123456789", "-123456788")]
    [InlineData("-1023", "-1022")]
    [InlineData("-1", "0")]
    [InlineData("0", "1")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456790")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void LessThanMultiPart_BigIntegerOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1022", "-1023")]
    [InlineData("0", "-1")]
    [InlineData("0", "0")]
    [InlineData("2", "1")]
    [InlineData("1024", "1023")]
    [InlineData("123456790", "123456790")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890")]
    public void LessThan_BigIntegerOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1022", "-1023")]
    [InlineData("0", "-1")]
    [InlineData("0", "0")]
    [InlineData("2", "1")]
    [InlineData("1024", "1023")]
    [InlineData("123456790", "123456790")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890")]
    public void LessThanMultiPart_BigIntegerOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    public void LessThan_TimeSpanOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

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
    public void LessThan_TimeSpanOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    public void LessThanMultiPart_TimeSpanOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void LessThanMultiPart_TimeSpanOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01")]
    [InlineData("00:00:11", "00:00:11")]
    [InlineData("00:11:00", "00:10:00")]
    [InlineData("10:00:02", "10:00:02")]
    [InlineData("01.00:00:02", "01.00:00:01")]
    public void LessThan_TimeSpanOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01")]
    [InlineData("00:00:11", "00:00:11")]
    [InlineData("00:11:00", "00:10:00")]
    [InlineData("10:00:02", "10:00:02")]
    [InlineData("01.00:00:02", "01.00:00:01")]
    public void LessThanMultiPart_TimeSpanOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01")]
    public void LessThan_DateTimeOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01")]
    public void LessThan_DateTimeOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01")]
    public void LessThanMultiPart_DateTimeOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01")]
    public void LessThanMultiPart_DateTimeOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:02")]
    public void LessThan_DateTimeOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:02")]
    public void LessThanMultiPart_DateTimeOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void LessThan_DateTimeOffsetOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void LessThan_DateTimeOffsetOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void LessThanMultiPart_DateTimeOffsetOfLessThanValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void LessThanMultiPart_DateTimeOffsetOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void LessThan_DateTimeOffsetOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .LessThan(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, LessThanErrorFormat, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void LessThanMultiPart_DateTimeOffsetOfGreaterThanOrEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .LessThan(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, LessThanErrorFormat, multiPartPropertyName, expectedValue));
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
