using System.Text.Json;
using Continuum.Converters.Json;

namespace Continuum.Tests;

public class NewIdConverter_Specs
{
    static readonly JsonSerializerOptions Options = new() { Converters = { new NewIdConverter() } };

    [Fact]
    public void Should_write_the_sequential_guid_string()
    {
        var id = NewId.Next();

        var json = JsonSerializer.Serialize(id, Options);

        Assert.Equal($"\"{id.ToSequentialGuid()}\"", json);
    }

    [Fact]
    public void Should_round_trip()
    {
        var id = NewId.Next();

        var result = JsonSerializer.Deserialize<NewId>(JsonSerializer.Serialize(id, Options), Options);

        Assert.Equal(id, result);
    }

    [Fact]
    public void Should_read_a_sequential_guid_string()
    {
        var guid = Guid.Parse("74b719ec-7596-3cf0-7d81-bf34437f0b01");

        var result = JsonSerializer.Deserialize<NewId>($"\"{guid}\"", Options);

        Assert.Equal(NewId.FromSequentialGuid(guid), result);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"not-a-guid\"")]
    [InlineData("123")]
    public void Should_throw_JsonException_for_invalid_values(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<NewId>(json, Options));
    }
}
