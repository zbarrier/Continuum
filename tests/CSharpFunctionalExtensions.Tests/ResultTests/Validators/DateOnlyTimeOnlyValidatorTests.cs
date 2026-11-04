namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class DateOnlyTimeOnlyValidatorTests
{
	const string Name = "MyProperty";
	static readonly PropertyPath Path = PropertyPath.Root.Append("Parent");
	const string FullName = "Parent.MyProperty";

	static readonly DateOnly D1 = new(2024, 1, 1);
	static readonly DateOnly D2 = new(2024, 1, 2);
	static readonly DateOnly D3 = new(2024, 1, 3);
	static readonly TimeOnly T1 = new(8, 0);
	static readonly TimeOnly T2 = new(9, 0);
	static readonly TimeOnly T3 = new(10, 0);

	static ValidationError Comparison(string name, string code, string message, ErrorArgument comparison)
		=> new(name, code, message, name, comparison);

	static ValidationError Between(string name, string code, string message, ErrorArgument from, ErrorArgument to, ErrorArgument value)
		=> new(name, code, message, name, from, to, value);

	static ValidationError NoArgs(string name, string code, string message)
		=> new(name, code, message, name);

	[Fact]
	public void LessThan_DateOnly()
	{
		Assert.True(Result.LessThan(D1, D2, Name).IsSuccess);
		Assert.Equal(Comparison(Name, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, D2), Result.LessThan(D2, D2, Name).Error);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, D2), Result.LessThan(D3, D2, Path, Name).Error);
		Assert.True(Result.Success(D1).LessThan(D2, Name).IsSuccess);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, D2), Result.Success(D2).LessThan(D2, Path, Name).Error);
	}

	[Fact]
	public void LessThan_TimeOnly()
	{
		Assert.True(Result.LessThan(T1, T2, Name).IsSuccess);
		Assert.Equal(Comparison(Name, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, T2), Result.LessThan(T2, T2, Name).Error);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, T2), Result.LessThan(T3, T2, Path, Name).Error);
		Assert.True(Result.Success(T1).LessThan(T2, Name).IsSuccess);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan, T2), Result.Success(T2).LessThan(T2, Path, Name).Error);
	}

	[Fact]
	public void LessThanOrEqual_DateOnly()
	{
		Assert.True(Result.LessThanOrEqual(D2, D2, Name).IsSuccess);
		Assert.Equal(Comparison(Name, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, D2), Result.LessThanOrEqual(D3, D2, Name).Error);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, D2), Result.LessThanOrEqual(D3, D2, Path, Name).Error);
		Assert.True(Result.Success(D2).LessThanOrEqual(D2, Name).IsSuccess);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, D2), Result.Success(D3).LessThanOrEqual(D2, Path, Name).Error);
	}

	[Fact]
	public void LessThanOrEqual_TimeOnly()
	{
		Assert.True(Result.LessThanOrEqual(T2, T2, Name).IsSuccess);
		Assert.Equal(Comparison(Name, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, T2), Result.LessThanOrEqual(T3, T2, Name).Error);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, T2), Result.LessThanOrEqual(T3, T2, Path, Name).Error);
		Assert.True(Result.Success(T2).LessThanOrEqual(T2, Name).IsSuccess);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual, T2), Result.Success(T3).LessThanOrEqual(T2, Path, Name).Error);
	}

	[Fact]
	public void GreaterThan_TimeOnly()
	{
		Assert.True(Result.GreaterThan(T3, T2, Name).IsSuccess);
		Assert.Equal(Comparison(Name, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan, T2), Result.GreaterThan(T2, T2, Name).Error);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan, T2), Result.GreaterThan(T1, T2, Path, Name).Error);
		Assert.True(Result.Success(T3).GreaterThan(T2, Name).IsSuccess);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan, T2), Result.Success(T2).GreaterThan(T2, Path, Name).Error);
	}

	[Fact]
	public void GreaterThanOrEqual_TimeOnly()
	{
		Assert.True(Result.GreaterThanOrEqual(T2, T2, Name).IsSuccess);
		Assert.Equal(Comparison(Name, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, T2), Result.GreaterThanOrEqual(T1, T2, Name).Error);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, T2), Result.GreaterThanOrEqual(T1, T2, Path, Name).Error);
		Assert.True(Result.Success(T2).GreaterThanOrEqual(T2, Name).IsSuccess);
		Assert.Equal(Comparison(FullName, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual, T2), Result.Success(T1).GreaterThanOrEqual(T2, Path, Name).Error);
	}

	[Fact]
	public void InclusiveBetween_DateOnly()
	{
		Assert.True(Result.InclusiveBetween(D1, D1, D3, Name).IsSuccess);
		Assert.Equal(Between(Name, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, D1, D2, D3), Result.InclusiveBetween(D3, D1, D2, Name).Error);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, D2, D3, D1), Result.InclusiveBetween(D1, D2, D3, Path, Name).Error);
		Assert.True(Result.Success(D3).InclusiveBetween(D1, D3, Name).IsSuccess);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, D1, D2, D3), Result.Success(D3).InclusiveBetween(D1, D2, Path, Name).Error);
		Assert.Throws<ArgumentOutOfRangeException>(() => Result.InclusiveBetween(D1, D3, D1, Name));
	}

	[Fact]
	public void InclusiveBetween_TimeOnly()
	{
		Assert.True(Result.InclusiveBetween(T1, T1, T3, Name).IsSuccess);
		Assert.Equal(Between(Name, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, T1, T2, T3), Result.InclusiveBetween(T3, T1, T2, Name).Error);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, T2, T3, T1), Result.InclusiveBetween(T1, T2, T3, Path, Name).Error);
		Assert.True(Result.Success(T3).InclusiveBetween(T1, T3, Name).IsSuccess);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.InclusiveBetween, ExpectedValidatorErrorStrings.InclusiveBetween, T1, T2, T3), Result.Success(T3).InclusiveBetween(T1, T2, Path, Name).Error);
		Assert.Throws<ArgumentOutOfRangeException>(() => Result.InclusiveBetween(T1, T3, T1, Name));
	}

	[Fact]
	public void ExclusiveBetween_DateOnly()
	{
		Assert.True(Result.ExclusiveBetween(D2, D1, D3, Name).IsSuccess);
		Assert.Equal(Between(Name, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, D1, D3, D1), Result.ExclusiveBetween(D1, D1, D3, Name).Error);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, D1, D3, D3), Result.ExclusiveBetween(D3, D1, D3, Path, Name).Error);
		Assert.True(Result.Success(D2).ExclusiveBetween(D1, D3, Name).IsSuccess);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, D1, D3, D1), Result.Success(D1).ExclusiveBetween(D1, D3, Path, Name).Error);
	}

	[Fact]
	public void ExclusiveBetween_TimeOnly()
	{
		Assert.True(Result.ExclusiveBetween(T2, T1, T3, Name).IsSuccess);
		Assert.Equal(Between(Name, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, T1, T3, T1), Result.ExclusiveBetween(T1, T1, T3, Name).Error);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, T1, T3, T3), Result.ExclusiveBetween(T3, T1, T3, Path, Name).Error);
		Assert.True(Result.Success(T2).ExclusiveBetween(T1, T3, Name).IsSuccess);
		Assert.Equal(Between(FullName, ExpectedValidationErrorCodes.ExclusiveBetween, ExpectedValidatorErrorStrings.ExclusiveBetween, T1, T3, T1), Result.Success(T1).ExclusiveBetween(T1, T3, Path, Name).Error);
	}

	[Fact]
	public void NotNull_DateOnly()
	{
		Assert.Equal(D1, Result.NotNull((DateOnly?)D1, Name).Value);
		Assert.Equal(NoArgs(Name, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.NotNull((DateOnly?)null, Name).Error);
		Assert.Equal(NoArgs(FullName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.NotNull((DateOnly?)null, Path, Name).Error);
		Assert.Equal(D1, Result.Success<DateOnly?>(D1).NotNull(Name).Value);
		Assert.Equal(NoArgs(FullName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.Success<DateOnly?>(null).NotNull(Path, Name).Error);
	}

	[Fact]
	public void NotNull_TimeOnly()
	{
		Assert.Equal(T1, Result.NotNull((TimeOnly?)T1, Name).Value);
		Assert.Equal(NoArgs(Name, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.NotNull((TimeOnly?)null, Name).Error);
		Assert.Equal(NoArgs(FullName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.NotNull((TimeOnly?)null, Path, Name).Error);
		Assert.Equal(T1, Result.Success<TimeOnly?>(T1).NotNull(Name).Value);
		Assert.Equal(NoArgs(FullName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.Success<TimeOnly?>(null).NotNull(Path, Name).Error);
	}

	[Fact]
	public void NotNullOrEmpty_TimeOnly()
	{
		Assert.Equal(T1, Result.NotNullOrEmpty((TimeOnly?)T1, Name).Value);
		Assert.Equal(NoArgs(Name, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.NotNullOrEmpty((TimeOnly?)null, Name).Error);
		Assert.Equal(NoArgs(FullName, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty), Result.NotNullOrEmpty((TimeOnly?)default(TimeOnly), Path, Name).Error);
		Assert.Equal(T1, Result.Success<TimeOnly?>(T1).NotNullOrEmpty(Name).Value);
		Assert.Equal(NoArgs(FullName, ExpectedValidationErrorCodes.NotNull, ExpectedValidatorErrorStrings.NotNull), Result.Success<TimeOnly?>(null).NotNullOrEmpty(Path, Name).Error);
		Assert.Equal(NoArgs(Name, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty), Result.Success<TimeOnly?>(default(TimeOnly)).NotNullOrEmpty(Name).Error);
	}
}
