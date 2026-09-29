using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class EqualExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region T

    [Fact]
    public void Equal_ClassOfEqualValue_Success()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .Equal(expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void Equal_ClassOfPreviousError_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .Equal(expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void EqualMultiPart_ClassOfEqualValue_Success()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, new TestClassEqualityComparer(), MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void EqualMultiPart_ClassOfPreviousError_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .Equal(expectedValue, new TestClassEqualityComparer(), MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void Equal_ClassOfDifferingValue_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testStringg");

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .Equal(expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    [Fact]
    public void EqualMultiPart_ClassOfDifferingValue_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testStringg");

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, new TestClassEqualityComparer(), MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString", "testString")]
    [InlineData("123", "123")]
    [InlineData("test", "test")]
    [InlineData("JohnDoe", "JohnDoe")]
    [InlineData("My Awesome Company Name", "My Awesome Company Name")]
    public void Equal_StringOfEqualValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .Equal(expectedValue, StringComparison.OrdinalIgnoreCase, MyPropertyName);

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
    public void Equal_StringOfPreviousError_Failure(string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .Equal(expectedValue, StringComparison.OrdinalIgnoreCase, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", "testString")]
    [InlineData("123", "123")]
    [InlineData("test", "test")]
    [InlineData("JohnDoe", "JohnDoe")]
    [InlineData("My Awesome Company Name", "My Awesome Company Name")]
    public void EqualMultiPart_StringOfEqualValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, StringComparison.OrdinalIgnoreCase, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void EqualMultiPart_StringOfPreviousError_Failure(string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .Equal(expectedValue, StringComparison.OrdinalIgnoreCase, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", "testStringg")]
    [InlineData("123", "321")]
    [InlineData("test", "mytest")]
    [InlineData("JohnDoe", "JaneDoe")]
    [InlineData("My Awesome Company Name", "My awesome Company Name")]
    public void Equal_StringOfDifferingValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .Equal(expectedValue, StringComparison.Ordinal, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("testString", "testStringg")]
    [InlineData("123", "321")]
    [InlineData("test", "mytest")]
    [InlineData("JohnDoe", "JaneDoe")]
    [InlineData("My Awesome Company Name", "My awesome Company Name")]
    public void EqualMultiPart_StringOfDifferingValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, StringComparison.Ordinal, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void Equal_ByteOfEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void Equal_ByteOfPreviousError_Failure(byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void EqualMultiPart_ByteOfEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void EqualMultiPart_ByteOfPreviousError_Failure(byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void Equal_ByteOfDifferingValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void EqualMultiPart_ByteOfDifferingValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    public void Equal_ShortOfEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void Equal_ShortOfPreviousError_Failure(short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    public void EqualMultiPart_ShortOfEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void EqualMultiPart_ShortOfPreviousError_Failure(short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void Equal_ShortOfDifferingValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void EqualMultiPart_ShortOfDifferingValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    public void Equal_IntOfEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    public void Equal_IntOfPreviousError_Failure(int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    public void EqualMultiPart_IntOfEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    public void EqualMultiPart_IntOfPreviousError_Failure(int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void Equal_IntOfDifferingValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void EqualMultiPart_IntOfDifferingValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region long

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_807)]
    public void Equal_LongOfEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807)]
    public void Equal_LongOfPreviousError_Failure(long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_807)]
    public void EqualMultiPart_LongOfEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648)]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    [InlineData(2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807)]
    public void EqualMultiPart_LongOfPreviousError_Failure(long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void Equal_LongOfDifferingValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_807)]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void EqualMultiPart_LongOfDifferingValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    public void Equal_FloatOfEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void Equal_FloatOfPreviousError_Failure(float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    public void EqualMultiPart_FloatOfEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void EqualMultiPart_FloatOfPreviousError_Failure(float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void Equal_FloatOfDifferingValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void EqualMultiPart_FloatOfDifferingValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    public void Equal_DoubleOfEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

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
    public void Equal_DoubleOfPreviousError_Failure(double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    public void EqualMultiPart_DoubleOfEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void EqualMultiPart_DoubleOfPreviousError_Failure(double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    public void Equal_DoubleOfDifferingValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    public void EqualMultiPart_DoubleOfDifferingValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    [InlineData(123456789.123456789, 123456789.123456789)]
    public void Equal_DecimalOfEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

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
    public void Equal_DecimalOfPreviousError_Failure(decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    [InlineData(123456789.123456789, 123456789.123456789)]
    public void EqualMultiPart_DecimalOfEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void EqualMultiPart_DecimalOfPreviousError_Failure(decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456780)]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    [InlineData(123456.123456789, 123456.123456780)]
    public void Equal_DecimalOfDifferingValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456780)]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    [InlineData(123456.123456789, 123456.123456780)]
    public void EqualMultiPart_DecimalOfDifferingValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1023", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("1", "1")]
    [InlineData("1023", "1023")]
    [InlineData("123456789", "123456789")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890")]
    public void Equal_BigIntegerOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1023", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("1", "1")]
    [InlineData("1023", "1023")]
    [InlineData("123456789", "123456789")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890")]
    public void Equal_BigIntegerOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1023", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("1", "1")]
    [InlineData("1023", "1023")]
    [InlineData("123456789", "123456789")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890")]
    public void EqualMultiPart_BigIntegerOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1023", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("1", "1")]
    [InlineData("1023", "1023")]
    [InlineData("123456789", "123456789")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567890")]
    public void EqualMultiPart_BigIntegerOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567891")]
    [InlineData("-123456789", "-123456780")]
    [InlineData("-1023", "-1024")]
    [InlineData("-1", "-2")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456780")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void Equal_BigIntegerOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567891")]
    [InlineData("-123456789", "-123456780")]
    [InlineData("-1023", "-1024")]
    [InlineData("-1", "-2")]
    [InlineData("1", "2")]
    [InlineData("1023", "1024")]
    [InlineData("123456789", "123456780")]
    [InlineData("1234567890123456789012345678901234567890", "1234567890123456789012345678901234567891")]
    public void EqualMultiPart_BigIntegerOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void Equal_TimeSpanOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void Equal_TimeSpanOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void EqualMultiPart_TimeSpanOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void EqualMultiPart_TimeSpanOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
    public void Equal_TimeSpanOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:01", "00:00:02")]
    [InlineData("00:00:10", "00:00:11")]
    [InlineData("00:10:00", "00:11:00")]
    [InlineData("10:00:01", "10:00:02")]
    [InlineData("01.00:00:01", "01.00:00:02")]
    public void EqualMultiPart_TimeSpanOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void Equal_DateTimeOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void Equal_DateTimeOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void EqualMultiPart_DateTimeOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void EqualMultiPart_DateTimeOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:02")]
    public void Equal_DateTimeOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:02")]
    public void EqualMultiPart_DateTimeOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void Equal_DateTimeOffsetOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void Equal_DateTimeOffsetOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void EqualMultiPart_DateTimeOffsetOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void EqualMultiPart_DateTimeOffsetOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void Equal_DateTimeOffsetOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .Equal(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void EqualMultiPart_DateTimeOffsetOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .Equal(expectedValue, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion


    public class TestClass : IEquatable<TestClass>
    {
        public TestClass(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }

        #region IEquatable

        public override bool Equals(object obj) => obj is TestClass testClass && Equals(testClass);
        public bool Equals(TestClass other) => other is not null && EqualsCore(other);
        bool EqualsCore(TestClass other) =>
            Id == other.Id &&
            Name.Equals(other.Name, StringComparison.Ordinal);

        public static bool operator ==(TestClass left, TestClass right) =>
            !(left is null ^ right is null) && (left is null || left.EqualsCore(right!));
        public static bool operator !=(TestClass left, TestClass right) => !(left == right);

        public override int GetHashCode() => HashCode.Combine(Id, Name);

        #endregion
    }

    public class TestClassEqualityComparer : IEqualityComparer<TestClass>
    {
        public bool Equals(TestClass x, TestClass y) => x == y;
        public int GetHashCode([DisallowNull] TestClass obj) => obj.GetHashCode();
    }
}
