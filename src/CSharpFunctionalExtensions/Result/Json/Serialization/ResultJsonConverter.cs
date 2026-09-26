#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

internal class ResultJsonConverter : JsonConverter<Result>
{
    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token.");
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "IsSuccess")
            {
                throw new JsonException("Expected PropertyName token of 'IsSuccess'.");
            }
            if (!reader.Read() || (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False))
            {
                throw new JsonException("Expected True or False token.");
            }

            bool isSuccess = reader.GetBoolean();
            if (isSuccess)
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected EndObject token.");
                }
                return Result.Success();
            }
            else
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "ErrorTypeDiscriminator")
                {
                    throw new JsonException("Expected PropertyName token of 'ErrorTypeDiscriminator'.");
                }
                if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                {
                    throw new JsonException("Expected Number token.");
                }

                ResultErrorTypeDiscriminator errorTypeDiscriminator = (ResultErrorTypeDiscriminator)reader.GetInt32();

                if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Error")
                {
                    throw new JsonException("Expected PropertyName token of 'Error'.");
                }
                if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token.");
                }

                Error? error = errorTypeDiscriminator switch
                {
                    ResultErrorTypeDiscriminator.RequestError => JsonSerializer.Deserialize<RequestError>(ref reader, options),
                    ResultErrorTypeDiscriminator.ValidationError => JsonSerializer.Deserialize<ValidationError>(ref reader, options),
                    _ => throw new NotSupportedException("The derived error type is not supported. You will need to create a custom ResultJsonConverter. Use the ResultJsonConverter as an example.")
                };

                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected EndObject token.");
                }

                return error is not null
                    ? Result.Failure(error)
                    : Result.Failure(DtoMessages.ContentJsonIsFailedResultWithoutError);
            }
        }
        catch (JsonException)
        {
            return Result.Failure(DtoMessages.ContentJsonNotResult);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteBoolean("IsSuccess", value.IsSuccess);

        bool isError = !value.IsSuccess;
        if (isError)
        {
            if (value.Error is RequestError requestError)
            {
                writer.WriteNumber("ErrorTypeDiscriminator", (int)ResultErrorTypeDiscriminator.RequestError);
                writer.WritePropertyName("Error");
                JsonSerializer.Serialize(writer, requestError, options);
            }
            else if (value.Error is ValidationError validationError)
            {
                writer.WriteNumber("ErrorTypeDiscriminator", (int)ResultErrorTypeDiscriminator.ValidationError);
                writer.WritePropertyName("Error");
                JsonSerializer.Serialize(writer, validationError, options);
            }
            else
            {
                throw new NotSupportedException("The derived error type is not supported. You will need to create a custom ResultJsonConverter. Use the ResultJsonConverter as an example.");  
            }
        }

        writer.WriteEndObject();
    }
}
