namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class BetweenLengthTests
{
    const string MyPropertyName = "myPropertyName";
    const string MultiPartPropertyNameFormat = "{0} {1}";
    const string SecondPart = "secondPart";

    [Theory]
    [InlineData("ab", 2, 4)]
    [InlineData("abc", 2, 4)]
    [InlineData("abcd", 2, 4)]
    public void LengthBetween_WithinRange_Success(string value, int min, int max)
    {
        var result = Result.LengthBetween(value, min, max, MyPropertyName);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData("a", 2, 4)]
    [InlineData("abcde", 2, 4)]
    public void LengthBetween_OutsideRange_Failure(string value, int min, int max)
    {
        var result = Result.LengthBetween(value, min, max, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LengthBetween, ExpectedValidatorErrorStrings.LengthBetween, MyPropertyName, min, max, value.Length), result.Error);
    }

    [Fact]
    public void LengthBetweenMultiPart_OutsideRange_Failure()
    {
        var result = Result.LengthBetween("a", 2, 4, MultiPartPropertyNameFormat, MyPropertyName, SecondPart);

        var propertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, SecondPart);
        Assert.True(result.IsFailure);
        Assert.Equal(new ValidationError(propertyName, ExpectedValidationErrorCodes.LengthBetween, ExpectedValidatorErrorStrings.LengthBetween, propertyName, 2, 4, 1), result.Error);
    }

    [Fact]
    public void LengthBetweenExtension_OutsideRange_Failure()
    {
        var result = Result.Success("abcde").LengthBetween(2, 4, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.LengthBetween, ExpectedValidatorErrorStrings.LengthBetween, MyPropertyName, 2, 4, 5), result.Error);
    }

    [Fact]
    public void LengthBetweenExtension_WithinRange_Success()
    {
        var result = Result.Success("abc").LengthBetween(2, 4, MultiPartPropertyNameFormat, MyPropertyName, SecondPart);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void LengthBetweenExtension_PreviousFailure_IsPreserved()
    {
        var error = new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName);

        var result = Result.Failure<string>(error).LengthBetween(2, 4, MyPropertyName);

        Assert.Equal(error, result.Error);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CountBetween_WithinRange_Success(int count)
    {
        IEnumerable<int> value = Enumerable.Range(0, count).ToList();

        var result = Result.CountBetween(value, 2, 4, MyPropertyName);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void CountBetween_OutsideRange_Failure(int count)
    {
        IEnumerable<int> value = Enumerable.Range(0, count).ToList();

        var result = Result.CountBetween(value, 2, 4, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.CountBetween, ExpectedValidatorErrorStrings.CountBetween, MyPropertyName, 2, 4, count), result.Error);
    }

    [Fact]
    public void CountBetweenMultiPart_OutsideRange_Failure()
    {
        IEnumerable<int> value = new List<int> { 1 };

        var result = Result.CountBetween(value, 2, 4, MultiPartPropertyNameFormat, MyPropertyName, SecondPart);

        var propertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, SecondPart);
        Assert.True(result.IsFailure);
        Assert.Equal(new ValidationError(propertyName, ExpectedValidationErrorCodes.CountBetween, ExpectedValidatorErrorStrings.CountBetween, propertyName, 2, 4, 1), result.Error);
    }

    [Fact]
    public void CountBetweenExtension_OutsideRange_Failure()
    {
        IEnumerable<int> value = new List<int> { 1, 2, 3, 4, 5 };

        var result = Result.Success(value).CountBetween(2, 4, MyPropertyName);

        Assert.True(result.IsFailure);
        Assert.Equal(new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.CountBetween, ExpectedValidatorErrorStrings.CountBetween, MyPropertyName, 2, 4, 5), result.Error);
    }

    [Fact]
    public void CountBetweenExtension_WithinRange_Success()
    {
        IEnumerable<int> value = new List<int> { 1, 2, 3 };

        var result = Result.Success(value).CountBetween(2, 4, MultiPartPropertyNameFormat, MyPropertyName, SecondPart);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CountBetween_FormatsEnglishMessage()
    {
        IEnumerable<int> value = new List<int> { 1 };

        var result = Result.CountBetween(value, 2, 4, "Items");

        Assert.Equal("'Items' must contain between 2 and 4 items.", ((ValidationError)result.Error).Entries[0].GetFormattedMessage(System.Globalization.CultureInfo.InvariantCulture));
    }
}
