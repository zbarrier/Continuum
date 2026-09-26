using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class InclusiveBetweenTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string InclusiveBetweenErrorFormat = "'{0}' must be between {1} and {2}. You entered {3}.";

    #region T

    [Theory]
    [InlineData(-100002, -100002, -10000)]
    [InlineData(-99, -101, -99)]
    [InlineData(1, 0, 2)]
    [InlineData(101, 100, 102)]
    [InlineData(102, 100, 102)]
    [InlineData(100000, 100000, 100002)]
    public void InclusiveBetween_ClassOfInclusiveBetweenValue_Success(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.InclusiveBetween(value, fromValue, toValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100002, -100002, -10000)]
    [InlineData(-99, -101, -99)]
    [InlineData(1, 0, 2)]
    [InlineData(101, 100, 102)]
    [InlineData(102, 100, 102)]
    [InlineData(100000, 100000, 100002)]
    public void InclusiveBetweenMultiPart_ClassOfInclusiveBetweenValue_Success(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.InclusiveBetween(value, fromValue, toValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100003, -100002, -10000)]
    [InlineData(-98, -101, -99)]
    [InlineData(-1, 0, 2)]
    [InlineData(99, 100, 102)]
    [InlineData(100003, 100000, 100002)]
    public void InclusiveBetween_ClassOfOutsideValue_Failure(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.InclusiveBetween(value, fromValue, toValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, fromValue, toValue, value));
    }

    [Theory]
    [InlineData(-100003, -100002, -10000)]
    [InlineData(-98, -101, -99)]
    [InlineData(-1, 0, 2)]
    [InlineData(99, 100, 102)]
    [InlineData(100003, 100000, 100002)]
    public void InclusiveBetweenMultiPart_ClassOfOutsideValue_Failure(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.InclusiveBetween(value, fromValue, toValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, fromValue, toValue, value));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("taat", "taat", "tzzt")]
    [InlineData("test", "taat", "tzzt")]
    [InlineData("tzzt", "taat", "tzzt")]
    [InlineData("111", "111", "333")]
    [InlineData("123", "111", "333")]
    [InlineData("333", "111", "333")]
    public void InclusiveBetween_StringOfInclusiveBetweenValue_Success(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("taat", "taat", "tzzt")]
    [InlineData("test", "taat", "tzzt")]
    [InlineData("tzzt", "taat", "tzzt")]
    [InlineData("111", "111", "333")]
    [InlineData("123", "111", "333")]
    [InlineData("333", "111", "333")]
    public void InclusiveBetweenMultiPart_StringOfInclusiveBetweenValue_Success(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("taas", "taat", "tzzt")]
    [InlineData("tzzu", "taat", "tzzt")]
    [InlineData("110", "111", "333")]
    [InlineData("334", "111", "333")]
    public void InclusiveBetween_StringOfOutsideValue_Failure(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("taas", "taat", "tzzt")]
    [InlineData("tzzu", "taat", "tzzt")]
    [InlineData("110", "111", "333")]
    [InlineData("334", "111", "333")]
    public void InclusiveBetweenMultiPart_StringOfOutsideValue_Failure(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(100, 100, 200)]
    [InlineData(128, 100, 200)]
    [InlineData(200, 100, 200)]
    [InlineData(254, 253, 255)]
    public void InclusiveBetween_ByteOfInclusiveBetweenValue_Success(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(100, 100, 200)]
    [InlineData(128, 100, 200)]
    [InlineData(200, 100, 200)]
    [InlineData(254, 253, 255)]
    public void InclusiveBetweenMultiPart_ByteOfInclusiveBetweenValue_Success(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 1, 3)]
    [InlineData(4, 1, 3)]
    [InlineData(99, 100, 200)]
    [InlineData(201, 100, 200)]
    [InlineData(255, 252, 254)]
    public void InclusiveBetween_ByteOfOutsideValue_Failure(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(0, 1, 3)]
    [InlineData(4, 1, 3)]
    [InlineData(99, 100, 200)]
    [InlineData(201, 100, 200)]
    [InlineData(255, 252, 254)]
    public void InclusiveBetweenMultiPart_ByteOfOutsideValue_Failure(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768, -32_768, -32_766)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(-32_766, -32_768, -32_766)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1000, 1000, 2000)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(2000, 1000, 2000)]
    [InlineData(32_700, 30_000, 32_701)]
    public void InclusiveBetween_ShortOfInclusiveBetweenValue_Success(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_768, -32_766)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(-32_766, -32_768, -32_766)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1000, 1000, 2000)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(2000, 1000, 2000)]
    [InlineData(32_700, 30_000, 32_701)]
    public void InclusiveBetweenMultiPart_ShortOfInclusiveBetweenValue_Success(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_767, -32_765)]
    [InlineData(-32_764, -32_767, -32_765)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(255, 256, 258)]
    [InlineData(999, 1000, 2000)]
    [InlineData(2001, 1000, 2000)]
    [InlineData(32_702, 30_000, 32_701)]
    public void InclusiveBetween_ShortOfOutsideValue_Failure(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-32_768, -32_767, -32_765)]
    [InlineData(-32_764, -32_767, -32_765)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(255, 256, 258)]
    [InlineData(999, 1000, 2000)]
    [InlineData(2001, 1000, 2000)]
    [InlineData(32_702, 30_000, 32_701)]
    public void InclusiveBetweenMultiPart_ShortOfOutsideValue_Failure(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648, -2_147_483_646)]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-2_147_483_646, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(30_000, 30_000, 33_000)]
    [InlineData(32_700, 30_000, 33_000)]
    [InlineData(33_000, 30_000, 33_000)]
    [InlineData(2_147_483_646, 2_147_483_645, 2_147_483_647)]
    public void InclusiveBetween_IntOfInclusiveBetweenValue_Success(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648, -2_147_483_646)]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-2_147_483_646, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(30_000, 30_000, 33_000)]
    [InlineData(32_700, 30_000, 33_000)]
    [InlineData(33_000, 30_000, 33_000)]
    [InlineData(2_147_483_646, 2_147_483_645, 2_147_483_647)]
    public void InclusiveBetweenMultiPart_IntOfInclusiveBetweenValue_Success(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647, -2_147_483_645)]
    [InlineData(-2_147_483_644, -2_147_483_647, -2_147_483_645)]
    [InlineData(-32_769, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(259, 256, 258)]
    [InlineData(3000, 1000, 2000)]
    [InlineData(29_999, 30_000, 33_000)]
    [InlineData(33_001, 30_000, 33_000)]
    [InlineData(2_147_483_644, 2_147_483_645, 2_147_483_647)]
    public void InclusiveBetween_IntOfOutsideValue_Failure(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647, -2_147_483_645)]
    [InlineData(-2_147_483_644, -2_147_483_647, -2_147_483_645)]
    [InlineData(-32_769, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(259, 256, 258)]
    [InlineData(3000, 1000, 2000)]
    [InlineData(29_999, 30_000, 33_000)]
    [InlineData(33_001, 30_000, 33_000)]
    [InlineData(2_147_483_644, 2_147_483_645, 2_147_483_647)]
    public void InclusiveBetweenMultiPart_IntOfOutsideValue_Failure(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region long

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-9_223_372_036_854_775_806, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1000, 1000, 5000)]
    [InlineData(1024, 1000, 5000)]
    [InlineData(5000, 1000, 5000)]
    [InlineData(32_768, 30_000, 35_000)]
    [InlineData(2_147_483_648, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void InclusiveBetween_LongOfInclusiveBetweenValue_Success(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-9_223_372_036_854_775_806, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1000, 1000, 5000)]
    [InlineData(1024, 1000, 5000)]
    [InlineData(5000, 1000, 5000)]
    [InlineData(32_768, 30_000, 35_000)]
    [InlineData(2_147_483_648, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void InclusiveBetweenMultiPart_LongOfInclusiveBetweenValue_Success(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807, -9_223_372_036_854_775_805)]
    [InlineData(-9_223_372_036_854_775_804, -9_223_372_036_854_775_807, -9_223_372_036_854_775_805)]
    [InlineData(-2_147_483_649, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_765, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(259, 256, 258)]
    [InlineData(999, 1000, 5000)]
    [InlineData(5001, 1000, 5000)]
    [InlineData(35_001, 30_000, 35_000)]
    [InlineData(2_147_483_650, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_804, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void InclusiveBetween_LongOfOutsideValue_Failure(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807, -9_223_372_036_854_775_805)]
    [InlineData(-9_223_372_036_854_775_804, -9_223_372_036_854_775_807, -9_223_372_036_854_775_805)]
    [InlineData(-2_147_483_649, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_765, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(259, 256, 258)]
    [InlineData(999, 1000, 5000)]
    [InlineData(5001, 1000, 5000)]
    [InlineData(35_001, 30_000, 35_000)]
    [InlineData(2_147_483_650, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_804, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void InclusiveBetweenMultiPart_LongOfOutsideValue_Failure(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.14, -3.12)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-3.12, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    public void InclusiveBetween_FloatOfInclusiveBetweenValue_Success(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.14, -3.12)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-3.12, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    public void InclusiveBetweenMultiPart_FloatOfInclusiveBetweenValue_Success(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.15, -3.14, -3.12)]
    [InlineData(-3.11, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.224, 1.225, 1.227)]
    [InlineData(3.628, 3.625, 3.627)]
    public void InclusiveBetween_FloatOfOutsideValue_Failure(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-3.15, -3.14, -3.12)]
    [InlineData(-3.11, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.224, 1.225, 1.227)]
    [InlineData(3.628, 3.625, 3.627)]
    public void InclusiveBetweenMultiPart_FloatOfOutsideValue_Failure(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789, -1234.123456787)]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-1234.123456787, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456789, 1234.123456789, 1234.123456791)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    [InlineData(1234.123456791, 1234.123456789, 1234.123456791)]
    public void InclusiveBetween_DoubleOfInclusiveBetweenValue_Success(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789, -1234.123456787)]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-1234.123456787, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456789, 1234.123456789, 1234.123456791)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    [InlineData(1234.123456791, 1234.123456789, 1234.123456791)]
    public void InclusiveBetweenMultiPart_DoubleOfInclusiveBetweenValue_Success(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456790, -1234.123456789, -1234.123456787)]
    [InlineData(-1234.123456786, -1234.123456789, -1234.123456787)]
    [InlineData(-3.11, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    [InlineData(1234.123456788, 1234.123456789, 1234.123456791)]
    [InlineData(1234.123456792, 1234.123456789, 1234.123456791)]
    public void InclusiveBetween_DoubleOfOutsideValue_Failure(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-1234.123456790, -1234.123456789, -1234.123456787)]
    [InlineData(-1234.123456786, -1234.123456789, -1234.123456787)]
    [InlineData(-3.11, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    [InlineData(1234.123456788, 1234.123456789, 1234.123456791)]
    [InlineData(1234.123456792, 1234.123456789, 1234.123456791)]
    public void InclusiveBetweenMultiPart_DoubleOfOutsideValue_Failure(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789, -123456.123456787)]
    [InlineData(-123456.123456788, -123456.123456789, -123456.123456787)]
    [InlineData(-123456.123456787, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456789, 123456.123456789, 123456.123456791)]
    [InlineData(123456.123456790, 123456.123456789, 123456.123456791)]
    [InlineData(123456.123456791, 123456.123456789, 123456.123456791)]
    public void InclusiveBetween_DecimalOfInclusiveBetweenValue_Success(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789, -123456.123456787)]
    [InlineData(-123456.123456788, -123456.123456789, -123456.123456787)]
    [InlineData(-123456.123456787, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(0, 0, 2)]
    [InlineData(1, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456789, 123456.123456789, 123456.123456791)]
    [InlineData(123456.123456790, 123456.123456789, 123456.123456791)]
    [InlineData(123456.123456791, 123456.123456789, 123456.123456791)]
    public void InclusiveBetweenMultiPart_DecimalOfInclusiveBetweenValue_Success(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456790, -123456.123456789, -123456.123456787)]
    [InlineData(-123456.123456786, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456790, -1234.123456789, -1234.123456787)]
    [InlineData(-3.15, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    [InlineData(1234.123456792, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456788, 123456.123456789, 123456.123456791)]
    [InlineData(123456.123456792, 123456.123456789, 123456.123456791)]
    public void InclusiveBetween_DecimalOfOutsideValue_Failure(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-123456.123456790, -123456.123456789, -123456.123456787)]
    [InlineData(-123456.123456786, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456786, -1234.123456789, -1234.123456787)]
    [InlineData(-3.15, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    [InlineData(1234.123456792, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456788, 123456.123456789, 123456.123456791)]
    [InlineData(123456.123456792, 123456.123456789, 123456.123456791)]
    public void InclusiveBetweenMultiPart_DecimalOfOutsideValue_Failure(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-1234567890123456789012345678901234567888", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456788", "-123456789", "-123456787")]
    [InlineData("-1022", "-1023", "-1021")]
    [InlineData("0", "0", "2")]
    [InlineData("1", "0", "2")]
    [InlineData("2", "0", "2")]
    [InlineData("1024", "1000", "2000")]
    [InlineData("123456790", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    [InlineData("1234567890123456789012345678901234567892", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void InclusiveBetween_BigIntegerOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-1234567890123456789012345678901234567888", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456788", "-123456789", "-123456787")]
    [InlineData("-1022", "-1023", "-1021")]
    [InlineData("0", "0", "2")]
    [InlineData("1", "0", "2")]
    [InlineData("2", "0", "2")]
    [InlineData("1024", "1000", "2000")]
    [InlineData("123456790", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    [InlineData("1234567890123456789012345678901234567892", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void InclusiveBetweenMultiPart_BigIntegerOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567891", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-1234567890123456789012345678901234567887", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456786", "-123456789", "-123456787")]
    [InlineData("-1020", "-1023", "-1021")]
    [InlineData("-1", "0", "2")]
    [InlineData("3", "0", "2")]
    [InlineData("999", "1000", "2000")]
    [InlineData("300000000", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567889", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    [InlineData("1234567890123456789012345678901234567893", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void InclusiveBetween_BigIntegerOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567891", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-1234567890123456789012345678901234567887", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456786", "-123456789", "-123456787")]
    [InlineData("-1020", "-1023", "-1021")]
    [InlineData("-1", "0", "2")]
    [InlineData("3", "0", "2")]
    [InlineData("999", "1000", "2000")]
    [InlineData("300000000", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567889", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    [InlineData("1234567890123456789012345678901234567893", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void InclusiveBetweenMultiPart_BigIntegerOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01", "00:00:01", "00:00:03")]
    [InlineData("00:00:02", "00:00:01", "00:00:03")]
    [InlineData("00:00:03", "00:00:01", "00:00:03")]
    [InlineData("00:00:11", "00:00:10", "00:00:12")]
    [InlineData("00:11:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:02", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:01", "01.00:00:01", "01.00:00:03")]
    [InlineData("01.00:00:02", "01.00:00:01", "01.00:00:03")]
    [InlineData("01.00:00:03", "01.00:00:01", "01.00:00:03")]
    public void InclusiveBetween_TimeSpanOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01", "00:00:03")]
    [InlineData("00:00:02", "00:00:01", "00:00:03")]
    [InlineData("00:00:03", "00:00:01", "00:00:03")]
    [InlineData("00:00:11", "00:00:10", "00:00:12")]
    [InlineData("00:11:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:02", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:01", "01.00:00:01", "01.00:00:03")]
    [InlineData("01.00:00:02", "01.00:00:01", "01.00:00:03")]
    [InlineData("01.00:00:03", "01.00:00:01", "01.00:00:03")]
    public void InclusiveBetweenMultiPart_TimeSpanOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:00", "00:00:01", "00:00:03")]
    [InlineData("00:00:04", "00:00:01", "00:00:03")]
    [InlineData("00:00:09", "00:00:10", "00:00:12")]
    [InlineData("00:13:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:04", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:00", "01.00:00:01", "01.00:00:03")]
    [InlineData("01.00:00:04", "01.00:00:01", "01.00:00:03")]
    public void InclusiveBetween_TimeSpanOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("00:00:00", "00:00:01", "00:00:03")]
    [InlineData("00:00:04", "00:00:01", "00:00:03")]
    [InlineData("00:00:09", "00:00:10", "00:00:12")]
    [InlineData("00:13:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:04", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:00", "01.00:00:01", "01.00:00:03")]
    [InlineData("01.00:00:04", "01.00:00:01", "01.00:00:03")]
    public void InclusiveBetweenMultiPart_TimeSpanOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:03", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    [InlineData("2023-05-01 10:00:03", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void InclusiveBetween_DateTimeOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:03", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    [InlineData("2023-05-01 10:00:03", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void InclusiveBetweenMultiPart_DateTimeOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:00", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:04", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:09", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:03", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:04", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void InclusiveBetween_DateTimeOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:00", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:04", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:09", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:03", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:04", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    [InlineData("2023-05-01 10:00:00", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void InclusiveBetweenMultiPart_DateTimeOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:03 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    [InlineData("2023-05-01 10:00:03 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void InclusiveBetween_DateTimeOffsetOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:03 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    [InlineData("2023-05-01 10:00:03 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void InclusiveBetweenMultiPart_DateTimeOffsetOfInclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:00 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:04 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:09 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:03 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:00 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    [InlineData("2023-05-01 10:00:04 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void InclusiveBetween_DateTimeOffsetOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, InclusiveBetweenErrorFormat, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:00 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:04 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:09 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:03 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:00 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    [InlineData("2023-05-01 10:00:04 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void InclusiveBetweenMultiPart_DateTimeOffsetOfOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.InclusiveBetween(value, from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, InclusiveBetweenErrorFormat, multiPartPropertyName, from, to, value));
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
