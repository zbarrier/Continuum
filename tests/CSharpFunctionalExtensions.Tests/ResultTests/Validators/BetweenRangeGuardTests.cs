using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Validators;

public class BetweenRangeGuardTests
{
    private sealed class MagnitudeComparer : IComparer<string>
    {
        public int Compare(string? x, string? y) => string.CompareOrdinal(x, y) * 10;
    }

    [Fact]
    public void Comparer_overload_rejects_to_below_from_regardless_of_magnitude()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Result.InclusiveBetween("b", "c", "a", new MagnitudeComparer(), "Name"));

        Assert.Equal("to", ex.ParamName);
        Assert.Throws<ArgumentOutOfRangeException>(() => Result.ExclusiveBetween("b", "c", "a", new MagnitudeComparer(), "Name"));
    }

    [Fact]
    public void Equal_bounds_are_allowed_for_every_overload()
    {
        Assert.True(Result.InclusiveBetween("a", "a", "a", new MagnitudeComparer(), "Name").IsSuccess);
        Assert.True(Result.InclusiveBetween("a", "a", "a", "Name").IsSuccess);
        Assert.True(Result.InclusiveBetween(5, 5, 5, "Name").IsSuccess);
    }

    [Fact]
    public void Numeric_overload_rejects_to_below_from()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Result.InclusiveBetween(5, 6, 4, "Name"));

        Assert.Equal("to", ex.ParamName);
        Assert.StartsWith("To should be larger than from.", ex.Message);
    }

    [Fact]
    public void String_overload_rejects_to_below_from()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Result.Success("b").InclusiveBetween("c", "a", "Name"));

        Assert.Equal("to", ex.ParamName);
    }
}
