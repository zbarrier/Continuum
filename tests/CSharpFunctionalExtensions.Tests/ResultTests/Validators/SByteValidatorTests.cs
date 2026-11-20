namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class SByteValidatorTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";
    const string MultiPartPropertyName = MyPropertyName + "." + MyPropertyNameTwo;

    static readonly PropertyPath Path = PropertyPath.Root.Append(MyPropertyName);
    static readonly ValidationError OtherError = new("other", "other_code", "'{0}' other.", "other");

    static ValidationError Comparison(string propertyName, string code, string template, sbyte comparisonValue) =>
        new(propertyName, code, template, propertyName, comparisonValue);

    static ValidationError Between(string propertyName, string code, string template, sbyte from, sbyte to, sbyte value) =>
        new(propertyName, code, template, propertyName, from, to, value);

    static ValidationError NoArgs(string propertyName, string code, string template) =>
        new(propertyName, code, template, propertyName);

    [Fact]
    public void ErrorArgument_SByte_ConvertsToInt64()
    {
        ErrorArgument argument = (sbyte)-128;

        Assert.Equal(ErrorArgumentKind.Int64, argument.Kind);
        Assert.Equal(-128L, argument.Value);
    }

    // Equal / NotEqual

    [Theory]
    [InlineData(-128)]
    [InlineData(-1)]
    [InlineData(127)]
    public void Equal_SameValue_Success(sbyte value)
    {
        Assert.Equal(value, Result.Equal(value, value, MyPropertyName).Value);
        Assert.Equal(value, Result.Equal(value, value, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).Equal(value, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).Equal(value, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(-128, 127)]
    public void Equal_DifferentValue_EqualError(sbyte value, sbyte expected)
    {
        var error = Comparison(MyPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, expected);
        var multiPartError = Comparison(MultiPartPropertyName, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal, expected);

        Assert.Equal(error, Result.Equal(value, expected, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Equal(value, expected, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).Equal(expected, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).Equal(expected, Path, MyPropertyNameTwo).Error);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(-128, 127)]
    public void NotEqual_DifferentValue_Success(sbyte value, sbyte other)
    {
        Assert.Equal(value, Result.NotEqual(value, other, MyPropertyName).Value);
        Assert.Equal(value, Result.NotEqual(value, other, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).NotEqual(other, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).NotEqual(other, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-128)]
    [InlineData(-1)]
    public void NotEqual_SameValue_NotEqualError(sbyte value)
    {
        var error = Comparison(MyPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, value);
        var multiPartError = Comparison(MultiPartPropertyName, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, value);

        Assert.Equal(error, Result.NotEqual(value, value, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.NotEqual(value, value, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).NotEqual(value, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).NotEqual(value, Path, MyPropertyNameTwo).Error);
    }

    // GreaterThan / GreaterThanOrEqual

    [Theory]
    [InlineData(-127, -128)]
    [InlineData(0, -1)]
    [InlineData(127, 126)]
    public void GreaterThan_GreaterValue_Success(sbyte value, sbyte comparisonValue)
    {
        Assert.Equal(value, Result.GreaterThan(value, comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.GreaterThan(value, comparisonValue, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).GreaterThan(comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).GreaterThan(comparisonValue, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-2, -1)]
    [InlineData(-128, 127)]
    public void GreaterThan_LessThanOrEqualValue_GreaterThanError(sbyte value, sbyte comparisonValue)
    {
        var error = Comparison(MyPropertyName, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan, comparisonValue);
        var multiPartError = Comparison(MultiPartPropertyName, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan, comparisonValue);

        Assert.Equal(error, Result.GreaterThan(value, comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.GreaterThan(value, comparisonValue, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).GreaterThan(comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).GreaterThan(comparisonValue, Path, MyPropertyNameTwo).Error);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, -1)]
    [InlineData(127, -128)]
    public void GreaterThanOrEqual_GreaterOrEqualValue_Success(sbyte value, sbyte comparisonValue)
    {
        Assert.Equal(value, Result.GreaterThanOrEqual(value, comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.GreaterThanOrEqual(value, comparisonValue, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).GreaterThanOrEqual(comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).GreaterThanOrEqual(comparisonValue, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-2, -1)]
    [InlineData(-128, 127)]
    public void GreaterThanOrEqual_LessValue_GreaterThanOrEqualError(sbyte value, sbyte comparisonValue)
    {
        var error = Comparison(MyPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, comparisonValue);
        var multiPartError = Comparison(MultiPartPropertyName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, comparisonValue);

        Assert.Equal(error, Result.GreaterThanOrEqual(value, comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.GreaterThanOrEqual(value, comparisonValue, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).GreaterThanOrEqual(comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).GreaterThanOrEqual(comparisonValue, Path, MyPropertyNameTwo).Error);
    }

    // LessThan / LessThanOrEqual

    [Theory]
    [InlineData(-128, -127)]
    [InlineData(-1, 0)]
    [InlineData(126, 127)]
    public void LessThan_LesserValue_Success(sbyte value, sbyte comparisonValue)
    {
        Assert.Equal(value, Result.LessThan(value, comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.LessThan(value, comparisonValue, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).LessThan(comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).LessThan(comparisonValue, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-1, -2)]
    [InlineData(127, -128)]
    public void LessThan_GreaterThanOrEqualValue_LessThanError(sbyte value, sbyte comparisonValue)
    {
        var error = Comparison(MyPropertyName, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, comparisonValue);
        var multiPartError = Comparison(MultiPartPropertyName, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, comparisonValue);

        Assert.Equal(error, Result.LessThan(value, comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.LessThan(value, comparisonValue, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).LessThan(comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).LessThan(comparisonValue, Path, MyPropertyNameTwo).Error);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-1, 0)]
    [InlineData(-128, 127)]
    public void LessThanOrEqual_LessOrEqualValue_Success(sbyte value, sbyte comparisonValue)
    {
        Assert.Equal(value, Result.LessThanOrEqual(value, comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.LessThanOrEqual(value, comparisonValue, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).LessThanOrEqual(comparisonValue, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).LessThanOrEqual(comparisonValue, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-1, -2)]
    [InlineData(127, -128)]
    public void LessThanOrEqual_GreaterValue_LessThanOrEqualError(sbyte value, sbyte comparisonValue)
    {
        var error = Comparison(MyPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, comparisonValue);
        var multiPartError = Comparison(MultiPartPropertyName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, comparisonValue);

        Assert.Equal(error, Result.LessThanOrEqual(value, comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.LessThanOrEqual(value, comparisonValue, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).LessThanOrEqual(comparisonValue, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).LessThanOrEqual(comparisonValue, Path, MyPropertyNameTwo).Error);
    }

    // InclusiveBetween / ExclusiveBetween

    [Theory]
    [InlineData(-10, -10, 10)]
    [InlineData(0, -10, 10)]
    [InlineData(10, -10, 10)]
    [InlineData(-128, -128, 127)]
    public void InclusiveBetween_InRange_Success(sbyte value, sbyte from, sbyte to)
    {
        Assert.Equal(value, Result.InclusiveBetween(value, from, to, MyPropertyName).Value);
        Assert.Equal(value, Result.InclusiveBetween(value, from, to, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).InclusiveBetween(from, to, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).InclusiveBetween(from, to, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-11, -10, 10)]
    [InlineData(11, -10, 10)]
    public void InclusiveBetween_OutOfRange_InclusiveBetweenError(sbyte value, sbyte from, sbyte to)
    {
        var error = Between(MyPropertyName, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, from, to, value);
        var multiPartError = Between(MultiPartPropertyName, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, from, to, value);

        Assert.Equal(error, Result.InclusiveBetween(value, from, to, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.InclusiveBetween(value, from, to, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).InclusiveBetween(from, to, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).InclusiveBetween(from, to, Path, MyPropertyNameTwo).Error);
    }

    [Theory]
    [InlineData(-9, -10, 10)]
    [InlineData(0, -10, 10)]
    [InlineData(9, -10, 10)]
    public void ExclusiveBetween_InRange_Success(sbyte value, sbyte from, sbyte to)
    {
        Assert.Equal(value, Result.ExclusiveBetween(value, from, to, MyPropertyName).Value);
        Assert.Equal(value, Result.ExclusiveBetween(value, from, to, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).ExclusiveBetween(from, to, MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).ExclusiveBetween(from, to, Path, MyPropertyNameTwo).Value);
    }

    [Theory]
    [InlineData(-10, -10, 10)]
    [InlineData(10, -10, 10)]
    [InlineData(-128, -10, 10)]
    public void ExclusiveBetween_OutOfRange_ExclusiveBetweenError(sbyte value, sbyte from, sbyte to)
    {
        var error = Between(MyPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, from, to, value);
        var multiPartError = Between(MultiPartPropertyName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, from, to, value);

        Assert.Equal(error, Result.ExclusiveBetween(value, from, to, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.ExclusiveBetween(value, from, to, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).ExclusiveBetween(from, to, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).ExclusiveBetween(from, to, Path, MyPropertyNameTwo).Error);
    }

    // NotEmpty / NotNull / NotNullOrEmpty

    [Theory]
    [InlineData(-128)]
    [InlineData(-1)]
    [InlineData(127)]
    public void NotEmpty_NonZero_Success(sbyte value)
    {
        Assert.Equal(value, Result.NotEmpty(value, MyPropertyName).Value);
        Assert.Equal(value, Result.NotEmpty(value, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(value).NotEmpty(MyPropertyName).Value);
        Assert.Equal(value, Result.Success(value).NotEmpty(Path, MyPropertyNameTwo).Value);
    }

    [Fact]
    public void NotEmpty_Zero_NotEmptyError()
    {
        var error = NoArgs(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty);
        var multiPartError = NoArgs(MultiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty);

        Assert.Equal(error, Result.NotEmpty((sbyte)0, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.NotEmpty((sbyte)0, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success((sbyte)0).NotEmpty(MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success((sbyte)0).NotEmpty(Path, MyPropertyNameTwo).Error);
    }

    [Theory]
    [InlineData(-128)]
    [InlineData(0)]
    [InlineData(127)]
    public void NotNull_HasValue_Success(sbyte value)
    {
        sbyte? nullable = value;

        Assert.Equal(value, Result.NotNull(nullable, MyPropertyName).Value);
        Assert.Equal(value, Result.NotNull(nullable, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(nullable).NotNull(MyPropertyName).Value);
        Assert.Equal(value, Result.Success(nullable).NotNull(Path, MyPropertyNameTwo).Value);
    }

    [Fact]
    public void NotNull_Null_NotNullError()
    {
        sbyte? value = null;
        var error = NoArgs(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull);
        var multiPartError = NoArgs(MultiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull);

        Assert.Equal(error, Result.NotNull(value, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.NotNull(value, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).NotNull(MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).NotNull(Path, MyPropertyNameTwo).Error);
    }

    [Theory]
    [InlineData(-128)]
    [InlineData(-1)]
    [InlineData(127)]
    public void NotNullOrEmpty_NonZero_Success(sbyte value)
    {
        sbyte? nullable = value;

        Assert.Equal(value, Result.NotNullOrEmpty(nullable, MyPropertyName).Value);
        Assert.Equal(value, Result.NotNullOrEmpty(nullable, Path, MyPropertyNameTwo).Value);
        Assert.Equal(value, Result.Success(nullable).NotNullOrEmpty(MyPropertyName).Value);
        Assert.Equal(value, Result.Success(nullable).NotNullOrEmpty(Path, MyPropertyNameTwo).Value);
    }

    [Fact]
    public void NotNullOrEmpty_Null_NotNullError()
    {
        sbyte? value = null;
        var error = NoArgs(MyPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull);
        var multiPartError = NoArgs(MultiPartPropertyName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull);

        Assert.Equal(error, Result.NotNullOrEmpty(value, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.NotNullOrEmpty(value, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).NotNullOrEmpty(MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).NotNullOrEmpty(Path, MyPropertyNameTwo).Error);
    }

    [Fact]
    public void NotNullOrEmpty_Zero_NotEmptyError()
    {
        sbyte? value = 0;
        var error = NoArgs(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty);
        var multiPartError = NoArgs(MultiPartPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty);

        Assert.Equal(error, Result.NotNullOrEmpty(value, MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.NotNullOrEmpty(value, Path, MyPropertyNameTwo).Error);
        Assert.Equal(error, Result.Success(value).NotNullOrEmpty(MyPropertyName).Error);
        Assert.Equal(multiPartError, Result.Success(value).NotNullOrEmpty(Path, MyPropertyNameTwo).Error);
    }

    // Chained validators pass a previous failure through unchanged.

    [Fact]
    public void Chained_FailedResult_ReturnsOriginalError()
    {
        var failed = Result.Failure<sbyte>(OtherError);
        var failedNullable = Result.Failure<sbyte?>(OtherError);

        Assert.Same(OtherError, failed.Equal(1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.Equal(1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.NotEqual(1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.NotEqual(1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.GreaterThan(1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.GreaterThan(1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.GreaterThanOrEqual(1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.GreaterThanOrEqual(1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.LessThan(1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.LessThan(1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.LessThanOrEqual(1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.LessThanOrEqual(1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.InclusiveBetween(-1, 1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.InclusiveBetween(-1, 1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.ExclusiveBetween(-1, 1, MyPropertyName).Error);
        Assert.Same(OtherError, failed.ExclusiveBetween(-1, 1, Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failed.NotEmpty(MyPropertyName).Error);
        Assert.Same(OtherError, failed.NotEmpty(Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failedNullable.NotNull(MyPropertyName).Error);
        Assert.Same(OtherError, failedNullable.NotNull(Path, MyPropertyNameTwo).Error);
        Assert.Same(OtherError, failedNullable.NotNullOrEmpty(MyPropertyName).Error);
        Assert.Same(OtherError, failedNullable.NotNullOrEmpty(Path, MyPropertyNameTwo).Error);
    }
}
