namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class ExactLengthExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region IEnumerable

    [Fact]
    public void ExactLength_IsEqualToLength_Success()
    {
        //Arrange
        int length = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .ExactLength(length, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void ExactLength_PreviousError_Failure()
    {
        //Arrange
        int length = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .ExactLength(length, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void ExactLengthMultiPart_IsEqualToLength_Success()
    {
        //Arrange
        int length = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExactLength(length, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void ExactLengthMultiPart_PreviousError_Failure()
    {
        //Arrange
        int length = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .ExactLength(length, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void ExactLength_NotEqualToLength_Failure()
    {
        //Arrange
        int length = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .ExactLength(length, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExactCount, ExpectedValidatorErrorStrings.ExactCount, MyPropertyName, length, value.Count()));
    }

    [Fact]
    public void ExactLengthMultiPart_NotEqualToLength_Failure()
    {
        //Arrange
        int length = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExactLength(length, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExactCount, ExpectedValidatorErrorStrings.ExactCount, multiPartPropertyName, length, value.Count()));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString", 10)]
    [InlineData("123", 3)]
    [InlineData("test", 4)]
    [InlineData("JohnDoe", 7)]
    [InlineData("My Awesome Company Name", 23)]
    public void ExactLength_StringIsEqualToLength_Success(string value, int length)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .ExactLength(length, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(23)]
    public void ExactLength_StringIsPreviousError_Failure(int length)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .ExactLength(length, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", 10)]
    [InlineData("123", 3)]
    [InlineData("test", 4)]
    [InlineData("JohnDoe", 7)]
    [InlineData("My Awesome Company Name", 23)]
    public void ExactLengthMultiPart_StringIsEqualToLength_Success(string value, int length)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExactLength(length, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(23)]
    public void ExactLengthMultiPart_StringIsPreviousError_Failure(int length)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .ExactLength(length, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", 11)]
    [InlineData("123", 2)]
    [InlineData("test", 10)]
    [InlineData("JohnDoe", 9)]
    [InlineData("My Awesome Company Name", 30)]
    public void ExactLength_StringIsNotEqualToLength_Success(string value, int length)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .ExactLength(length, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.ExactLength, ExpectedValidatorErrorStrings.ExactLength, MyPropertyName, length, value.Length));
    }

    [Theory]
    [InlineData("testString", 11)]
    [InlineData("123", 2)]
    [InlineData("test", 10)]
    [InlineData("JohnDoe", 9)]
    [InlineData("My Awesome Company Name", 30)]
    public void EqualMultiPart_StringOfDifferingValue_Failure(string value, int length)
    {
        //Arrange

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .ExactLength(length, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.ExactLength, ExpectedValidatorErrorStrings.ExactLength, multiPartPropertyName, length, value.Length));
    }

    #endregion
}
