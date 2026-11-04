using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class DateTimeParseValidatorTests
{
	const string Name = "MyProperty";
	static readonly PropertyPath Path = PropertyPath.Root.Append("Parent");
	const string FullName = "Parent.MyProperty";

	static ValidationError InvalidFormat(string name)
		=> new(name, ExpectedValidationErrorCodes.InvalidFormat, ExpectedValidatorErrorStrings.InvalidFormat, name);

	static ValidationError NotEmpty(string name)
		=> new(name, ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, name);

	[Fact]
	public void IsDateTimeOffset_ValidValue_Success()
	{
		var expected = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.FromHours(-5));

		Assert.Equal(expected, Result.IsDateTimeOffset("2024-01-15T10:30:00-05:00", Name).Value);
		Assert.Equal(expected, Result.Success("2024-01-15T10:30:00-05:00").IsDateTimeOffset(Path, Name).Value);
	}

	[Fact]
	public void IsDateTimeOffset_NoOffset_AssumesUtc()
	{
		var result = Result.IsDateTimeOffset("2024-01-15T10:30:00", Name);

		Assert.Equal(new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero), result.Value);
	}

	[Fact]
	public void IsDateTimeOffset_InvalidValue_InvalidFormatError()
	{
		Assert.Equal(InvalidFormat(Name), Result.IsDateTimeOffset("not a date", Name).Error);
		Assert.Equal(InvalidFormat(FullName), Result.IsDateTimeOffset("not a date", Path, Name).Error);
		Assert.Equal(InvalidFormat(FullName), Result.Success("not a date").IsDateTimeOffset(Path, Name).Error);
		Assert.Equal(NotEmpty(Name), Result.IsDateTimeOffset(null, Name).Error);
	}

	[Fact]
	public void IsTimeOnly_ValidValue_Success()
	{
		Assert.Equal(new TimeOnly(14, 30), Result.IsTimeOnly("14:30", Name).Value);
		Assert.Equal(new TimeOnly(14, 30, 15), Result.Success("14:30:15").IsTimeOnly(Path, Name).Value);
	}

	[Fact]
	public void IsTimeOnly_InvalidValue_InvalidFormatError()
	{
		Assert.Equal(InvalidFormat(Name), Result.IsTimeOnly("25:00", Name).Error);
		Assert.Equal(InvalidFormat(FullName), Result.IsTimeOnly("not a time", Path, Name).Error);
		Assert.Equal(InvalidFormat(FullName), Result.Success("not a time").IsTimeOnly(Path, Name).Error);
		Assert.Equal(NotEmpty(Name), Result.IsTimeOnly(null, Name).Error);
	}

	[Fact]
	public void IsTimeSpan_ValidValue_Success()
	{
		Assert.Equal(new TimeSpan(1, 2, 3, 4), Result.IsTimeSpan("1.02:03:04", Name).Value);
		Assert.Equal(TimeSpan.FromMinutes(90), Result.Success("01:30:00").IsTimeSpan(Path, Name).Value);
	}

	[Fact]
	public void IsTimeSpan_InvalidValue_InvalidFormatError()
	{
		Assert.Equal(InvalidFormat(Name), Result.IsTimeSpan("not a duration", Name).Error);
		Assert.Equal(InvalidFormat(FullName), Result.IsTimeSpan("not a duration", Path, Name).Error);
		Assert.Equal(InvalidFormat(FullName), Result.Success("not a duration").IsTimeSpan(Path, Name).Error);
		Assert.Equal(NotEmpty(Name), Result.IsTimeSpan(null, Name).Error);
	}

	[Fact]
	public void IsDateTime_UtcDesignator_PreservesUtcKind()
	{
		var result = Result.IsDateTime("2024-01-15T10:30:00Z", Name);

		Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
		Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc), result.Value);
	}

	[Fact]
	public void Parsers_IgnoreCurrentCulture()
	{
		var original = CultureInfo.CurrentCulture;
		try
		{
			// de-DE uses dd.MM.yyyy and ',' as the decimal separator; parsing must still be invariant.
			CultureInfo.CurrentCulture = new CultureInfo("de-DE");

			Assert.Equal(new DateTime(2024, 1, 2), Result.IsDateTime("01/02/2024", Name).Value);
			Assert.Equal(new DateOnly(2024, 1, 2), Result.IsDateOnly("01/02/2024", Name).Value);
			Assert.Equal(new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero), Result.IsDateTimeOffset("01/02/2024", Name).Value);
			Assert.Equal(new TimeOnly(14, 30), Result.IsTimeOnly("2:30 PM", Name).Value);
			Assert.Equal(new TimeSpan(0, 0, 0, 1, 500), Result.IsTimeSpan("00:00:01.5", Name).Value);
		}
		finally
		{
			CultureInfo.CurrentCulture = original;
		}
	}
}
