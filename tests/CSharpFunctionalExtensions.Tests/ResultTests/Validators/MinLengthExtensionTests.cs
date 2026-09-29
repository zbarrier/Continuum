namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class MinLengthExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region IEnumerable

    [Fact]
    public void MinLength_IsGreaterThanMinLength_Success()
    {
        //Arrange
        int minLength = 2;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MinLength_PreviousError_Failure()
    {
        //Arrange
        int minLength = 2;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void MinLengthMultiPart_IsGreaterThanMinLength_Success()
    {
        //Arrange
        int minLength = 2;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MinLengthMultiPart_PreviousError_Failure()
    {
        //Arrange
        int minLength = 2;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void MinLength_IsEqualToMinLength_Success()
    {
        //Arrange
        int minLength = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MinLengthMultiPart_IsEqualToMinLength_Success()
    {
        //Arrange
        int minLength = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MinLength_LessThanMinLength_Failure()
    {
        //Arrange
        int minLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.MinCount, ExpectedValidatorErrorStrings.MinCount, MyPropertyName, minLength, value.Count()));
    }

    [Fact]
    public void MinLengthMultiPart_LessThanMinLength_Failure()
    {
        //Arrange
        int minLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.MinCount, ExpectedValidatorErrorStrings.MinCount, multiPartPropertyName, minLength, value.Count()));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString", 10)]
    [InlineData("123", 2)]
    [InlineData("test", 2)]
    [InlineData("JohnDoe", 7)]
    [InlineData("My Awesome Company Name", 23)]
    public void MinLength_StringIsGreaterThanOrEqualToMinLength_Success(string value, int minLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(23)]
    public void MinLength_StringPreviousError_Failure(int minLength)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", 10)]
    [InlineData("123", 2)]
    [InlineData("test", 2)]
    [InlineData("JohnDoe", 7)]
    [InlineData("My Awesome Company Name", 23)]
    public void MinLengthMultiPart_StringIsGreaterThanOrEqualToMinLength_Success(string value, int minLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(23)]
    public void MinLengthMultiPart_StringPreviousError_Failure(int minLength)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", 11)]
    [InlineData("123", 4)]
    [InlineData("test", 10)]
    [InlineData("JohnDoe", 10)]
    [InlineData("My Awesome Company Name", 24)]
    public void MinLength_StringIsGreaterThanMaxLength_Success(string value, int minLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MinLength(minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.MinLength, ExpectedValidatorErrorStrings.MinLength, MyPropertyName, minLength, value.Length));
    }

    [Theory]
    [InlineData("testString", 11)]
    [InlineData("123", 4)]
    [InlineData("test", 10)]
    [InlineData("JohnDoe", 10)]
    [InlineData("My Awesome Company Name", 24)]
    public void MinLengthMultiPart_StringIsGreaterThanMaxLength_Failure(string value, int minLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MinLength(minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.MinLength, ExpectedValidatorErrorStrings.MinLength, multiPartPropertyName, minLength, value.Length));
    }

    #endregion
}
