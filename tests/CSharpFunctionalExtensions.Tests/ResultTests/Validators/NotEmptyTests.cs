using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class NotEmptyTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    #region IEnumerable

    [Fact]
    public void NotEmpty_NonEmptyCollection_Success()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmptyMultiPart_NonEmptyCollection_Success()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_EmptyCollection_Failure()
    {
        //Arrange
        var value = new List<int>();

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_EmptyCollection_Failure()
    {
        //Arrange
        var value = new List<int>();

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotEmpty_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotEmptyMultiPart_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEmpty_NonEmptyByte_Success(byte value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEmptyMultiPart_NonEmptyByte_Success(byte value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultByte_Failure()
    {
        //Arrange
        byte value = 0;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultByte_Failure()
    {
        //Arrange
        byte value = 0;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotEmpty_NonEmptyShort_Success(short value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotEmptyMultiPart_NonEmptyShort_Success(short value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultShort_Failure()
    {
        //Arrange
        short value = 0;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultShort_Failure()
    {
        //Arrange
        short value = 0;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    public void NotEmpty_NonEmptyInt_Success(int value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    public void NotEmptyMultiPart_NonEmptyInt_Success(int value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultInt_Failure()
    {
        //Arrange
        int value = 0;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultInt_Failure()
    {
        //Arrange
        int value = 0;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region long

    [Theory]
    [InlineData(-9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807)]
    public void NotEmpty_NonEmptyLong_Success(long value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807)]
    public void NotEmptyMultiPart_NonEmptyLong_Success(long value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultLong_Failure()
    {
        //Arrange
        long value = 0;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultLong_Failure()
    {
        //Arrange
        long value = 0;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotEmpty_NonEmptyFloat_Success(float value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotEmptyMultiPart_NonEmptyFloat_Success(float value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultFloat_Failure()
    {
        //Arrange
        float value = 0.0f;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultFloat_Failure()
    {
        //Arrange
        float value = 0.0f;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    public void NotEmpty_NonEmptyDouble_Success(double value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    public void NotEmptyMultiPart_NonEmptyDouble_Success(double value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultDouble_Failure()
    {
        //Arrange
        double value = 0.0;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultDouble_Failure()
    {
        //Arrange
        double value = 0.0;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456789)]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    [InlineData(123456789.123456789)]
    public void NotEmpty_NonEmptyDecimal_Success(decimal value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789)]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    [InlineData(123456789.123456789)]
    public void NotEmptyMultiPart_NonEmptyDecimal_Success(decimal value)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultDecimal_Failure()
    {
        //Arrange
        decimal value = 0.0m;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultDecimal_Failure()
    {
        //Arrange
        decimal value = 0.0m;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890")]
    [InlineData("-123456789")]
    [InlineData("-1023")]
    [InlineData("-1")]
    [InlineData("1")]
    [InlineData("1023")]
    [InlineData("123456789")]
    [InlineData("1234567890123456789012345678901234567890")]
    public void NotEmpty_NonEmptyBigInteger_Success(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890")]
    [InlineData("-123456789")]
    [InlineData("-1023")]
    [InlineData("-1")]
    [InlineData("1")]
    [InlineData("1023")]
    [InlineData("123456789")]
    [InlineData("1234567890123456789012345678901234567890")]
    public void NotEmptyMultiPart_NonEmptyBigInteger_Success(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultBigInteger_Failure()
    {
        //Arrange
        var value = BigInteger.Zero;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultBigInteger_Failure()
    {
        //Arrange
        var value = BigInteger.Zero;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01")]
    [InlineData("00:00:10")]
    [InlineData("00:10:00")]
    [InlineData("10:00:01")]
    [InlineData("01.00:00:01")]
    public void NotEmpty_NonEmptyTimeSpan_Success(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:01")]
    [InlineData("00:00:10")]
    [InlineData("00:10:00")]
    [InlineData("10:00:01")]
    [InlineData("01.00:00:01")]
    public void NotEmptyMultiPart_NonEmptyTimeSpan_Success(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultTimeSpan_Failure()
    {
        //Arrange
        var value = TimeSpan.Zero;

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultTimeSpan_Failure()
    {
        //Arrange
        var value = TimeSpan.Zero;

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotEmpty_NonEmptyDateTime_Success(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotEmptyMultiPart_NonEmptyDateTime_Success(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultDateTime_Failure()
    {
        //Arrange
        var value = default(DateTime);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultDateTime_Failure()
    {
        //Arrange
        var value = default(DateTime);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotEmpty_NonEmptyDateTimeOffset_Success(string valueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotEmptyMultiPart_NonEmptyDateTimeOffset_Success(string valueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEmpty_DefaultDateTimeOffset_Failure()
    {
        //Arrange
        var value = default(DateTimeOffset);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotEmptyMultiPart_DefaultDateTimeOffset_Failure()
    {
        //Arrange
        var value = default(DateTimeOffset);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion
}
