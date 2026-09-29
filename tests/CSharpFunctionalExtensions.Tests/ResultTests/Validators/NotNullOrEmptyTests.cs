using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class NotNullOrEmptyTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    #region IEnumerable

    [Fact]
    public void NotNullOrEmpty_NonEmptyCollection_Success()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NonEmptyCollection_Success()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_EmptyCollection_Failure()
    {
        //Arrange
        var value = new List<int>();

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_EmptyCollection_Failure()
    {
        //Arrange
        var value = new List<int>();

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullCollection_Failure()
    {
        //Arrange
        List<int> value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullCollection_Failure()
    {
        //Arrange
        List<int> value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotNullOrEmpty_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_EmptyString_Failure()
    {
        //Arrange
        var value = string.Empty;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullString_Failure()
    {
        //Arrange
        string value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullString_Failure()
    {
        //Arrange
        string value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void NotNullOrEmpty_NonEmptyByte_Success(byte value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotNullOrEmptyMultiPart_NonEmptyByte_Success(byte value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultByte_Failure()
    {
        //Arrange
        byte? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultByte_Failure()
    {
        //Arrange
        byte? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullByte_Failure()
    {
        //Arrange
        byte? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullByte_Failure()
    {
        //Arrange
        byte? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotNullOrEmpty_NonEmptyShort_Success(short value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyShort_Success(short value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultShort_Failure()
    {
        //Arrange
        short? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultShort_Failure()
    {
        //Arrange
        short? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullShort_Failure()
    {
        //Arrange
        short? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullShort_Failure()
    {
        //Arrange
        short? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void NotNullOrEmpty_NonEmptyInt_Success(int value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyInt_Success(int value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultInt_Failure()
    {
        //Arrange
        int? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultInt_Failure()
    {
        //Arrange
        int? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullInt_Failure()
    {
        //Arrange
        int? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullInt_Failure()
    {
        //Arrange
        int? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
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
    public void NotNullOrEmpty_NonEmptyLong_Success(long value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyLong_Success(long value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultLong_Failure()
    {
        //Arrange
        long? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultLong_Failure()
    {
        //Arrange
        long? value = 0;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullLong_Failure()
    {
        //Arrange
        long? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullLong_Failure()
    {
        //Arrange
        long? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotNullOrEmpty_NonEmptyFloat_Success(float value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotNullOrEmptyMultiPart_NonEmptyFloat_Success(float value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultFloat_Failure()
    {
        //Arrange
        float? value = 0.0f;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultFloat_Failure()
    {
        //Arrange
        float? value = 0.0f;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullFloat_Failure()
    {
        //Arrange
        float? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullFloat_Failure()
    {
        //Arrange
        float? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
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
    public void NotNullOrEmpty_NonEmptyDouble_Success(double value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyDouble_Success(double value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultDouble_Failure()
    {
        //Arrange
        double? value = 0.0;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultDouble_Failure()
    {
        //Arrange
        double? value = 0.0;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullDouble_Failure()
    {
        //Arrange
        double? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullDouble_Failure()
    {
        //Arrange
        double? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
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
    public void NotNullOrEmpty_NonEmptyDecimal_Success(decimal value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyDecimal_Success(decimal value)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultDecimal_Failure()
    {
        //Arrange
        decimal? value = 0.0m;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultDecimal_Failure()
    {
        //Arrange
        decimal? value = 0.0m;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullDecimal_Failure()
    {
        //Arrange
        decimal? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullDecimal_Failure()
    {
        //Arrange
        decimal? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
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
    public void NotNullOrEmpty_NonEmptyBigInteger_Success(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyBigInteger_Success(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultBigInteger_Failure()
    {
        //Arrange
        BigInteger? value = BigInteger.Zero;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultBigInteger_Failure()
    {
        //Arrange
        BigInteger? value = BigInteger.Zero;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullBigInteger_Failure()
    {
        //Arrange
        BigInteger? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullBigInteger_Failure()
    {
        //Arrange
        BigInteger? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01")]
    [InlineData("00:00:10")]
    [InlineData("00:10:00")]
    [InlineData("10:00:01")]
    [InlineData("01.00:00:01")]
    public void NotNullOrEmpty_NonEmptyTimeSpan_Success(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

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
    public void NotNullOrEmptyMultiPart_NonEmptyTimeSpan_Success(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultTimeSpan_Failure()
    {
        //Arrange
        TimeSpan? value = TimeSpan.Zero;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultTimeSpan_Failure()
    {
        //Arrange
        TimeSpan? value = TimeSpan.Zero;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullTimeSpan_Failure()
    {
        //Arrange
        TimeSpan? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullTimeSpan_Failure()
    {
        //Arrange
        TimeSpan? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region DateOnly

    [Theory]
    [InlineData("2023-05-01")]
    [InlineData("2023-05-02")]
    [InlineData("2023-05-03")]
    [InlineData("2023-05-04")]
    public void NotNullOrEmpty_NonEmptyDateOnly_Success(string valueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01")]
    [InlineData("2023-05-02")]
    [InlineData("2023-05-03")]
    [InlineData("2023-05-04")]
    public void NotNullOrEmptyMultiPart_NonEmptyDateOnly_Success(string valueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultDateOnly_Failure()
    {
        //Arrange
        DateOnly? value = default(DateOnly);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultDateOnly_Failure()
    {
        //Arrange
        DateOnly? value = default(DateOnly);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullDateOnly_Failure()
    {
        //Arrange
        DateOnly? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullDateOnly_Failure()
    {
        //Arrange
        DateOnly? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotNullOrEmpty_NonEmptyDateTime_Success(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotNullOrEmptyMultiPart_NonEmptyDateTime_Success(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultDateTime_Failure()
    {
        //Arrange
        DateTime? value = default(DateTime);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultDateTime_Failure()
    {
        //Arrange
        DateTime? value = default(DateTime);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullDateTime_Failure()
    {
        //Arrange
        DateTime? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullDateTime_Failure()
    {
        //Arrange
        DateTime? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotNullOrEmpty_NonEmptyDateTimeOffset_Success(string valueStr)
    {
        //Arrange
        DateTimeOffset? value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotNullOrEmptyMultiPart_NonEmptyDateTimeOffset_Success(string valueStr)
    {
        //Arrange
        DateTimeOffset? value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullOrEmpty_DefaultDateTimeOffset_Failure()
    {
        //Arrange
        DateTimeOffset? value = default(DateTimeOffset);

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_DefaultDateTimeOffset_Failure()
    {
        //Arrange
        DateTimeOffset? value = default(DateTimeOffset);

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    [Fact]
    public void NotNullOrEmpty_NullDateTimeOffset_Failure()
    {
        //Arrange
        DateTimeOffset? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Fact]
    public void NotNullOrEmptyMultiPart_NullDateTimeOffset_Failure()
    {
        //Arrange
        DateTimeOffset? value = null;

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotEmpty, multiPartPropertyName));
    }

    #endregion
}
