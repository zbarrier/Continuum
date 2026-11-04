using System.Text.Json;

namespace Continuum.Domain.Orleans.Tests;

public class SessionSaveResponseConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new StreamTransactionResponseConverter(new TestTypeMapper()) } };

    [Fact]
    public void RoundTrip_PreservesChangesAndVersion()
    {
        var original = new StreamTransactionResponse([new Change("CounterIncremented", new CounterIncremented(7))], 3);
        var json = JsonSerializer.Serialize(original, Options);
        var result = JsonSerializer.Deserialize<StreamTransactionResponse>(json, Options)!;
        Assert.Equal(3, result.Version);
        var change = Assert.Single(result.Changes);
        Assert.Equal("CounterIncremented", change.EventType);
        Assert.Equal(new CounterIncremented(7), change.Event);
    }

    [Fact]
    public void RoundTrip_EmptyChanges()
    {
        var json = JsonSerializer.Serialize(new StreamTransactionResponse([], 0), Options);
        var result = JsonSerializer.Deserialize<StreamTransactionResponse>(json, Options)!;
        Assert.Empty(result.Changes);
        Assert.Equal(0, result.Version);
    }

    [Fact]
    public void Read_UnmappedEventType_Throws()
    {
        const string json = "{\"Changes\":[{\"EventType\":\"Unknown\",\"Event\":{}}],\"Version\":1}";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StreamTransactionResponse>(json, Options));
    }
}
