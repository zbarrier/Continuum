using System.Globalization;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Serialization;

namespace Continuum.Orleans.Tests;

public sealed class BigIntegerSerializationTests
{
    private static readonly Serializer Serializer = new ServiceCollection()
        .AddSerializer()
        .BuildServiceProvider()
        .GetRequiredService<Serializer>();

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("9223372036854775807")]
    [InlineData("-9223372036854775809")]
    [InlineData("123456789012345678901234567890123456789012345678901234567890")]
    [InlineData("-987654321098765432109876543210987654321098765432109876543210")]
    public void BigInteger_RoundTrips_with_builtin_codec(string text)
    {
        var value = BigInteger.Parse(text, CultureInfo.InvariantCulture);

        Assert.Equal(value, Serializer.Deserialize<BigInteger>(Serializer.SerializeToArray(value)));
    }
}
