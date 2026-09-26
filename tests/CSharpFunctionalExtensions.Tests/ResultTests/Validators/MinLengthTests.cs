namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class MinLengthTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string MinLengthErrorFormat = "The length of '{0}' must be at least {1} characters.";

    #region IEnumerable

    [Fact]
    public void MinLength_IsGreaterThanMinLength_Success()
    {
        //Arrange
        int minLength = 2;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.MinLength(value, minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MinLengthMultiPart_IsGreaterThanMinLength_Success()
    {
        //Arrange
        int minLength = 2;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.MinLength(value, minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
    }

    [Fact]
    public void MinLength_IsEqualToMinLength_Success()
    {
        //Arrange
        int minLength = 3;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.MinLength(value, minLength, MyPropertyName);

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
        var result = Result.MinLength(value, minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

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
        var result = Result.MinLength(value, minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, MinLengthErrorFormat, MyPropertyName, minLength));
    }

    [Fact]
    public void MinLengthMultiPart_LessThanMinLength_Failure()
    {
        //Arrange
        int minLength = 4;
        var value = new List<int> { 1, 2, 3 };

        //Act
        var result = Result.MinLength(value, minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, MinLengthErrorFormat, multiPartPropertyName, minLength));
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
        var result = Result.MinLength(value, minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.MinLength(value, minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, value);
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
        var result = Result.MinLength(value, minLength, MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, MinLengthErrorFormat, MyPropertyName, minLength));
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
        var result = Result.MinLength(value, minLength, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, MinLengthErrorFormat, multiPartPropertyName, minLength));
    }

    #endregion
}
