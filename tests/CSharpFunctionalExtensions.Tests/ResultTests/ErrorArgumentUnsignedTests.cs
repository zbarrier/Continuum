using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests;

public class ErrorArgumentUnsignedTests
{
	[Fact]
	public void UShort_ConvertsToInt64()
	{
		ErrorArgument argument = (ushort)65_535;

		Assert.Equal(ErrorArgumentKind.Int64, argument.Kind);
		Assert.Equal(65_535L, argument.Value);
	}

	[Fact]
	public void ULong_WithinInt64Range_ConvertsToInt64()
	{
		ErrorArgument argument = (ulong)long.MaxValue;

		Assert.Equal(ErrorArgumentKind.Int64, argument.Kind);
		Assert.Equal(long.MaxValue, argument.Value);
	}

	[Fact]
	public void ULong_AboveInt64Range_ConvertsToBigInteger()
	{
		ErrorArgument argument = ulong.MaxValue;

		Assert.Equal(ErrorArgumentKind.BigInteger, argument.Kind);
		Assert.Equal(new BigInteger(ulong.MaxValue), argument.Value);
		Assert.Equal(argument, ErrorArgument.FromObject(ulong.MaxValue));
	}
}
