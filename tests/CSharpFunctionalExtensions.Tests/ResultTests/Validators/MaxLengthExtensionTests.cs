namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class MaxLengthExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    #region IEnumerable

    [Fact]
    public void MaxLength_IsLessThanMaxLength_Success()
    {
        //Arrange
        int maxLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MaxLength_PreviousError_Failure()
    {
        //Arrange
        int maxLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void MaxLengthMultiPart_IsLessThanMaxLength_Success()
    {
        //Arrange
        int maxLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MaxLengthMultiPart_PreviousError_Failure()
    {
        //Arrange
        int maxLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.Failure<IEnumerable<int>>(PreviousFailureError)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void MaxLength_IsEqualToMaxLength_Success()
    {
        //Arrange
        int maxLength = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MaxLengthMultiPart_IsEqualToMaxLength_Success()
    {
        //Arrange
        int maxLength = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MaxLength_GreaterThanMaxLength_Failure()
    {
        //Arrange
        int maxLength = 3;
        var value = new List<int> { 1, 2, 3, 4 };

        //Act
        var result = Result.NotEmpty(value, MyPropertyName)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.MaxCount, ExpectedValidatorErrorStrings.MaxCount, MyPropertyName, maxLength, value.Count()));
    }

    [Fact]
    public void MaxLengthMultiPart_GreaterThanMaxLength_Failure()
    {
        //Arrange
        int maxLength = 3;
        var value = new List<int> { 1, 2, 3, 4 };

        //Act
        var result = Result.NotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.MaxCount, ExpectedValidatorErrorStrings.MaxCount, multiPartPropertyName, maxLength, value.Count()));
    }

    #endregion

    #region string

    [Theory]
    [InlineData("testString", 10)]
    [InlineData("123", 5)]
    [InlineData("test", 4)]
    [InlineData("JohnDoe", 10)]
    [InlineData("My Awesome Company Name", 23)]
    public void MaxLength_StringIsLessThanOrEqualToMaxLength_Success(string value, int maxLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(23)]
    public void MaxLength_StringPreviousError_Failure(int maxLength)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", 10)]
    [InlineData("123", 5)]
    [InlineData("test", 4)]
    [InlineData("JohnDoe", 10)]
    [InlineData("My Awesome Company Name", 23)]
    public void MaxLengthMultiPart_StringIsLessThanOrEqualToMaxLength_Success(string value, int maxLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(23)]
    public void MaxLengthMultiPart_StringPreviousError_Failure(int maxLength)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("testString", 9)]
    [InlineData("123", 2)]
    [InlineData("test", 3)]
    [InlineData("JohnDoe", 5)]
    [InlineData("My Awesome Company Name", 10)]
    public void MaxLength_StringIsGreaterThanMaxLength_Success(string value, int maxLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .MaxLength(maxLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.MaxLength, ExpectedValidatorErrorStrings.MaxLength, MyPropertyName, maxLength, value.Length));
    }

    [Theory]
    [InlineData("testString", 9)]
    [InlineData("123", 2)]
    [InlineData("test", 3)]
    [InlineData("JohnDoe", 5)]
    [InlineData("My Awesome Company Name", 10)]
    public void MaxLengthMultiPart_StringIsGreaterThanMaxLength_Failure(string value, int maxLength)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .MaxLength(maxLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.MaxLength, ExpectedValidatorErrorStrings.MaxLength, multiPartPropertyName, maxLength, value.Length));
    }

    #endregion
}
