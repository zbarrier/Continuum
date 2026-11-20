namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class ParseValidatorTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";
    const string MultiPartPropertyName = MyPropertyName + "." + MyPropertyNameTwo;

    static ValidationError InvalidFormat(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, propertyName);

    static ValidationError NotEmpty(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, propertyName);

    static ValidationError Email(string propertyName) =>
        new(propertyName, ExpectedValidationErrorCodes.Email, ExpectedValidatorErrorStrings.Email, propertyName);

    static readonly ValidationError OtherError = new("other", "other_code", "'{0}' other.", "other");

    // IsDateTime

    [Fact]
    public void IsDateTime_ValidValue_Success()
    {
        var result = Result.IsDateTime("2024-01-15T10:30:00", MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0), result.Value);
    }

    [Fact]
    public void IsDateTime_InvalidValue_InvalidFormatError()
    {
        var result = Result.IsDateTime("not a date", MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsDateTime_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsDateTime(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsDateTimeMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsDateTime("not a date", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsDateTimeMultiPart_ValidValue_Success()
    {
        var result = Result.IsDateTime("2024-01-15", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(new DateTime(2024, 1, 15), result.Value);
    }

    [Fact]
    public void IsDateTimeChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateTime(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateTime(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsInt32

    [Theory]
    [InlineData("0", 0)]
    [InlineData("-42", -42)]
    [InlineData("2147483647", int.MaxValue)]
    public void IsInt32_ValidValue_Success(string value, int expected)
    {
        var result = Result.IsInt32(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    public void IsInt32_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsInt32(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsInt32_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsInt32(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsInt32MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsInt32("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsInt32MultiPart_ValidValue_Success()
    {
        var result = Result.IsInt32("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void IsInt32Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt32(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt32(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsInt64

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("-42", -42L)]
    [InlineData("2147483648", 2147483648L)]
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    public void IsInt64_ValidValue_Success(string value, long expected)
    {
        var result = Result.IsInt64(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("9223372036854775808")]
    public void IsInt64_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsInt64(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsInt64_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsInt64(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsInt64MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsInt64("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsInt64MultiPart_ValidValue_Success()
    {
        var result = Result.IsInt64("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(7L, result.Value);
    }

    [Fact]
    public void IsInt64Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt64(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt64(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsInt16

    [Theory]
    [InlineData("0", (short)0)]
    [InlineData("-32768", short.MinValue)]
    [InlineData("32767", short.MaxValue)]
    public void IsInt16_ValidValue_Success(string value, short expected)
    {
        var result = Result.IsInt16(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("32768")]
    [InlineData("-32769")]
    public void IsInt16_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsInt16(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsInt16_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsInt16(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsInt16MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsInt16("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsInt16MultiPart_ValidValue_Success()
    {
        var result = Result.IsInt16("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal((short)7, result.Value);
    }

    [Fact]
    public void IsInt16Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt16(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsInt16(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsUInt16

    [Theory]
    [InlineData("0", (ushort)0)]
    [InlineData("65535", ushort.MaxValue)]
    public void IsUInt16_ValidValue_Success(string value, ushort expected)
    {
        var result = Result.IsUInt16(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("-1")]
    [InlineData("65536")]
    public void IsUInt16_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsUInt16(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsUInt16_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsUInt16(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsUInt16MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsUInt16("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsUInt16MultiPart_ValidValue_Success()
    {
        var result = Result.IsUInt16("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal((ushort)7, result.Value);
    }

    [Fact]
    public void IsUInt16Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsUInt16(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsUInt16(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsUInt32

    [Theory]
    [InlineData("0", 0u)]
    [InlineData("2147483648", 2147483648u)]
    [InlineData("4294967295", uint.MaxValue)]
    public void IsUInt32_ValidValue_Success(string value, uint expected)
    {
        var result = Result.IsUInt32(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("-1")]
    [InlineData("4294967296")]
    public void IsUInt32_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsUInt32(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsUInt32_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsUInt32(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsUInt32MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsUInt32("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsUInt32MultiPart_ValidValue_Success()
    {
        var result = Result.IsUInt32("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(7u, result.Value);
    }

    [Fact]
    public void IsUInt32Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsUInt32(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsUInt32(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsUInt64

    [Theory]
    [InlineData("0", 0ul)]
    [InlineData("9223372036854775808", 9223372036854775808ul)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    public void IsUInt64_ValidValue_Success(string value, ulong expected)
    {
        var result = Result.IsUInt64(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("-1")]
    [InlineData("18446744073709551616")]
    public void IsUInt64_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsUInt64(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsUInt64_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsUInt64(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsUInt64MultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsUInt64("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsUInt64MultiPart_ValidValue_Success()
    {
        var result = Result.IsUInt64("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(7ul, result.Value);
    }

    [Fact]
    public void IsUInt64Chained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsUInt64(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsUInt64(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsByte

    [Theory]
    [InlineData("0", (byte)0)]
    [InlineData("128", (byte)128)]
    [InlineData("255", byte.MaxValue)]
    public void IsByte_ValidValue_Success(string value, byte expected)
    {
        var result = Result.IsByte(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("-1")]
    [InlineData("256")]
    public void IsByte_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsByte(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsByte_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsByte(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsByteMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsByte("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsByteMultiPart_ValidValue_Success()
    {
        var result = Result.IsByte("7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal((byte)7, result.Value);
    }

    [Fact]
    public void IsByteChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsByte(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsByte(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsSByte

    [Theory]
    [InlineData("-128", sbyte.MinValue)]
    [InlineData("0", (sbyte)0)]
    [InlineData("127", sbyte.MaxValue)]
    public void IsSByte_ValidValue_Success(string value, sbyte expected)
    {
        var result = Result.IsSByte(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("-129")]
    [InlineData("128")]
    public void IsSByte_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsSByte(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsSByte_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsSByte(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsSByteMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsSByte("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsSByteMultiPart_ValidValue_Success()
    {
        var result = Result.IsSByte("-7", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal((sbyte)-7, result.Value);
    }

    [Fact]
    public void IsSByteChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsSByte(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsSByte(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsSingle

    [Theory]
    [InlineData("0", 0f)]
    [InlineData("1.5", 1.5f)]
    [InlineData("-2.25", -2.25f)]
    [InlineData("3.4028235E+38", float.MaxValue)]
    public void IsSingle_ValidValue_Success(string value, float expected)
    {
        var result = Result.IsSingle(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("1e39")]
    public void IsSingle_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsSingle(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsSingle_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsSingle(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsSingleMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsSingle("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsSingleMultiPart_ValidValue_Success()
    {
        var result = Result.IsSingle("7.5", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(7.5f, result.Value);
    }

    [Fact]
    public void IsSingleChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsSingle(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsSingle(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsDouble

    [Theory]
    [InlineData("0", 0d)]
    [InlineData("1.5", 1.5d)]
    [InlineData("-2.25", -2.25d)]
    [InlineData("1e39", 1e39d)]
    public void IsDouble_ValidValue_Success(string value, double expected)
    {
        var result = Result.IsDouble(value, MyPropertyName);

        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("1e400")]
    public void IsDouble_InvalidValue_InvalidFormatError(string value)
    {
        var result = Result.IsDouble(value, MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsDouble_NullOrEmpty_NotEmptyError(string? value)
    {
        var result = Result.IsDouble(value, MyPropertyName);

        Assert.Equal(NotEmpty(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsDoubleMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.IsDouble("abc", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsDoubleMultiPart_ValidValue_Success()
    {
        var result = Result.IsDouble("7.5", PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(7.5d, result.Value);
    }

    [Fact]
    public void IsDoubleChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDouble(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDouble(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsDateOnly (chained)

    [Fact]
    public void IsDateOnlyChained_ValidValue_Success()
    {
        var result = Result.Success("2024-01-15").IsDateOnly(MyPropertyName);

        Assert.Equal(new DateOnly(2024, 1, 15), result.Value);
    }

    [Fact]
    public void IsDateOnlyChained_InvalidValue_InvalidFormatError()
    {
        var result = Result.Success("not a date").IsDateOnly(MyPropertyName);

        Assert.Equal(InvalidFormat(MyPropertyName), result.Error);
    }

    [Fact]
    public void IsDateOnlyChainedMultiPart_InvalidValue_UsesFormattedPropertyName()
    {
        var result = Result.Success("not a date").IsDateOnly(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo);

        Assert.Equal(InvalidFormat(MultiPartPropertyName), result.Error);
    }

    [Fact]
    public void IsDateOnlyChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateOnly(MyPropertyName).Error);
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsDateOnly(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    // IsAspNetCoreCompatibleEmail

    [Theory]
    [InlineData("a@b")]
    [InlineData("user@example.com")]
    [InlineData("first.last+tag@sub.example.org")]
    public void IsAspNetCoreCompatibleEmail_ValidValue_Success(string value)
    {
        Assert.Equal(value, Result.IsAspNetCoreCompatibleEmail(value, MyPropertyName).Value);
        Assert.Equal(value, Result.IsAspNetCoreCompatibleEmail(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).IsAspNetCoreCompatibleEmail(MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).IsAspNetCoreCompatibleEmail(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("userexample.com")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("a@b@c")]
    public void IsAspNetCoreCompatibleEmail_InvalidValue_EmailError(string value)
    {
        Assert.Equal(Email(MyPropertyName), Result.IsAspNetCoreCompatibleEmail(value, MyPropertyName).Error);
        Assert.Equal(Email(MultiPartPropertyName), Result.IsAspNetCoreCompatibleEmail(value, PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
        Assert.Equal(Email(MyPropertyName), Result.Success(value).IsAspNetCoreCompatibleEmail(MyPropertyName).Error);
        Assert.Equal(Email(MultiPartPropertyName), Result.Success(value).IsAspNetCoreCompatibleEmail(PropertyPath.Root.Append(MyPropertyName), MyPropertyNameTwo).Error);
    }

    [Fact]
    public void IsAspNetCoreCompatibleEmailChained_FailedResult_ReturnsOriginalError()
    {
        Assert.Same(OtherError, Result.Failure<string>(OtherError).IsAspNetCoreCompatibleEmail(MyPropertyName).Error);
    }
}
