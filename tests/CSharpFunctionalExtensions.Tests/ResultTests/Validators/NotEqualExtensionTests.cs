using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class NotEqualExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";



    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region T

    [Fact]
    public void NotEqual_ClassOfEqualValue_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .NotEqual(expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    [Fact]
    public void NotEqual_ClassOfPreviousError_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .NotEqual(expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotEqualMultiPart_ClassOfEqualValue_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.NotNull(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, new TestClassEqualityComparer(), PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, ErrorArgument.FromObject(expectedValue)));
    }

    [Fact]
    public void NotEqualMultiPart_ClassOfPreviousError_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.Failure<TestClass>(PreviousFailureError)
            .NotEqual(expectedValue, new TestClassEqualityComparer(), PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void NotEqual_ClassOfDifferingValue_Success()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testStringg");

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .NotEqual(expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void NotEqualMultiPart_ClassOfDifferingValue_Success()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testStringg");

        //Act
        var result = Result.NotNull(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, new TestClassEqualityComparer(), PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString", "testString")]
    [InlineData("123", "123")]
    [InlineData("test", "test")]
    [InlineData("JohnDoe", "JohnDoe")]
    [InlineData("My Awesome Company Name", "My Awesome Company Name")]
    public void NotEqual_StringOfEqualValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, StringComparison.OrdinalIgnoreCase, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotEqual_StringOfPreviousError_Failure(string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .NotEqual(expectedValue, StringComparison.OrdinalIgnoreCase, MyPropertyName);

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
    public void NotEqualMultiPart_StringOfEqualValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, StringComparison.OrdinalIgnoreCase, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("testString")]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("JohnDoe")]
    [InlineData("My Awesome Company Name")]
    public void NotEqualMultiPart_StringOfPreviousError_Failure(string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .NotEqual(expectedValue, StringComparison.OrdinalIgnoreCase, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_StringOfDifferingValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, StringComparison.Ordinal, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("testString", "testStringg")]
    [InlineData("123", "321")]
    [InlineData("test", "mytest")]
    [InlineData("JohnDoe", "JaneDoe")]
    [InlineData("My Awesome Company Name", "My awesome Company Name")]
    public void NotEqualMultiPart_StringOfDifferingValue_Success(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotNullOrEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, StringComparison.Ordinal, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqual_ByteOfEqualValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqual_ByteOfPreviousError_Failure(byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqualMultiPart_ByteOfEqualValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqualMultiPart_ByteOfPreviousError_Failure(byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<byte>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqual_ByteOfDifferingValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqualMultiPart_ByteOfDifferingValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region ushort

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqual_UShortOfEqualValue_Failure(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqual_UShortOfPreviousError_Failure(ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<ushort>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqualMultiPart_UShortOfEqualValue_Failure(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqualMultiPart_UShortOfPreviousError_Failure(ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<ushort>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqual_UShortOfDifferingValue_Success(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqualMultiPart_UShortOfDifferingValue_Success(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region uint

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqual_UIntOfEqualValue_Failure(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqual_UIntOfPreviousError_Failure(uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<uint>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqualMultiPart_UIntOfEqualValue_Failure(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqualMultiPart_UIntOfPreviousError_Failure(uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<uint>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqual_UIntOfDifferingValue_Success(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqualMultiPart_UIntOfDifferingValue_Success(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region ulong

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqual_ULongOfEqualValue_Failure(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqual_ULongOfPreviousError_Failure(ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<ulong>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void NotEqualMultiPart_ULongOfEqualValue_Failure(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(127)]
    [InlineData(255)]
    public void NotEqualMultiPart_ULongOfPreviousError_Failure(ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<ulong>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqual_ULongOfDifferingValue_Success(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void NotEqualMultiPart_ULongOfDifferingValue_Success(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqual_ShortOfEqualValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotEqual_ShortOfPreviousError_Failure(short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_ShortOfEqualValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_768)]
    [InlineData(-1023)]
    [InlineData(-256)]
    [InlineData(256)]
    [InlineData(1023)]
    [InlineData(32_767)]
    public void NotEqualMultiPart_ShortOfPreviousError_Failure(short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<short>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_ShortOfDifferingValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void NotEqualMultiPart_ShortOfDifferingValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqual_IntOfEqualValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
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
    public void NotEqual_IntOfPreviousError_Failure(int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_IntOfEqualValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
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
    public void NotEqualMultiPart_IntOfPreviousError_Failure(int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<int>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_IntOfDifferingValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqualMultiPart_IntOfDifferingValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqual_LongOfEqualValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
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
    public void NotEqual_LongOfPreviousError_Failure(long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_LongOfEqualValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
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
    public void NotEqualMultiPart_LongOfPreviousError_Failure(long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<long>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_LongOfDifferingValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqualMultiPart_LongOfDifferingValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    public void NotEqual_FloatOfEqualValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotEqual_FloatOfPreviousError_Failure(float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    public void NotEqualMultiPart_FloatOfEqualValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    public void NotEqualMultiPart_FloatOfPreviousError_Failure(float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<float>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void NotEqual_FloatOfDifferingValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void NotEqualMultiPart_FloatOfDifferingValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqual_DoubleOfEqualValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    public void NotEqual_DoubleOfPreviousError_Failure(double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_DoubleOfEqualValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456789)]
    [InlineData(-3.14)]
    [InlineData(-1.3333)]
    [InlineData(1.225)]
    [InlineData(3.625)]
    [InlineData(1234.123456789)]
    public void NotEqualMultiPart_DoubleOfPreviousError_Failure(double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<double>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_DoubleOfDifferingValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    public void NotEqualMultiPart_DoubleOfDifferingValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqual_DecimalOfEqualValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
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
    public void NotEqual_DecimalOfPreviousError_Failure(decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_DecimalOfEqualValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
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
    public void NotEqualMultiPart_DecimalOfPreviousError_Failure(decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Failure<decimal>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_DecimalOfDifferingValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqualMultiPart_DecimalOfDifferingValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqual_BigIntegerOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
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
    public void NotEqual_BigIntegerOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_BigIntegerOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
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
    public void NotEqualMultiPart_BigIntegerOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<BigInteger>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_BigIntegerOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
    public void NotEqualMultiPart_BigIntegerOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = BigInteger.Parse(valueStr);
        var expectedValue = BigInteger.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region TimeSpan

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void NotEqual_TimeSpanOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void NotEqual_TimeSpanOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_TimeSpanOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:01", "00:00:01")]
    [InlineData("00:00:10", "00:00:10")]
    [InlineData("00:10:00", "00:10:00")]
    [InlineData("10:00:01", "10:00:01")]
    [InlineData("01.00:00:01", "01.00:00:01")]
    public void NotEqualMultiPart_TimeSpanOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeSpan>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void NotEqual_TimeSpanOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

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
    public void NotEqualMultiPart_TimeSpanOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region Guid

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void NotEqual_GuidOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void NotEqual_GuidOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<Guid>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void NotEqualMultiPart_GuidOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void NotEqualMultiPart_GuidOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<Guid>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000001")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964fe")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "a47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void NotEqual_GuidOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000001")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964fe")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "a47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void NotEqualMultiPart_GuidOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region DateOnly

    [Theory]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("2024-02-29", "2024-02-29")]
    [InlineData("9999-12-31", "9999-12-31")]
    public void NotEqual_DateOnlyOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("2024-02-29", "2024-02-29")]
    [InlineData("9999-12-31", "9999-12-31")]
    public void NotEqual_DateOnlyOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateOnly>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("2024-02-29", "2024-02-29")]
    [InlineData("9999-12-31", "9999-12-31")]
    public void NotEqualMultiPart_DateOnlyOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("2024-02-29", "2024-02-29")]
    [InlineData("9999-12-31", "9999-12-31")]
    public void NotEqualMultiPart_DateOnlyOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateOnly>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-02")]
    [InlineData("2024-02-29", "2024-03-01")]
    [InlineData("9999-12-30", "9999-12-31")]
    public void NotEqual_DateOnlyOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-02")]
    [InlineData("2024-02-29", "2024-03-01")]
    [InlineData("9999-12-30", "9999-12-31")]
    public void NotEqualMultiPart_DateOnlyOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region TimeOnly

    [Theory]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:30:45", "12:30:45")]
    [InlineData("23:59:59", "23:59:59")]
    public void NotEqual_TimeOnlyOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:30:45", "12:30:45")]
    [InlineData("23:59:59", "23:59:59")]
    public void NotEqual_TimeOnlyOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeOnly>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:30:45", "12:30:45")]
    [InlineData("23:59:59", "23:59:59")]
    public void NotEqualMultiPart_TimeOnlyOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:30:45", "12:30:45")]
    [InlineData("23:59:59", "23:59:59")]
    public void NotEqualMultiPart_TimeOnlyOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<TimeOnly>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("00:00:00", "00:00:01")]
    [InlineData("12:30:45", "12:31:45")]
    [InlineData("23:59:58", "23:59:59")]
    public void NotEqual_TimeOnlyOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:00", "00:00:01")]
    [InlineData("12:30:45", "12:31:45")]
    [InlineData("23:59:58", "23:59:59")]
    public void NotEqualMultiPart_TimeOnlyOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Success(value)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region DateTime

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void NotEqual_DateTimeOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void NotEqual_DateTimeOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void NotEqualMultiPart_DateTimeOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:01")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:10")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:00")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:01")]
    public void NotEqualMultiPart_DateTimeOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTime>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:02")]
    public void NotEqual_DateTimeOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01", "2023-05-01 00:00:02")]
    [InlineData("2023-05-01 00:00:10", "2023-05-01 00:00:11")]
    [InlineData("2023-05-01 00:10:00", "2023-05-01 00:10:01")]
    [InlineData("2023-05-01 10:00:01", "2023-05-01 10:00:02")]
    public void NotEqualMultiPart_DateTimeOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTime.Parse(valueStr);
        var expectedValue = DateTime.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    #endregion

    #region DateTimeOffset

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void NotEqual_DateTimeOffsetOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void NotEqual_DateTimeOffsetOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void NotEqualMultiPart_DateTimeOffsetOfEqualValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, multiPartPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:01 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:10 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:00 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:01 -4:00")]
    public void NotEqualMultiPart_DateTimeOffsetOfPreviousError_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.Failure<DateTimeOffset>(PreviousFailureError)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void NotEqual_DateTimeOffsetOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .NotEqual(expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("2023-05-01 00:00:01 -4:00", "2023-05-01 00:00:02 -4:00")]
    [InlineData("2023-05-01 00:00:10 -4:00", "2023-05-01 00:00:11 -4:00")]
    [InlineData("2023-05-01 00:10:00 -4:00", "2023-05-01 00:10:01 -4:00")]
    [InlineData("2023-05-01 10:00:01 -4:00", "2023-05-01 10:00:02 -4:00")]
    public void NotEqualMultiPart_DateTimeOffsetOfDifferingValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateTimeOffset.Parse(valueStr);
        var expectedValue = DateTimeOffset.Parse(expectedValueStr);

        //Act
        var result = Result.NotEmpty(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo)
            .NotEqual(expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
