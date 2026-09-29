#nullable enable
using System.Text.Json;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

/// <summary>
///     Shared read/write logic for the failure part of a serialized result:
///     <c>"ErrorType": "&lt;discriminator&gt;", "Error": { ... }</c>.
/// </summary>
internal static class ResultJsonShared
{
    public const string IsSuccessName = "IsSuccess";
    public const string ValueName = "Value";
    public const string ErrorTypeName = "ErrorType";
    public const string ErrorName = "Error";

    public static void ReadPropertyName(ref Utf8JsonReader reader, string name)
    {
        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != name)
        {
            throw new JsonException($"Expected PropertyName token of '{name}'.");
        }
    }

    public static bool ReadIsSuccess(ref Utf8JsonReader reader)
    {
        ErrorJson.Expect(ref reader, JsonTokenType.StartObject);
        ReadPropertyName(ref reader, IsSuccessName);
        if (!reader.Read() || (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False))
        {
            throw new JsonException("Expected True or False token.");
        }
        return reader.GetBoolean();
    }

    public static Error ReadFailure(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        ReadPropertyName(ref reader, ErrorTypeName);
        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected String token.");
        }
        var discriminator = reader.GetString()!;

        ReadPropertyName(ref reader, ErrorName);
        reader.Read();
        var error = ErrorJson.ReadError(ref reader, discriminator, options);

        ReadEndObject(ref reader);
        return error;
    }

    public static void ReadEndObject(ref Utf8JsonReader reader)
    {
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Expected EndObject token.");
        }
    }

    public static void WriteFailure(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
    {
        writer.WriteString(ErrorTypeName, ErrorJson.GetTypeDiscriminator(error));
        writer.WritePropertyName(ErrorName);
        ErrorJson.WriteError(writer, error, options);
    }
}
