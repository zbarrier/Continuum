#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

internal class ResultOfTJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }
        return typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type wrappedType = typeToConvert.GetGenericArguments()[0];

        var genericResultType = typeof(ResultOfTJsonConverter<>).MakeGenericType(wrappedType);
        JsonConverter? converter = Activator.CreateInstance(genericResultType) as JsonConverter;

        return converter!;
    }
}

internal class ResultOfTJsonConverter<T> : JsonConverter<Result<T>>
{
    public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Value")
                {
                    throw new JsonException("Expected PropertyName token of 'Value'.");
                }
                if (!reader.Read())
                {
                    throw new JsonException("Expected StartObject token or Value.");
                }

                T? value = JsonSerializer.Deserialize<T>(ref reader, options);

                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected EndObject token.");
                }

                return value is not null
                    ? Result.Success(value)
                    : Result.Failure<T>(DtoMessages.ContentJsonIsSuccessfulResultWithoutValue);
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
                    ResultErrorTypeDiscriminator.DomainError => JsonSerializer.Deserialize<DomainError>(ref reader, options),
                    ResultErrorTypeDiscriminator.RequestError => JsonSerializer.Deserialize<RequestError>(ref reader, options),
                    ResultErrorTypeDiscriminator.ValidationError => JsonSerializer.Deserialize<ValidationError>(ref reader, options),
                    _ => throw new NotSupportedException("The derived error type is not supported. You will need to create a custom ResultJsonConverter. Use the ResultJsonConverter as an example.")
                };

                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected EndObject token.");
                }

                return error is not null
                    ? Result.Failure<T>(error)
                    : Result.Failure<T>(DtoMessages.ContentJsonIsFailedResultWithoutError);
            }
        }
        catch (JsonException)
        {
            return Result.Failure<T>(DtoMessages.ContentJsonNotResult);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<T> result, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("IsSuccess", result.IsSuccess);
   
        if (result.IsSuccess)
        {
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, result.Value, options);
        }
        else
        {
            if (result.Error is DomainError domainError)
            {
                writer.WriteNumber("ErrorTypeDiscriminator", (int)ResultErrorTypeDiscriminator.DomainError);
                writer.WritePropertyName("Error");
                JsonSerializer.Serialize(writer, domainError, options);
            }
            else if (result.Error is RequestError requestError)
            {
                writer.WriteNumber("ErrorTypeDiscriminator", (int)ResultErrorTypeDiscriminator.RequestError);
                writer.WritePropertyName("Error");
                JsonSerializer.Serialize(writer, requestError, options);
            }
            else if (result.Error is ValidationError validationError)
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
