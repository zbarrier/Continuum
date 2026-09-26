using System.Net;

using FluentAssertions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests;

public class CombineForValidationTests
{
    const string FirstNameProperty = "FirstName";
    const string LastNameProperty = "LastName";

    const string MultiPartPropertyNameFormat = "{0} {1}";

    const string ValidationErrorFormat = "'{0}' must not be empty.";

    static readonly Exception CombineNotSupportedException = new NotSupportedException("Only validation errors can be combined.");

    [Fact]
    public void CombineForValidation_ResultWithoutValidationErrors_Success()
    {
        //Arrange
        Result result1 = Result.Success();
        Result result2 = Result.Success();
        Result result3 = Result.Success();

        //Act
        Result result = Result.CombineForValidation(result1, result2, result3);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineForValidation_ResultWithoutValidationErrorsEnumerable_Success()
    {
        //Arrange
        Result result1 = Result.Success();
        Result result2 = Result.Success();
        Result result3 = Result.Success();
        var results = new List<Result> { result1, result2, result3 };

        //Act
        Result result = Result.CombineForValidation(results.AsEnumerable());

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineForValidation_ResultWithValidationErrors_Success()
    {
        //Arrange
        Result result1 = Result.Success();
        Result result2 = Result.Failure(new ValidationError(FirstNameProperty, "'{0}' must not be empty.", FirstNameProperty));
        Result result3 = Result.Failure(new ValidationError(LastNameProperty, "'{0}' must not be empty.", LastNameProperty));

        //Act
        Result result = Result.CombineForValidation(result1, result2, result3);

        //Assert
        result.IsSuccess.Should().BeFalse();

        var valiationError = result.Error as ValidationError;
        Assert.NotNull(valiationError);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, valiationError.HttpStatusCode);
        Assert.Equal((int)Grpc.Core.StatusCode.InvalidArgument, valiationError.GrpcStatusCode);

        var firstEntry = valiationError.Entries.First();
        Assert.Equal(FirstNameProperty, firstEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, firstEntry.Format);
        Assert.Equal(FirstNameProperty, firstEntry.Arguments.First());

        var secondEntry = valiationError.Entries.Last();
        Assert.Equal(LastNameProperty, secondEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, secondEntry.Format);
        Assert.Equal(LastNameProperty, secondEntry.Arguments.First());
    }

    [Fact]
    public void CombineForValidation_ResultWithValidationErrorsEnumerable_Success()
    {
        //Arrange
        Result result1 = Result.Success();
        Result result2 = Result.Failure(new ValidationError(FirstNameProperty, "'{0}' must not be empty.", FirstNameProperty));
        Result result3 = Result.Failure(new ValidationError(LastNameProperty, "'{0}' must not be empty.", LastNameProperty));
        var results = new List<Result> { result1, result2, result3 };

        //Act
        Result result = Result.CombineForValidation(results.AsEnumerable());

        //Assert
        result.IsSuccess.Should().BeFalse();

        var valiationError = result.Error as ValidationError;
        Assert.NotNull(valiationError);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, valiationError.HttpStatusCode);
        Assert.Equal((int)Grpc.Core.StatusCode.InvalidArgument, valiationError.GrpcStatusCode);

        var firstEntry = valiationError.Entries.First();
        Assert.Equal(FirstNameProperty, firstEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, firstEntry.Format);
        Assert.Equal(FirstNameProperty, firstEntry.Arguments.First());

        var secondEntry = valiationError.Entries.Last();
        Assert.Equal(LastNameProperty, secondEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, secondEntry.Format);
        Assert.Equal(LastNameProperty, secondEntry.Arguments.First());
    }


    [Fact]
    public void CombineForValidation_ResultTWithoutValidationErrors_Success()
    {
        //Arrange
        var result1 = Result.Success("test");
        var result2 = Result.Success("test");
        var result3 = Result.Success("test");

        //Act
        Result result = Result.CombineForValidation(result1, result2, result3);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineForValidation_ResultTWithoutValidationErrorsEnumerable_Success()
    {
        //Arrange
        var result1 = Result.Success("test");
        var result2 = Result.Success("test");
        var result3 = Result.Success("test");
        var results = new List<Result> { result1, result2, result3 };

        //Act
        Result result = Result.CombineForValidation(results.AsEnumerable());

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineForValidation_ResultTWithValidationErrors_Success()
    {
        //Arrange
        var result1 = Result.Success("test");
        var result2 = Result.Failure<string>(new ValidationError(FirstNameProperty, "'{0}' must not be empty.", FirstNameProperty));
        var result3 = Result.Failure<string>(new ValidationError(LastNameProperty, "'{0}' must not be empty.", LastNameProperty));

        //Act
        Result result = Result.CombineForValidation(result1, result2, result3);

        //Assert
        result.IsSuccess.Should().BeFalse();

        var valiationError = result.Error as ValidationError;
        Assert.NotNull(valiationError);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, valiationError.HttpStatusCode);
        Assert.Equal((int)Grpc.Core.StatusCode.InvalidArgument, valiationError.GrpcStatusCode);

        var firstEntry = valiationError.Entries.First();
        Assert.Equal(FirstNameProperty, firstEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, firstEntry.Format);
        Assert.Equal(FirstNameProperty, firstEntry.Arguments.First());

        var secondEntry = valiationError.Entries.Last();
        Assert.Equal(LastNameProperty, secondEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, secondEntry.Format);
        Assert.Equal(LastNameProperty, secondEntry.Arguments.First());
    }

    [Fact]
    public void CombineForValidation_ResultTWithValidationErrorsEnumerable_Success()
    {
        //Arrange
        var result1 = Result.Success("test");
        var result2 = Result.Failure<string>(new ValidationError(FirstNameProperty, "'{0}' must not be empty.", FirstNameProperty));
        var result3 = Result.Failure<string>(new ValidationError(LastNameProperty, "'{0}' must not be empty.", LastNameProperty));
        var results = new List<Result> { result1, result2, result3 };

        //Act
        Result result = Result.CombineForValidation(results.AsEnumerable());

        //Assert
        result.IsSuccess.Should().BeFalse();

        var valiationError = result.Error as ValidationError;
        Assert.NotNull(valiationError);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, valiationError.HttpStatusCode);
        Assert.Equal((int)Grpc.Core.StatusCode.InvalidArgument, valiationError.GrpcStatusCode);

        var firstEntry = valiationError.Entries.First();
        Assert.Equal(FirstNameProperty, firstEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, firstEntry.Format);
        Assert.Equal(FirstNameProperty, firstEntry.Arguments.First());

        var secondEntry = valiationError.Entries.Last();
        Assert.Equal(LastNameProperty, secondEntry.Identifier);
        Assert.Equal(ValidationErrorFormat, secondEntry.Format);
        Assert.Equal(LastNameProperty, secondEntry.Arguments.First());
    }


    [Fact]
    public void CombineForValidation_ResultWithNonValidationErrors_Throws()
    {
        //Arrange
        Result result1 = Result.Success();
        Result result2 = Result.Failure(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
        Result result3 = Result.Failure(RequestErrors.NewInvalidArg("'{0}' must not be empty.", LastNameProperty));

        Action action = () => { Result result = Result.CombineForValidation(result1, result2, result3); };

        //Act
        action.Should().Throw<ArgumentException>().WithMessage("Only validation errors are expected. (Parameter 'failedResults')");

        //Assert
    }

    [Fact]
    public void CombineForValidation_ResultWithNonValidationErrorsEnumerable_Throws()
    {
        //Arrange
        Result result1 = Result.Success();
        Result result2 = Result.Failure(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
        Result result3 = Result.Failure(RequestErrors.NewInvalidArg("'{0}' must not be empty.", LastNameProperty));
        var results = new List<Result> { result1, result2, result3 };

        Action action = () => { Result result = Result.CombineForValidation(results.AsEnumerable()); };

        //Act
        action.Should().Throw<ArgumentException>().WithMessage("Only validation errors are expected. (Parameter 'failedResults')");

        //Assert
    }

    [Fact]
    public void CombineForValidation_ResultTWithNonValidationErrors_Throws()
    {
        //Arrange
        var result1 = Result.Success("test");
        var result2 = Result.Failure<string>(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
        var result3 = Result.Failure<string>(RequestErrors.NewInvalidArg("'{0}' must not be empty.", LastNameProperty));

        Action action = () => { Result result = Result.CombineForValidation(result1, result2, result3); };

        //Act
        action.Should().Throw<ArgumentException>().WithMessage("Only validation errors are expected. (Parameter 'failedResults')");

        //Assert
    }

    [Fact]
    public void CombineForValidation_ResultTWithNonValidationErrorsEnumerable_Throws()
    {
        //Arrange
        var result1 = Result.Success("test");
        var result2 = Result.Failure<string>(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
        var result3 = Result.Failure<string>(RequestErrors.NewInvalidArg("'{0}' must not be empty.", LastNameProperty));
        var results = new List<Result> { result1, result2, result3 };

        Action action = () => { Result result = Result.CombineForValidation(results.AsEnumerable()); };

        //Act
        action.Should().Throw<ArgumentException>().WithMessage("Only validation errors are expected. (Parameter 'failedResults')");

        //Assert
    }
}
