#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class NotNullExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region IEnumerable

    [Fact]
    public void NotNull_NonEmptyCollection_Success()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_CollectionPreviousError_Failure()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNullMultiPart_NonEmptyCollection_Success()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_CollectionPreviousError_Failure()
    {
        //Arrange
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_EmptyCollection_Success()
    {
        //Arrange
        var value = new List<int>();

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_EmptyCollection_Success()
    {
        //Arrange
        var value = new List<int>();

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullCollection_Failure()
    {
        //Arrange
        List<int>? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullCollection_Failure()
    {
        //Arrange
        List<int>? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotNull_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_StringPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotNullMultiPart_NonEmptyString_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_StringPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_EmptyString_Success()
    {
        //Arrange
        string? value = string.Empty;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_EmptyString_Success()
    {
        //Arrange
        string? value = string.Empty;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullString_Failure()
    {
        //Arrange
        string? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullString_Failure()
    {
        //Arrange
        string? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotNull_NonEmptyByte_Success(byte value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_BytePreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotNullMultiPart_NonEmptyByte_Success(byte value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_BytePreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultByte_Success()
    {
        //Arrange
        byte? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultByte_Success()
    {
        //Arrange
        byte? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullByte_Failure()
    {
        //Arrange
        byte? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullByte_Failure()
    {
        //Arrange
        byte? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotNull_NonEmptyShort_Success(short value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_ShortPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotNullMultiPart_NonEmptyShort_Success(short value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_ShortPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultShort_Success()
    {
        //Arrange
        short? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultShort_Success()
    {
        //Arrange
        short? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullShort_Failure()
    {
        //Arrange
        short? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullShort_Failure()
    {
        //Arrange
        short? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
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
    public void NotNull_NonEmptyInt_Success(int value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_IntPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    public void NotNullMultiPart_NonEmptyInt_Success(int value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_IntPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultInt_Success()
    {
        //Arrange
        int? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultInt_Success()
    {
        //Arrange
        int? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullInt_Failure()
    {
        //Arrange
        int? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullInt_Failure()
    {
        //Arrange
        int? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
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
    public void NotNull_NonEmptyLong_Success(long value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_LongPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
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
    public void NotNullMultiPart_NonEmptyLong_Success(long value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_LongPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultLong_Success()
    {
        //Arrange
        long? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultLong_Success()
    {
        //Arrange
        long? value = 0;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullLong_Failure()
    {
        //Arrange
        long? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullLong_Failure()
    {
        //Arrange
        long? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotNull_NonEmptyFloat_Success(float value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_FloatPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotNullMultiPart_NonEmptyFloat_Success(float value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_FloatPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultFloat_Success()
    {
        //Arrange
        float? value = 0.0f;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultFloat_Success()
    {
        //Arrange
        float? value = 0.0f;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullFloat_Failure()
    {
        //Arrange
        float? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullFloat_Failure()
    {
        //Arrange
        float? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
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
    public void NotNull_NonEmptyDouble_Success(double value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_DoublePreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    public void NotNullMultiPart_NonEmptyDouble_Success(double value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DoublePreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultDouble_Success()
    {
        //Arrange
        double? value = 0.0;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultDouble_Success()
    {
        //Arrange
        double? value = 0.0;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullDouble_Failure()
    {
        //Arrange
        double? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullDouble_Failure()
    {
        //Arrange
        double? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
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
    public void NotNull_NonEmptyDecimal_Success(decimal value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_DecimalPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
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
    public void NotNullMultiPart_NonEmptyDecimal_Success(decimal value)
    {
        //Arrange

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DecimalPreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultDecimal_Success()
    {
        //Arrange
        decimal? value = 0.0m;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultDecimal_Success()
    {
        //Arrange
        decimal? value = 0.0m;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullDecimal_Failure()
    {
        //Arrange
        decimal? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullDecimal_Failure()
    {
        //Arrange
        decimal? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
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
    public void NotNull_NonEmptyBigInteger_Success(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

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
    public void NotNull_BigIntegerPreviousError_Failure(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
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
    public void NotNullMultiPart_NonEmptyBigInteger_Success(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void NotNullMultiPart_BigIntegerPreviousError_Failure(string valueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultBigInteger_Success()
    {
        //Arrange
        BigInteger? value = BigInteger.Zero;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultBigInteger_Success()
    {
        //Arrange
        BigInteger? value = BigInteger.Zero;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullBigInteger_Failure()
    {
        //Arrange
        BigInteger? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullBigInteger_Failure()
    {
        //Arrange
        BigInteger? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01")]
    [InlineData("00:00:10")]
    [InlineData("00:10:00")]
    [InlineData("10:00:01")]
    [InlineData("01.00:00:01")]
    public void NotNull_NonEmptyTimeSpan_Success(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

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
    public void NotNull_TimeSpanPreviousError_Failure(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:01")]
    [InlineData("00:00:10")]
    [InlineData("00:10:00")]
    [InlineData("10:00:01")]
    [InlineData("01.00:00:01")]
    public void NotNullMultiPart_NonEmptyTimeSpan_Success(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void NotNullMultiPart_TimeSpanPreviousError_Failure(string valueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultTimeSpan_Success()
    {
        //Arrange
        TimeSpan? value = TimeSpan.Zero;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultTimeSpan_Success()
    {
        //Arrange
        TimeSpan? value = TimeSpan.Zero;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullTimeSpan_Failure()
    {
        //Arrange
        TimeSpan? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullTimeSpan_Failure()
    {
        //Arrange
        TimeSpan? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotNull_NonEmptyDateTime_Success(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotNull_DateTimePreviousError_Failure(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotNullMultiPart_NonEmptyDateTime_Success(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01")]
    public void NotNullMultiPart_DateTimePreviousError_Failure(string valueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultDateTime_Success()
    {
        //Arrange
        DateTime? value = default(DateTime);

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultDateTime_Success()
    {
        //Arrange
        DateTime? value = default(DateTime);

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullDateTime_Failure()
    {
        //Arrange
        DateTime? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullDateTime_Failure()
    {
        //Arrange
        DateTime? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotNull_NonEmptyDateTimeOffset_Success(string valueStr)
    {
        //Arrange
        DateTimeOffset? value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotNull_DateTimeOffsetPreviousError_Failure(string valueStr)
    {
        //Arrange
        DateTimeOffset? value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotNullMultiPart_NonEmptyDateTimeOffset_Success(string valueStr)
    {
        //Arrange
        DateTimeOffset? value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00")]
    public void NotNullMultiPart_DateTimeOffsetPreviousError_Failure(string valueStr)
    {
        //Arrange
        DateTimeOffset? value = DateTimeOffset.Parse(valueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotNull_DefaultDateTimeOffset_Success()
    {
        //Arrange
        DateTimeOffset? value = default(DateTimeOffset);

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNullMultiPart_DefaultDateTimeOffset_Success()
    {
        //Arrange
        DateTimeOffset? value = default(DateTimeOffset);

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotNull_NullDateTimeOffset_Failure()
    {
        //Arrange
        DateTimeOffset? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, MyPropertyName));
    }

    [Fact]
    public void NotNullMultiPart_NullDateTimeOffset_Failure()
    {
        //Arrange
        DateTimeOffset? value = null;

        //Act
        var result = Result.Success(value)
            .NotNull(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull, multiPartPropertyName));
    }

    #endregion
}
