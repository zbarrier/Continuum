using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies the string to KurrentDB position conversions parse valid input and fall back to the default position,
///     rather than throwing, for anything malformed or out of range.
/// </summary>
public class KurrentDBExtensionsTests
{
    private const string Overflow = "18446744073709551616";

    [Theory]
    [InlineData("C:0/P:0", 0UL, 0UL)]
    [InlineData("C:1234/P:1200", 1234UL, 1200UL)]
    [InlineData("C:18446744073709551615/P:18446744073709551615", ulong.MaxValue, ulong.MaxValue)]
    public void ToPosition_Parses_a_valid_position(string source, ulong commitPosition, ulong preparePosition)
    {
        Assert.Equal(new Position(commitPosition, preparePosition), source.ToPosition());

        Assert.True(source.TryToPosition(out var position));
        Assert.Equal(new Position(commitPosition, preparePosition), position);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("C:1234")]
    [InlineData("C:abc/P:1")]
    [InlineData("C:-1/P:1")]
    [InlineData("C:" + Overflow + "/P:0")]
    [InlineData("C:0/P:" + Overflow)]
    public void ToPosition_Falls_back_to_the_default_for_invalid_input(string source)
    {
        Assert.Equal(default, source.ToPosition());

        Assert.False(source.TryToPosition(out var position));
        Assert.Equal(default, position);
    }

    [Theory]
    [InlineData("0", 0UL)]
    [InlineData("42", 42UL)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    public void ToStreamPosition_Parses_a_valid_position(string source, ulong expected)
    {
        Assert.Equal(new StreamPosition(expected), source.ToStreamPosition());

        Assert.True(source.TryToStreamPosition(out var position));
        Assert.Equal(new StreamPosition(expected), position);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData(Overflow)]
    public void ToStreamPosition_Falls_back_to_the_default_for_invalid_input(string source)
    {
        Assert.Equal(default, source.ToStreamPosition());

        Assert.False(source.TryToStreamPosition(out var position));
        Assert.Equal(default, position);
    }
}
