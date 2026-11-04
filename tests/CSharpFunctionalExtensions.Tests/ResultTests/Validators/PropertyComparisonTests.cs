namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class PropertyComparisonTests
{
	static readonly PropertyPath Path = PropertyPath.Root.Append("Transfers").Index(0);

	static ValidationError Error(string code, string message)
		=> new("Transfers[0].From", code, message, "Transfers[0].From", "Transfers[0].To");

	[Fact]
	public void Equal_Equal_Succeeds() => Assert.Equal(1, Result.Equal(1, 1, Path, "From", "To").Value);

	[Fact]
	public void Equal_NotEqual_FailsWithBothPropertyNames()
		=> Assert.Equal(Error(ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal), Result.Equal(1, 2, Path, "From", "To").Error);

	[Fact]
	public void NotEqual_NotEqual_Succeeds()
	{
		var value = Guid.NewGuid();
		Assert.Equal(value, Result.NotEqual(value, Guid.NewGuid(), Path, "From", "To").Value);
	}

	[Fact]
	public void NotEqual_Equal_FailsWithBothPropertyNames()
	{
		var value = Guid.NewGuid();
		Assert.Equal(Error(ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual), Result.NotEqual(value, value, Path, "From", "To").Error);
	}

	[Theory]
	[InlineData(2, 1, true)]
	[InlineData(1, 1, false)]
	public void GreaterThan_ComparesProperties(int value, int other, bool success)
		=> AssertOutcome(Result.GreaterThan(value, other, Path, "From", "To"), success, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan);

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(0, 1, false)]
	public void GreaterThanOrEqual_ComparesProperties(int value, int other, bool success)
		=> AssertOutcome(Result.GreaterThanOrEqual(value, other, Path, "From", "To"), success, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual);

	[Theory]
	[InlineData(0, 1, true)]
	[InlineData(1, 1, false)]
	public void LessThan_ComparesProperties(int value, int other, bool success)
		=> AssertOutcome(Result.LessThan(value, other, Path, "From", "To"), success, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan);

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(2, 1, false)]
	public void LessThanOrEqual_ComparesProperties(int value, int other, bool success)
		=> AssertOutcome(Result.LessThanOrEqual(value, other, Path, "From", "To"), success, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual);

	static readonly PropertyPath OtherPath = PropertyPath.Root.Append("Contract");

	static ValidationError CrossPathError(string code, string message)
		=> new("Transfers[0].From", code, message, "Transfers[0].From", "Contract.End");

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equal_CrossPath_ComparesProperties(int value, int other, bool success)
		=> AssertCrossPathOutcome(Result.Equal(value, other, Path, "From", OtherPath, "End"), success, ExpectedValidationErrorCodes.Equal, ExpectedValidatorErrorStrings.Equal);

	[Theory]
	[InlineData(1, 2, true)]
	[InlineData(1, 1, false)]
	public void NotEqual_CrossPath_ComparesProperties(int value, int other, bool success)
		=> AssertCrossPathOutcome(Result.NotEqual(value, other, Path, "From", OtherPath, "End"), success, ExpectedValidationErrorCodes.NotEqual, ExpectedValidatorErrorStrings.NotEqual);

	[Theory]
	[InlineData(2, 1, true)]
	[InlineData(1, 1, false)]
	public void GreaterThan_CrossPath_ComparesProperties(int value, int other, bool success)
		=> AssertCrossPathOutcome(Result.GreaterThan(value, other, Path, "From", OtherPath, "End"), success, ExpectedValidationErrorCodes.GreaterThan, ExpectedValidatorErrorStrings.GreaterThan);

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(0, 1, false)]
	public void GreaterThanOrEqual_CrossPath_ComparesProperties(int value, int other, bool success)
		=> AssertCrossPathOutcome(Result.GreaterThanOrEqual(value, other, Path, "From", OtherPath, "End"), success, ExpectedValidationErrorCodes.GreaterThanOrEqual, ExpectedValidatorErrorStrings.GreaterThanOrEqual);

	[Theory]
	[InlineData(0, 1, true)]
	[InlineData(1, 1, false)]
	public void LessThan_CrossPath_ComparesProperties(int value, int other, bool success)
		=> AssertCrossPathOutcome(Result.LessThan(value, other, Path, "From", OtherPath, "End"), success, ExpectedValidationErrorCodes.LessThan, ExpectedValidatorErrorStrings.LessThan);

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(2, 1, false)]
	public void LessThanOrEqual_CrossPath_ComparesProperties(int value, int other, bool success)
		=> AssertCrossPathOutcome(Result.LessThanOrEqual(value, other, Path, "From", OtherPath, "End"), success, ExpectedValidationErrorCodes.LessThanOrEqual, ExpectedValidatorErrorStrings.LessThanOrEqual);

	static void AssertCrossPathOutcome(Result<int> result, bool success, string code, string message)
	{
		Assert.Equal(success, result.IsSuccess);
		if (!success)
		{
			Assert.Equal(CrossPathError(code, message), result.Error);
		}
	}

	static void AssertOutcome(Result<int> result, bool success, string code, string message)
	{
		Assert.Equal(success, result.IsSuccess);
		if (!success)
		{
			Assert.Equal(Error(code, message), result.Error);
		}
	}
}
