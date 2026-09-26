using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using Microsoft.Extensions.Hosting;

using Xunit.Sdk;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class IsGuidExtensionTests
{
    const string MyPropertyName = "myPropertyName";
    const string MyPropertyNameTwo = "propertyNameSecondPart";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string IsGuidErrorFormat = "'{0}' is not in the correct format.";

    static readonly Error PreviousFailureError = RequestErrors.NewInvalidArg("Previous failure.");

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    public void IsGuid_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .IsGuid(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    public void IsGuid_PreviousError_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .IsGuid(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    public void IsGuidMultiPart_ValidValue_Success(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsGuid(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("d8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25")]
    public void IsGuidMultiPart_PreviousError_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.Failure<string>(PreviousFailureError)
            .IsGuid(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, PreviousFailureError);
    }

    [Fact]
    public void IsGuid_EmptyGuid_Success()
    {
        //Arrange
        var value = Guid.Empty.ToString();

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .IsGuid(MyPropertyName);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.ToString(), value);
    }

    [Theory]
    [InlineData("s8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-00000000000s")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25555")]
    public void IsGuid_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MyPropertyName)
            .IsGuid(MyPropertyName);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, new ValidationError(MyPropertyName, IsGuidErrorFormat, MyPropertyName));
    }

    [Theory]
    [InlineData("s8865f38-600a-4ffd-ab32-25efcca9f54f")]
    [InlineData("00000000-0000-0000-0000-00000000000s")]
    [InlineData("ae4597de-8386-41cd-89da-4188edcaee25555")]
    public void IsGuidMultiPart_InvalidValue_Failure(string value)
    {
        //Arrange

        //Act
        var result = Result.NotNull(value, MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo)
            .IsGuid(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);

        //Assert
        Assert.True(result.IsFailure);

        var multiPartPropertyName = string.Format(MultiPartPropertyNameFormat, MyPropertyName, MyPropertyNameTwo);
        Assert.Equal(result.Error, new ValidationError(multiPartPropertyName, IsGuidErrorFormat, multiPartPropertyName));
    }
}
