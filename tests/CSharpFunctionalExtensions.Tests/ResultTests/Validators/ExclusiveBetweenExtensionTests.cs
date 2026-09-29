using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class ExclusiveBetweenExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region T

    [Theory]
    [InlineData(-100001, -100002, -10000)]
    [InlineData(-100, -101, -99)]
    [InlineData(1, 0, 2)]
    [InlineData(101, 100, 102)]
    [InlineData(100001, 100000, 100002)]
    public void ExclusiveBetween_ClassOfExclusiveBetweenValue_Success(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(fromValue, toValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100001, -100002, -10000)]
    [InlineData(-100, -101, -99)]
    [InlineData(1, 0, 2)]
    [InlineData(101, 100, 102)]
    [InlineData(100001, 100000, 100002)]
    public void ExclusiveBetween_ClassOfPreviousError_Failure(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .ExclusiveBetween(fromValue, toValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-100001, -100002, -10000)]
    [InlineData(-100, -101, -99)]
    [InlineData(1, 0, 2)]
    [InlineData(101, 100, 102)]
    [InlineData(100001, 100000, 100002)]
    public void ExclusiveBetweenMultiPart_ClassOfExlusiveBetweenValue_Success(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(fromValue, toValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-100001, -100002, -10000)]
    [InlineData(-100, -101, -99)]
    [InlineData(1, 0, 2)]
    [InlineData(101, 100, 102)]
    [InlineData(100001, 100000, 100002)]
    public void ExclusiveBetweenMultiPart_ClassOfPreviousError_Failure(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .ExclusiveBetween(fromValue, toValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-100003, -100002, -10000)]
    [InlineData(-101, -101, -99)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(99, 100, 102)]
    [InlineData(100003, 100000, 100002)]
    public void ExclusiveBetween_ClassOfEqualOrOutsideValue_Failure(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(fromValue, toValue, TestClassComparer.Default, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, ErrorArgument.FromObject(fromValue), ErrorArgument.FromObject(toValue), ErrorArgument.FromObject(value)));
    }

    [Theory]
    [InlineData(-100003, -100002, -10000)]
    [InlineData(-101, -101, -99)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(99, 100, 102)]
    [InlineData(100003, 100000, 100002)]
    public void ExclusiveBetweenMultiPart_ClassOfEqualOrOutsideValue_Failure(long id, long from, long to)
    {
        //Arrange
        var value = new TestClass(id);
        var fromValue = new TestClass(from);
        var toValue = new TestClass(to);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(fromValue, toValue, TestClassComparer.Default, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, ErrorArgument.FromObject(fromValue), ErrorArgument.FromObject(toValue), ErrorArgument.FromObject(value)));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("test", "taat", "tzzt")]
    [InlineData("123", "111", "333")]
    public void ExclusiveBetween_StringOfExclusiveBetweenValue_Success(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("taat", "tzzt")]
    [InlineData("111", "333")]
    public void ExclusiveBetween_StringOfPreviousError_Failure(string from, string to)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("test", "taat", "tzzt")]
    [InlineData("123", "111", "333")]
    public void ExclusiveBetweenMultiPart_StringOfExclusiveBetweenValue_Success(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("taat", "tzzt")]
    [InlineData("111", "333")]
    public void ExclusiveBetweenMultiPart_StringOfPreviousError_Failure(string from, string to)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("saat", "taat", "tzzt")]
    [InlineData("taat", "taat", "tzzt")]
    [InlineData("110", "111", "333")]
    [InlineData("111", "111", "333")]
    [InlineData("333", "111", "333")]
    [InlineData("334", "111", "333")]
    public void ExclusiveBetween_StringOfEqualOrOutsideValue_Failure(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("saat", "taat", "tzzt")]
    [InlineData("taat", "taat", "tzzt")]
    [InlineData("110", "111", "333")]
    [InlineData("111", "111", "333")]
    [InlineData("333", "111", "333")]
    [InlineData("334", "111", "333")]
    public void ExclusiveBetweenMultiPart_StringOfEqualOrOutsideValue_Failure(string value, string from, string to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(1, 0, 2)]
    [InlineData(2, 1, 3)]
    [InlineData(13, 12, 14)]
    [InlineData(128, 100, 200)]
    [InlineData(254, 200, 255)]
    public void ExclusiveBetween_ByteOfExclusiveBetweenValue_Success(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 3)]
    [InlineData(12, 14)]
    [InlineData(100, 200)]
    [InlineData(200, 255)]
    public void ExclusiveBetween_ByteOfPreviousError_Failure(byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 0, 2)]
    [InlineData(2, 1, 3)]
    [InlineData(13, 12, 14)]
    [InlineData(128, 100, 200)]
    [InlineData(254, 200, 255)]
    public void ExclusiveBetweenMultiPart_ByteOfExclusiveBetweenValue_Success(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 3)]
    [InlineData(12, 14)]
    [InlineData(100, 200)]
    [InlineData(200, 255)]
    public void ExclusiveBetweenMultiPart_ByteOfPreviousError_Failure(byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(0, 0, 2)]
    [InlineData(0, 1, 3)]
    [InlineData(14, 12, 14)]
    [InlineData(201, 100, 200)]
    [InlineData(199, 200, 255)]
    public void ExclusiveBetween_ByteOfEqualOrOutsideValue_Failure(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(0, 0, 2)]
    [InlineData(0, 1, 3)]
    [InlineData(14, 12, 14)]
    [InlineData(201, 100, 200)]
    [InlineData(199, 200, 255)]
    public void ExclusiveBetweenMultiPart_ByteOfEqualOrOutsideValue_Failure(byte value, byte from, byte to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(1, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(32_700, 30_000, 32_701)]
    public void ExclusiveBetween_ShortOfExclusiveBetweenValue_Success(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_766)]
    [InlineData(0, 2)]
    [InlineData(256, 258)]
    [InlineData(1000, 2000)]
    [InlineData(30_000, 32_701)]
    public void ExclusiveBetween_ShortOfPreviousError_Failure(short from, short to)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(1, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(32_700, 30_000, 32_701)]
    public void ExclusiveBetweenMultiPart_ShortOfExclusiveBetweenValue_Success(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_766)]
    [InlineData(0, 2)]
    [InlineData(256, 258)]
    [InlineData(1000, 2000)]
    [InlineData(30_000, 32_701)]
    public void ExclusiveBetweenMultiPart_ShortOfPreviousError_Failure(short from, short to)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_768, -32_767, -32_765)]
    [InlineData(-32_767, -32_767, -32_765)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(199, 200, 300)]
    [InlineData(2001, 1000, 2000)]
    [InlineData(29_999, 30_000, 32_000)]
    public void ExclusiveBetween_ShortOfEqualOrOutsideValue_Failure(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-32_768, -32_767, -32_765)]
    [InlineData(-32_767, -32_767, -32_765)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(199, 200, 300)]
    [InlineData(2001, 1000, 2000)]
    [InlineData(29_999, 30_000, 32_000)]
    public void ExclusiveBetweenMultiPart_ShortOfEqualOrOutsideValue_Failure(short value, short from, short to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(1, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(32_700, 30_000, 33_000)]
    [InlineData(2_147_483_646, 2_147_483_645, 2_147_483_647)]
    public void ExclusiveBetween_IntOfExclusiveBetweenValue_Success(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_646)]
    [InlineData(-32_768, -32_766)]
    [InlineData(0, 2)]
    [InlineData(256, 258)]
    [InlineData(1000, 2000)]
    [InlineData(30_000, 33_000)]
    [InlineData(2_147_483_645, 2_147_483_647)]
    public void ExclusiveBetween_IntOfPreviousError_Failure(int from, int to)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(1, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 2000)]
    [InlineData(32_700, 30_000, 33_000)]
    [InlineData(2_147_483_646, 2_147_483_645, 2_147_483_647)]
    public void ExclusiveBetweenMultiPart_IntOfExclusiveBetweenValue_Success(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_646)]
    [InlineData(-32_768, -32_766)]
    [InlineData(0, 2)]
    [InlineData(256, 258)]
    [InlineData(1000, 2000)]
    [InlineData(30_000, 33_000)]
    [InlineData(2_147_483_645, 2_147_483_647)]
    public void ExclusiveBetweenMultiPart_IntOfPreviousError_Failure(int from, int to)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647, -2_147_483_645)]
    [InlineData(-2_147_483_647, -2_147_483_647, -2_147_483_645)]
    [InlineData(-32_766, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(199, 200, 300)]
    [InlineData(2001, 1000, 2000)]
    [InlineData(34_000, 30_000, 33_000)]
    [InlineData(2_147_483_600, 2_147_483_645, 2_147_483_647)]
    public void ExclusiveBetween_IntOfEqualOrOutsideValue_Failure(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647, -2_147_483_645)]
    [InlineData(-2_147_483_647, -2_147_483_647, -2_147_483_645)]
    [InlineData(-32_766, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(199, 200, 300)]
    [InlineData(2001, 1000, 2000)]
    [InlineData(34_000, 30_000, 33_000)]
    [InlineData(2_147_483_600, 2_147_483_645, 2_147_483_647)]
    public void ExclusiveBetweenMultiPart_IntOfEqualOrOutsideValue_Failure(int value, int from, int to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region long

    [Theory]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(1, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 5000)]
    [InlineData(32_768, 30_000, 35_000)]
    [InlineData(2_147_483_648, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void ExclusiveBetween_LongOfExclusiveBetweenValue_Success(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_648, -2_147_483_646)]
    [InlineData(-32_768, -32_766)]
    [InlineData(0, 2)]
    [InlineData(256, 258)]
    [InlineData(1000, 5000)]
    [InlineData(30_000, 35_000)]
    [InlineData(2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void ExclusiveBetween_LongOfPreviousError_Failure(long from, long to)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_807, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_647, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_767, -32_768, -32_766)]
    [InlineData(1, 0, 2)]
    [InlineData(257, 256, 258)]
    [InlineData(1024, 1000, 5000)]
    [InlineData(32_768, 30_000, 35_000)]
    [InlineData(2_147_483_648, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void ExclusiveBetweenMultiPart_LongOfExclusiveBetweenValue_Success(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_648, -2_147_483_646)]
    [InlineData(-32_768, -32_766)]
    [InlineData(0, 2)]
    [InlineData(256, 258)]
    [InlineData(1000, 5000)]
    [InlineData(30_000, 35_000)]
    [InlineData(2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void ExclusiveBetweenMultiPart_LongOfPreviousError_Failure(long from, long to)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_646, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_769, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(259, 256, 258)]
    [InlineData(500, 1000, 5000)]
    [InlineData(40_000, 30_000, 35_000)]
    [InlineData(2_147_483_650, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void ExclusiveBetween_LongOfEqualOrOutsideValue_Failure(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808, 9_223_372_036_854_775_806)]
    [InlineData(-2_147_483_646, -2_147_483_648, -2_147_483_646)]
    [InlineData(-32_769, -32_768, -32_766)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(259, 256, 258)]
    [InlineData(500, 1000, 5000)]
    [InlineData(40_000, 30_000, 35_000)]
    [InlineData(2_147_483_650, 2_147_483_647, 2_147_483_649)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_805, 9_223_372_036_854_775_807)]
    public void ExclusiveBetweenMultiPart_LongOfEqualOrOutsideValue_Failure(long value, long from, long to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(1, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    public void ExclusiveBetween_FloatOfExclusiveBetweenValue_Success(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.12)]
    [InlineData(-1.3333, -1.3331)]
    [InlineData(0, 2)]
    [InlineData(1.225, 1.227)]
    [InlineData(3.625, 3.627)]
    public void ExclusiveBetween_FloatOfPreviousError_Failure(float from, float to)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(1, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    public void ExclusiveBetweenMultiPart_FloatOfExclusiveBetweenValue_Success(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.12)]
    [InlineData(-1.3333, -1.3331)]
    [InlineData(0, 2)]
    [InlineData(1.225, 1.227)]
    [InlineData(3.625, 3.627)]
    public void ExclusiveBetweenMultiPart_FloatOfPreviousError_Failure(float from, float to)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14, -3.14, -3.12)]
    [InlineData(-1.3331, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    public void ExclusiveBetween_FloatOfEqualOrOutsideValue_Failure(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-3.14, -3.14, -3.12)]
    [InlineData(-1.3331, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    public void ExclusiveBetweenMultiPart_FloatOfEqualOrOutsideValue_Failure(float value, float from, float to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(1, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    public void ExclusiveBetween_DoubleOfExclusiveBetweenValue_Success(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456787)]
    [InlineData(-3.14, -3.12)]
    [InlineData(-1.3333, -1.3331)]
    [InlineData(0, 2)]
    [InlineData(1.225, 1.227)]
    [InlineData(3.625, 3.627)]
    [InlineData(1234.123456789, 1234.123456791)]
    public void ExclusiveBetween_DoubleOfPreviousError_Failure(double from, double to)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(1, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    public void ExclusiveBetweenMultiPart_DoubleOfExclusiveBetweenValue_Success(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456787)]
    [InlineData(-3.14, -3.12)]
    [InlineData(-1.3333, -1.3331)]
    [InlineData(0, 2)]
    [InlineData(1.225, 1.227)]
    [InlineData(3.625, 3.627)]
    [InlineData(1234.123456789, 1234.123456791)]
    public void ExclusiveBetweenMultiPart_DoubleOfPreviousError_Failure(double from, double to)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789, -1234.123456787)]
    [InlineData(-3.12, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.627, 3.625, 3.627)]
    [InlineData(1234.123456788, 1234.123456789, 1234.123456791)]
    public void ExclusiveBetween_DoubleOfEqualOrOutsideValue_Failure(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789, -1234.123456787)]
    [InlineData(-3.12, -3.14, -3.12)]
    [InlineData(-1.3334, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.228, 1.225, 1.227)]
    [InlineData(3.627, 3.625, 3.627)]
    [InlineData(1234.123456788, 1234.123456789, 1234.123456791)]
    public void ExclusiveBetweenMultiPart_DoubleOfEqualOrOutsideValue_Failure(double value, double from, double to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(1, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456790, 123456.123456789, 123456.123456791)]
    public void ExclusiveBetween_DecimalOfExclusiveBetweenValue_Success(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456789, -1234.123456787)]
    [InlineData(-3.14, -3.12)]
    [InlineData(-1.3333, -1.3331)]
    [InlineData(0, 2)]
    [InlineData(1.225, 1.227)]
    [InlineData(3.625, 3.627)]
    [InlineData(1234.123456789, 1234.123456791)]
    [InlineData(123456.123456789, 123456.123456791)]
    public void ExclusiveBetween_DecimalOfPreviousError_Failure(decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-123456.123456788, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456788, -1234.123456789, -1234.123456787)]
    [InlineData(-3.13, -3.14, -3.12)]
    [InlineData(-1.3332, -1.3333, -1.3331)]
    [InlineData(1, 0, 2)]
    [InlineData(1.226, 1.225, 1.227)]
    [InlineData(3.626, 3.625, 3.627)]
    [InlineData(1234.123456790, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456790, 123456.123456789, 123456.123456791)]
    public void ExclusiveBetweenMultiPart_DecimalOfExclusiveBetweenValue_Success(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456789, -1234.123456787)]
    [InlineData(-3.14, -3.12)]
    [InlineData(-1.3333, -1.3331)]
    [InlineData(0, 2)]
    [InlineData(1.225, 1.227)]
    [InlineData(3.625, 3.627)]
    [InlineData(1234.123456789, 1234.123456791)]
    [InlineData(123456.123456789, 123456.123456791)]
    public void ExclusiveBetweenMultiPart_DecimalOfPreviousError_Failure(decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456787, -1234.123456789, -1234.123456787)]
    [InlineData(-3.15, -3.14, -3.12)]
    [InlineData(-1.3330, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.227, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    [InlineData(1234.123456792, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456791, 123456.123456789, 123456.123456791)]
    public void ExclusiveBetween_DecimalOfEqualOrOutsideValue_Failure(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789, -123456.123456787)]
    [InlineData(-1234.123456787, -1234.123456789, -1234.123456787)]
    [InlineData(-3.15, -3.14, -3.12)]
    [InlineData(-1.3330, -1.3333, -1.3331)]
    [InlineData(-1, 0, 2)]
    [InlineData(0, 0, 2)]
    [InlineData(2, 0, 2)]
    [InlineData(3, 0, 2)]
    [InlineData(1.227, 1.225, 1.227)]
    [InlineData(3.624, 3.625, 3.627)]
    [InlineData(1234.123456792, 1234.123456789, 1234.123456791)]
    [InlineData(123456.123456791, 123456.123456789, 123456.123456791)]
    public void ExclusiveBetweenMultiPart_DecimalOfEqualOrOutsideValue_Failure(decimal value, decimal from, decimal to)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456788", "-123456789", "-123456787")]
    [InlineData("-1022", "-1023", "-1021")]
    [InlineData("1", "0", "2")]
    [InlineData("1024", "1000", "2000")]
    [InlineData("123456790", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void ExclusiveBetween_BigIntegerOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456788", "-123456789", "-123456787")]
    [InlineData("-1022", "-1023", "-1021")]
    [InlineData("1", "0", "2")]
    [InlineData("1024", "1000", "2000")]
    [InlineData("123456790", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void ExclusiveBetween_BigIntegerOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456788", "-123456789", "-123456787")]
    [InlineData("-1022", "-1023", "-1021")]
    [InlineData("1", "0", "2")]
    [InlineData("1024", "1000", "2000")]
    [InlineData("123456790", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void ExclusiveBetweenMultiPart_BigIntegerOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567889", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456788", "-123456789", "-123456787")]
    [InlineData("-1022", "-1023", "-1021")]
    [InlineData("1", "0", "2")]
    [InlineData("1024", "1000", "2000")]
    [InlineData("123456790", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567891", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void ExclusiveBetweenMultiPart_BigIntegerOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456787", "-123456789", "-123456787")]
    [InlineData("-1024", "-1023", "-1021")]
    [InlineData("-1", "0", "2")]
    [InlineData("0", "0", "2")]
    [InlineData("2", "0", "2")]
    [InlineData("3", "0", "2")]
    [InlineData("999", "1000", "2000")]
    [InlineData("300000000", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567892", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void ExclusiveBetween_BigIntegerOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567888")]
    [InlineData("-123456787", "-123456789", "-123456787")]
    [InlineData("-1024", "-1023", "-1021")]
    [InlineData("-1", "0", "2")]
    [InlineData("0", "0", "2")]
    [InlineData("2", "0", "2")]
    [InlineData("3", "0", "2")]
    [InlineData("999", "1000", "2000")]
    [InlineData("300000000", "100000000", "200000000")]
    [InlineData("1234567890123456789012345678901234567892", "1234567890123456789012345678901234567890", "1234567890123456789012345678901234567892")]
    public void ExclusiveBetweenMultiPart_BigIntegerOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var from = BigInteger.Parse(fromStr);
        var to = BigInteger.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:02", "00:00:01", "00:00:03")]
    [InlineData("00:00:11", "00:00:10", "00:00:12")]
    [InlineData("00:11:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:02", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:02", "01.00:00:01", "01.00:00:03")]
    public void ExclusiveBetween_TimeSpanOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01", "00:00:03")]
    [InlineData("00:00:11", "00:00:10", "00:00:12")]
    [InlineData("00:11:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:02", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:02", "01.00:00:01", "01.00:00:03")]
    public void ExclusiveBetween_TimeSpanOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01", "00:00:03")]
    [InlineData("00:00:11", "00:00:10", "00:00:12")]
    [InlineData("00:11:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:02", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:02", "01.00:00:01", "01.00:00:03")]
    public void ExclusiveBetweenMultiPart_TimeSpanOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:02", "00:00:01", "00:00:03")]
    [InlineData("00:00:11", "00:00:10", "00:00:12")]
    [InlineData("00:11:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:02", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:02", "01.00:00:01", "01.00:00:03")]
    public void ExclusiveBetweenMultiPart_TimeSpanOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01", "00:00:03")]
    [InlineData("00:00:12", "00:00:10", "00:00:12")]
    [InlineData("00:09:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:04", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:00", "01.00:00:01", "01.00:00:03")]
    public void ExclusiveBetween_TimeSpanOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01", "00:00:03")]
    [InlineData("00:00:12", "00:00:10", "00:00:12")]
    [InlineData("00:09:00", "00:10:00", "00:12:00")]
    [InlineData("10:00:04", "10:00:01", "10:00:03")]
    [InlineData("01.00:00:00", "01.00:00:01", "01.00:00:03")]
    public void ExclusiveBetweenMultiPart_TimeSpanOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var from = TimeSpan.Parse(fromStr);
        var to = TimeSpan.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void ExclusiveBetween_DateTimeOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void ExclusiveBetween_DateTimeOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void ExclusiveBetweenMultiPart_DateTimeOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:11", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:10:01", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:02", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void ExclusiveBetweenMultiPart_DateTimeOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:12", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:09:00", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:04", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void ExclusiveBetween_DateTimeOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01", "2023-05-01 00:00:03")]
    [InlineData("2023-05-01 00:00:12", "2023-05-01 00:00:10", "2023-05-01 00:00:12")]
    [InlineData("2023-05-01 00:09:00", "2023-05-01 00:10:00", "2023-05-01 00:10:02")]
    [InlineData("2023-05-01 10:00:04", "2023-05-01 10:00:01", "2023-05-01 10:00:03")]
    public void ExclusiveBetweenMultiPart_DateTimeOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var from = DateTime.Parse(fromStr);
        var to = DateTime.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void ExclusiveBetween_DateTimeOffsetOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void ExclusiveBetween_DateTimeOffsetOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void ExclusiveBetweenMultiPart_DateTimeOffsetOfExclusiveBetweenValue_Success(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:02 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:11 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:10:01 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:02 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void ExclusiveBetweenMultiPart_DateTimeOffsetOfPreviousError_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:12 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:09:00 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:04 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void ExclusiveBetween_DateTimeOffsetOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .ExclusiveBetween(from, to, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, MyPropertyName, from, to, value));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:03 -4:00")]
    [InlineData("2023-05-01 00:00:12 -4:00", "2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:12 -4:00")]
    [InlineData("2023-05-01 00:09:00 -4:00", "2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:02 -4:00")]
    [InlineData("2023-05-01 10:00:04 -4:00", "2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:03 -4:00")]
    public void ExclusiveBetweenMultiPart_DateTimeOffsetOfEqualOrOutsideValue_Failure(string valueStr, string fromStr, string toStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var from = DateTimeOffset.Parse(fromStr);
        var to = DateTimeOffset.Parse(toStr);

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExclusiveBetween(from, to, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, multiPartPropertyName, from, to, value));
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
