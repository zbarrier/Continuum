using System.Globalization;
using System.Text.Json;

using Continuum.Serialization.Orleans;

namespace Continuum.Orleans.Tests;

public sealed class TimeSpanSerializationTests
{
    private static readonly JsonSerializerOptions Options = TypeMappedJsonGrainStorageSerializer.DefaultOptions;

    public static TheoryData<TimeSpan> Values => new()
    {
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(1234.5678),
        new TimeSpan(3, 4, 5, 6, 7),
        -TimeSpan.FromHours(26),
        TimeSpan.MaxValue,
        TimeSpan.MinValue,
    };

    [Theory]
    [MemberData(nameof(Values))]
    public void RoundTrips_with_builtin_handling(TimeSpan value)
    {
        var json = JsonSerializer.Serialize(value, Options);

        Assert.Equal(value, JsonSerializer.Deserialize<TimeSpan>(json, Options));
    }

    [Fact]
    public void Writes_constant_format_independent_of_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            var value = new TimeSpan(1, 2, 3, 4, 500);

            var json = JsonSerializer.Serialize(value, Options);

            Assert.Equal("\"1.02:03:04.5000000\"", json);
            Assert.Equal(value, JsonSerializer.Deserialize<TimeSpan>(json, Options));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
