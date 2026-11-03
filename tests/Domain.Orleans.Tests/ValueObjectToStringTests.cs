namespace Continuum.Domain.Orleans.Tests;

public class ValueObjectToStringTests
{
    private sealed record Quantity(int V) : Int32ValueObject<Quantity>(V);
    private sealed record Price(decimal V) : DecimalValueObject<Price>(V);
    private sealed record OccurredOn(DateTime V) : DateTimeValueObject<OccurredOn>(V);
    private sealed record BirthDate(DateOnly V) : DateOnlyValueObject<BirthDate>(V);
    private sealed record Duration(TimeSpan V) : TimeSpanValueObject<Duration>(V);
    private sealed record Moment(DateTimeOffset V) : DateTimeOffsetValueObject<Moment>(V);

    [Fact]
    public void ToString_ReturnsWrappedValue()
    {
        Assert.Equal("42", new Quantity(42).ToString());
        Assert.Equal("1.5", new Price(1.5m).ToString());
        Assert.Equal("2024-01-02T03:04:05.0000000Z", new OccurredOn(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)).ToString());
        Assert.Equal("2024-01-02", new BirthDate(new DateOnly(2024, 1, 2)).ToString());
        Assert.Equal("01:02:03", new Duration(new TimeSpan(1, 2, 3)).ToString());
        Assert.Equal("2024-01-02T03:04:05.0000000-05:00", new Moment(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(-5))).ToString());
    }

    [Fact]
    public void DateTimeOffset_ComparesInstantsAcrossOffsets()
    {
        var utc = new Moment(new DateTimeOffset(2024, 1, 2, 8, 0, 0, TimeSpan.Zero));
        var eastern = new Moment(new DateTimeOffset(2024, 1, 2, 4, 0, 0, TimeSpan.FromHours(-5)));

        Assert.True(utc < eastern);
        Assert.True(eastern >= utc);
    }

    [Fact]
    public void ImplicitStringAndDeconstruct_UseToString()
    {
        var quantity = new Quantity(7);
        string s = quantity;
        quantity.Deconstruct(out string d);

        Assert.Equal("7", s);
        Assert.Equal("7", d);
    }
}
