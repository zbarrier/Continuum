using FluentAssertions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests;

public class CombineForValidationChainTests
{
	const string FromAccountIdProperty = "FromAccountId";
	const string ToAccountIdProperty = "ToAccountId";

	static readonly ValidationError FirstStageError = new(FromAccountIdProperty, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, FromAccountIdProperty);
	static readonly ValidationError SecondStageError = new(ToAccountIdProperty, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual, ToAccountIdProperty, FromAccountIdProperty);

	[Fact]
	public void CombineForValidation_SuccessThenSuccess_Success()
	{
		//Arrange
		var firstStage = Result.Success();

		//Act
		Result result = firstStage.CombineForValidation(Result.Success(), Result.Success());

		//Assert
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public void CombineForValidation_SuccessThenFailure_ReturnsSecondStageErrors()
	{
		//Arrange
		var firstStage = Result.Success();

		//Act
		Result result = firstStage.CombineForValidation(Result.Success(), Result.Failure(SecondStageError));

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(SecondStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_FailureThenFailure_ReturnsFirstStageErrorsOnly()
	{
		//Arrange
		var firstStage = Result.Failure(FirstStageError);

		//Act
		Result result = firstStage.CombineForValidation(Result.Failure(SecondStageError));

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(FirstStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_SuccessThenFailureEnumerable_ReturnsSecondStageErrors()
	{
		//Arrange
		var firstStage = Result.Success();
		var results = new List<Result> { Result.Success(), Result.Failure(SecondStageError) };

		//Act
		Result result = firstStage.CombineForValidation(results.AsEnumerable());

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(SecondStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_FailureThenFailureEnumerable_ReturnsFirstStageErrorsOnly()
	{
		//Arrange
		var firstStage = Result.Failure(FirstStageError);
		var results = new List<Result> { Result.Failure(SecondStageError) };

		//Act
		Result result = firstStage.CombineForValidation(results.AsEnumerable());

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(FirstStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_SuccessThenResultTFailure_ReturnsSecondStageErrors()
	{
		//Arrange
		var firstStage = Result.Success();

		//Act
		Result result = firstStage.CombineForValidation(Result.Success("test"), Result.Failure<string>(SecondStageError));

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(SecondStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_FailureThenResultTFailure_ReturnsFirstStageErrorsOnly()
	{
		//Arrange
		var firstStage = Result.Failure(FirstStageError);

		//Act
		Result result = firstStage.CombineForValidation(Result.Failure<string>(SecondStageError));

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(FirstStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_SuccessThenResultTFailureEnumerable_ReturnsSecondStageErrors()
	{
		//Arrange
		var firstStage = Result.Success();
		var results = new List<Result<string>> { Result.Success("test"), Result.Failure<string>(SecondStageError) };

		//Act
		Result result = firstStage.CombineForValidation(results.AsEnumerable());

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(SecondStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_FailureThenResultTFailureEnumerable_ReturnsFirstStageErrorsOnly()
	{
		//Arrange
		var firstStage = Result.Failure(FirstStageError);
		var results = new List<Result<string>> { Result.Failure<string>(SecondStageError) };

		//Act
		Result result = firstStage.CombineForValidation(results.AsEnumerable());

		//Assert
		result.IsFailure.Should().BeTrue();
		Assert.Equal(FirstStageError, result.Error);
	}

	[Fact]
	public void CombineForValidation_ChainedStages_ComparesValuesOnlyAfterFirstStageSucceeds()
	{
		//Arrange
		var fromAccountId = Guid.NewGuid();
		var toAccountId = fromAccountId;

		//Act
		Result result = Result.CombineForValidation(
				Result.NotEmpty(fromAccountId, FromAccountIdProperty),
				Result.NotEmpty(toAccountId, ToAccountIdProperty))
			.CombineForValidation(
				Result.NotEqual(toAccountId, fromAccountId, ToAccountIdProperty));

		//Assert
		result.IsFailure.Should().BeTrue();
		var validationError = Assert.IsType<ValidationError>(result.Error);
		var entry = Assert.Single(validationError.Entries);
		Assert.Equal(ToAccountIdProperty, entry.Target);
		Assert.Equal(ExpectedValidatorErrorStrings.NotEqual, entry.Format);
	}
}
