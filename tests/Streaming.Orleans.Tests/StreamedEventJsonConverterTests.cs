using System.Text.Json;
using System.Text.Json.Nodes;

using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;
using Continuum.Orleans.Streams;

namespace Continuum.Streaming.Orleans.Tests;

/// <summary>
///     Verifies the single-pass reader of <see cref="StreamedEventJsonConverter" /> handles property order, unknown
///     properties and missing required properties.
/// </summary>
public class StreamedEventJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    [Fact]
    public void Reads_Payload_Written_Before_Event_Type_And_Skips_Unknown_Properties()
    {
        var json = """
            {"unknown":{"nested":[1,2]},"event":{"Text":"early"},"metadata":{"values":{"tenant":"acme"}},
             "eventId":"00000000-0000-0000-0000-000000000001","eventType":"Tests.Payload",
             "streamName":{"value":"counted-acme|1","category":"counted","tenantId":"acme","key":"1"},"streamVersion":2,
             "cursor":{"topic":"$all","partition":"0","sequenceNumber":"4","subSequenceNumber":5},"timestamp":"2024-01-02T03:04:05Z"}
            """;

        var restored = JsonSerializer.Deserialize<IStreamedEvent<object>>(json, Options)!;

        Assert.Equal("early", Assert.IsType<Payload>(restored.Event).Text);
        Assert.Equal("acme", restored.Metadata.GetValueOrDefault("tenant"));
        Assert.Equal(2UL, restored.StreamVersion);
        Assert.Equal(new StreamName("counted-acme|1", "counted", "acme", "1"), restored.StreamName);
        Assert.Equal("acme", restored.StreamName.TenantId);
        Assert.Equal(new StreamedEventCursor("$all", "0", 4, 5), restored.Cursor);
    }

    [Theory]
    [InlineData("eventId")]
    [InlineData("eventType")]
    [InlineData("event")]
    [InlineData("metadata")]
    [InlineData("streamName")]
    [InlineData("cursor")]
    [InlineData("timestamp")]
    public void Throws_When_Required_Property_Is_Missing(string missing)
    {
        var original = new StreamedEvent<object>(
            Guid.CreateVersion7(), "Tests.Payload", new StreamName("counted-1", "counted", null, "1"), 0,
            new StreamedEventCursor("$all", "0", 1, 0), DateTime.UtcNow, new Payload("payload"));
        var node = JsonNode.Parse(JsonSerializer.Serialize<IStreamedEvent<object>>(original, Options))!.AsObject();
        node.Remove(missing);

        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IStreamedEvent<object>>(node.ToJsonString(), Options));

        Assert.Contains($"'{missing}'", exception.Message);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var mapper = new DefaultTypeMapAttributeMapper();
        mapper.AddType(typeof(Payload), "Tests.Payload");
        mapper.Freeze();

        var options = new JsonSerializerOptions(TypeMappedJsonGrainStorageSerializer.DefaultOptions);
        options.Converters.Add(new StreamedEventJsonConverter(mapper));
        return options;
    }

    public sealed record Payload(string Text);
}
