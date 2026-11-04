using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class EqualTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";



    #region T

    [Fact]
    public void Equal_ClassOfEqualValue_Success()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.Equal(value, expectedValue, new TestClassEqualityComparer(), MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void EqualMultiPart_ClassOfEqualValue_Success()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testString");

        //Act
        var result = Result.Equal(value, expectedValue, new TestClassEqualityComparer(), PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void Equal_ClassOfDifferingValue_Failure()
    {
        //Arrange
        var guidId = Guid.NewGuid();
        var value = new TestClass(guidId, "testString");
        var expectedValue = new TestClass(guidId, "testStringg");

        //Act
        var result = Result.Equal(value, expectedValue, new TestClassEqualityComparer(), MyPropertyName);

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
        var result = Result.Equal(value, expectedValue, new TestClassEqualityComparer(), PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
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
        var result = Result.Equal(value, expectedValue, StringComparison.OrdinalIgnoreCase, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.Equal(value, expectedValue, StringComparison.OrdinalIgnoreCase, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void Equal_StringOfDifferingValue_Failure(string value, string expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, StringComparison.Ordinal, MyPropertyName);

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
        var result = Result.Equal(value, expectedValue, StringComparison.Ordinal, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region byte

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void Equal_ByteOfEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void EqualMultiPart_ByteOfEqualValue_Success(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void Equal_ByteOfDifferingValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void EqualMultiPart_ByteOfDifferingValue_Failure(byte value, byte expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region ushort

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void Equal_UShortOfEqualValue_Success(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void EqualMultiPart_UShortOfEqualValue_Success(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void Equal_UShortOfDifferingValue_Failure(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void EqualMultiPart_UShortOfDifferingValue_Failure(ushort value, ushort expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region uint

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void Equal_UIntOfEqualValue_Success(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void EqualMultiPart_UIntOfEqualValue_Success(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void Equal_UIntOfDifferingValue_Failure(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void EqualMultiPart_UIntOfDifferingValue_Failure(uint value, uint expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region ulong

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void Equal_ULongOfEqualValue_Success(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(127, 127)]
    [InlineData(255, 255)]
    public void EqualMultiPart_ULongOfEqualValue_Success(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void Equal_ULongOfDifferingValue_Failure(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(12, 13)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    public void EqualMultiPart_ULongOfDifferingValue_Failure(ulong value, ulong expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region short

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(0, 0)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    public void Equal_ShortOfEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(0, 0)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    public void EqualMultiPart_ShortOfEqualValue_Success(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void Equal_ShortOfDifferingValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_766, 32_767)]
    public void EqualMultiPart_ShortOfDifferingValue_Failure(short value, short expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region int

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(0, 0)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    public void Equal_IntOfEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(0, 0)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    public void EqualMultiPart_IntOfEqualValue_Success(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void Equal_IntOfDifferingValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-2_147_483_648, -2_147_483_647)]
    [InlineData(-32_768, -32_767)]
    [InlineData(-1023, -1024)]
    [InlineData(-256, -257)]
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_646, 2_147_483_647)]
    public void EqualMultiPart_IntOfDifferingValue_Failure(int value, int expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
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
    [InlineData(0, 0)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_807)]
    public void Equal_LongOfEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-9_223_372_036_854_775_808, -9_223_372_036_854_775_808)]
    [InlineData(-2_147_483_648, -2_147_483_648)]
    [InlineData(-32_768, -32_768)]
    [InlineData(-1023, -1023)]
    [InlineData(-256, -256)]
    [InlineData(0, 0)]
    [InlineData(256, 256)]
    [InlineData(1023, 1023)]
    [InlineData(32_767, 32_767)]
    [InlineData(2_147_483_647, 2_147_483_647)]
    [InlineData(9_223_372_036_854_775_807, 9_223_372_036_854_775_807)]
    public void EqualMultiPart_LongOfEqualValue_Success(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void Equal_LongOfDifferingValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

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
    [InlineData(0, 1)]
    [InlineData(256, 257)]
    [InlineData(1023, 1024)]
    [InlineData(32_767, 32_768)]
    [InlineData(2_147_483_647, 2_147_483_648)]
    [InlineData(9_223_372_036_854_775_806, 9_223_372_036_854_775_807)]
    public void EqualMultiPart_LongOfDifferingValue_Failure(long value, long expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region float

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    public void Equal_FloatOfEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    public void EqualMultiPart_FloatOfEqualValue_Success(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void Equal_FloatOfDifferingValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    public void EqualMultiPart_FloatOfDifferingValue_Failure(float value, float expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region double

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    public void Equal_DoubleOfEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    public void EqualMultiPart_DoubleOfEqualValue_Success(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    public void Equal_DoubleOfDifferingValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    public void EqualMultiPart_DoubleOfDifferingValue_Failure(double value, double expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region decimal

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    [InlineData(123456789.123456789, 123456789.123456789)]
    public void Equal_DecimalOfEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456789)]
    [InlineData(-1234.123456789, -1234.123456789)]
    [InlineData(-3.14, -3.14)]
    [InlineData(-1.3333, -1.3333)]
    [InlineData(0, 0)]
    [InlineData(1.225, 1.225)]
    [InlineData(3.625, 3.625)]
    [InlineData(1234.123456789, 1234.123456789)]
    [InlineData(123456789.123456789, 123456789.123456789)]
    public void EqualMultiPart_DecimalOfEqualValue_Success(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456780)]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    [InlineData(123456.123456789, 123456.123456780)]
    public void Equal_DecimalOfDifferingValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData(-123456.123456789, -123456.123456780)]
    [InlineData(-1234.123456789, -1234.123456780)]
    [InlineData(-3.14, -3.15)]
    [InlineData(-1.3333, -1.3334)]
    [InlineData(0, 1)]
    [InlineData(1.225, 1.226)]
    [InlineData(3.625, 3.626)]
    [InlineData(1234.123456789, 1234.123456780)]
    [InlineData(123456.123456789, 123456.123456780)]
    public void EqualMultiPart_DecimalOfDifferingValue_Failure(decimal value, decimal expectedValue)
    {
        //Arrange

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region BigInteger

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1023", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("0", "0")]
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567890")]
    [InlineData("-123456789", "-123456789")]
    [InlineData("-1023", "-1023")]
    [InlineData("-1", "-1")]
    [InlineData("0", "0")]
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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567891")]
    [InlineData("-123456789", "-123456780")]
    [InlineData("-1023", "-1024")]
    [InlineData("-1", "-2")]
    [InlineData("0", "1")]
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("-1234567890123456789012345678901234567890", "-1234567890123456789012345678901234567891")]
    [InlineData("-123456789", "-123456780")]
    [InlineData("-1023", "-1024")]
    [InlineData("-1", "-2")]
    [InlineData("0", "1")]
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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

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
    public void EqualMultiPart_TimeSpanOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

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
    public void Equal_TimeSpanOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeSpan.Parse(valueStr);
        var expectedValue = TimeSpan.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region Guid

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void Equal_GuidOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964ff")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "f47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void EqualMultiPart_GuidOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000001")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964fe")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "a47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void Equal_GuidOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000001")]
    [InlineData("6f9619ff-8b86-d011-b42d-00c04fc964ff", "6f9619ff-8b86-d011-b42d-00c04fc964fe")]
    [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479", "a47ac10b-58cc-4372-a567-0e02b2c3d479")]
    public void EqualMultiPart_GuidOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = Guid.Parse(valueStr);
        var expectedValue = Guid.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region DateOnly

    [Theory]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("2024-02-29", "2024-02-29")]
    [InlineData("9999-12-31", "9999-12-31")]
    public void Equal_DateOnlyOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-01")]
    [InlineData("2024-02-29", "2024-02-29")]
    [InlineData("9999-12-31", "9999-12-31")]
    public void EqualMultiPart_DateOnlyOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-02")]
    [InlineData("2024-02-29", "2024-03-01")]
    [InlineData("9999-12-30", "9999-12-31")]
    public void Equal_DateOnlyOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-02")]
    [InlineData("2024-02-29", "2024-03-01")]
    [InlineData("9999-12-30", "9999-12-31")]
    public void EqualMultiPart_DateOnlyOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = DateOnly.Parse(valueStr);
        var expectedValue = DateOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, multiPartPropertyName, expectedValue));
    }

    #endregion

    #region TimeOnly

    [Theory]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:30:45", "12:30:45")]
    [InlineData("23:59:59", "23:59:59")]
    public void Equal_TimeOnlyOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:00", "00:00:00")]
    [InlineData("12:30:45", "12:30:45")]
    [InlineData("23:59:59", "23:59:59")]
    public void EqualMultiPart_TimeOnlyOfEqualValue_Success(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData("00:00:00", "00:00:01")]
    [InlineData("12:30:45", "12:31:45")]
    [InlineData("23:59:58", "23:59:59")]
    public void Equal_TimeOnlyOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, MyPropertyName, expectedValue));
    }

    [Theory]
    [InlineData("00:00:00", "00:00:01")]
    [InlineData("12:30:45", "12:31:45")]
    [InlineData("23:59:58", "23:59:59")]
    public void EqualMultiPart_TimeOnlyOfDifferingValue_Failure(string valueStr, string expectedValueStr)
    {
        //Arrange
        var value = TimeOnly.Parse(valueStr);
        var expectedValue = TimeOnly.Parse(expectedValueStr);

        //Act
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.Equal(value, expectedValue, MyPropertyName);

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
        var result = Result.Equal(value, expectedValue, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = PropertyPath.Root.Append(MyPropertyName).Format(MyPropertyNameTwo);
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
