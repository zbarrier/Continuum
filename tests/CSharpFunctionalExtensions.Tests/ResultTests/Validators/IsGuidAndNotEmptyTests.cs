using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class IsGuidAndNotEmptyTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";


    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    [InlineData("bc8b0ae1-0370-477f-8109-3fc373c1cc28")]
    public void IsGuidAndNotEmpty_NonEmptyValidGuid_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsGuidAndNotEmpty(value, MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    [InlineData("bc8b0ae1-0370-477f-8109-3fc373c1cc28")]
    public void IsGuidAndNotEmptyMultiPart_NonEmptyValidGuid_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.IsGuidAndNotEmpty(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Fact]
    public void IsGuidAndNotEmpty_EmptyGuid_Failure()
    {
        //Arrange
        var value = Guid.Empty.ToString();

        //Act
        var result = Result.IsGuidAndNotEmpty(value, MyPropertyName);

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
        var result = Result.IsGuid(value, MyPropertyName);

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
        var result = Result.IsGuid(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, multiPartPropertyName));
    }
}
