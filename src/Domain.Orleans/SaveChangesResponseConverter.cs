using System.Text.Json;
using System.Text.Json.Serialization;

using Continuum.TypeMapping;

namespace Continuum.Domain.Orleans;

/// <summary>JSON converter for <see cref="SaveChangesResponse"/> that resolves event types through an <see cref="ITypeMapper"/>.</summary>
public sealed class SaveChangesResponseConverter : JsonConverter<SaveChangesResponse>
{
    private readonly ITypeMapper _typeMapper;

    /// <summary>Initializes a new instance of the <see cref="SaveChangesResponseConverter"/> class.</summary>
    /// <param name="typeMapper">The type mapper used to resolve event types by name.</param>
    public SaveChangesResponseConverter(ITypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    /// <inheritdoc/>
    public override SaveChangesResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !IsPropertyName(ref reader, "Changes"))
        {
            throw new JsonException("Expected PropertyName token of 'Changes'.");
        }
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token for 'Changes'.");
        }

        var changes = new List<IChange>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token for Change object.");
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !IsPropertyName(ref reader, "EventType"))
            {
                throw new JsonException("Expected PropertyName token of 'EventType'.");
            }

            reader.Read();
            string? eventTypeName = reader.GetString();

            if (string.IsNullOrWhiteSpace(eventTypeName))
            {
                throw new JsonException("Invalid EventType.");
            }
            if (!_typeMapper.TryGetType(eventTypeName, out var eventType))
            {
                throw new JsonException("Missing EventType in TypeMapper.");
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !IsPropertyName(ref reader, "Event"))
            {
                throw new JsonException("Expected PropertyName token of 'Event'.");
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token for Change.Event object.");
            }

            object? change = JsonSerializer.Deserialize(ref reader, options.GetTypeInfo(eventType));
            if (change is null)
            {
                throw new JsonException("Expected Change.Event object but received null.");
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("Expected EndObject token for Change object.");
            }

            changes.Add(new Change(eventTypeName, change));
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !IsPropertyName(ref reader, "Version"))
        {
            throw new JsonException("Expected PropertyName token of 'Version'.");
        }
        if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expected Number token for 'Version'.");
        }

        int version = reader.GetInt32();

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Expected EndObject token.");
        }

        return new SaveChangesResponse(changes, version);
    }

    private static bool IsPropertyName(ref Utf8JsonReader reader, string name) =>
        string.Equals(reader.GetString(), name, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SaveChangesResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Changes");
        writer.WriteStartArray();
        foreach (var change in value.Changes)
        {
            writer.WriteStartObject();
            writer.WriteString("EventType", change.EventType);
            writer.WritePropertyName("Event");
            JsonSerializer.Serialize(writer, change.Event, options.GetTypeInfo(change.Event.GetType()));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteNumber("Version", value.Version);

        writer.WriteEndObject();
    }
}
