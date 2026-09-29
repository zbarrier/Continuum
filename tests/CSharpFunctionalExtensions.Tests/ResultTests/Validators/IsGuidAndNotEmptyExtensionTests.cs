using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class IsGuidAndNotEmptyExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    [InlineData("bc8b0ae1-0370-477f-8109-3fc373c1cc28")]
    public void IsGuidAndNotEmpty_NonEmptyValidGuid_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .IsGuidAndNotEmpty(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Fact]
    public void IsGuidAndNotEmpty_PreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .IsGuidAndNotEmpty(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    [InlineData("bc8b0ae1-0370-477f-8109-3fc373c1cc28")]
    public void IsGuidAndNotEmptyMultiPart_NonEmptyValidGuid_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsGuidAndNotEmpty(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Fact]
    public void IsGuidAndNotEmptyMultiPart_PreviousError_Failure()
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .IsGuidAndNotEmpty(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void IsGuidAndNotEmpty_EmptyGuid_Failure()
    {
        //Arrange
        var value = Guid.Empty.ToString();

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .IsGuidAndNotEmpty(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, MyPropertyName));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("s8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-00000000000s")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25555")]
    public void IsGuidAndNotEmpty_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .IsGuid(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("test")]
    [InlineData("s8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-00000000000s")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25555")]
    public void IsGuidAndNotEmptyMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsGuid(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, multiPartPropertyName));
    }
}
