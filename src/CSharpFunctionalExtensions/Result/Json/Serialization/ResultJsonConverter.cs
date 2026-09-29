#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

/// <summary>
///     Reads and writes <see cref="Result"/> as <c>{ "IsSuccess": true }</c> or
///     <c>{ "IsSuccess": false, "ErrorType": "...", "Error": { ... } }</c>.
/// </summary>
/// <remarks>Trimming and Native AOT safe. Error types are resolved through <see cref="ErrorJsonTypeRegistry"/>.</remarks>
public sealed class ResultJsonConverter : JsonConverter<Result>
{
    /// <inheritdoc/>
    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (ResultJsonShared.ReadIsSuccess(ref reader))
            {
                ResultJsonShared.ReadEndObject(ref reader);
                return Result.Success();
            }

            return Result.Failure(ResultJsonShared.ReadFailure(ref reader, options));
        }
        catch (JsonException)
        {
            return Result.Failure(DtoMessages.ContentJsonNotResult);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean(ResultJsonShared.IsSuccessName, value.IsSuccess);
        if (value.IsFailure)
        {
            ResultJsonShared.WriteFailure(writer, value.Error, options);
        }
        writer.WriteEndObject();
    }
}

/// <summary>
///     Reads and writes <see cref="Result{T}"/> as <c>{ "IsSuccess": true, "Value": ... }</c> or
///     <c>{ "IsSuccess": false, "ErrorType": "...", "Error": { ... } }</c>.
/// </summary>
/// <remarks>
///     Trimming and Native AOT safe when <typeparamref name="T"/> metadata is available from the options'
///     <see cref="JsonSerializerOptions.TypeInfoResolver"/> (for example a source-generated <see cref="JsonSerializerContext"/>).
///     For Native AOT, add one instance per <typeparamref name="T"/> to <see cref="JsonSerializerOptions.Converters"/> instead of
///     relying on the reflection-based factory.
/// </remarks>
/// <typeparam name="T">The success value type.</typeparam>
public sealed class ResultJsonConverter<T> : JsonConverter<Result<T>>
{
    /// <inheritdoc/>
    public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            if (ResultJsonShared.ReadIsSuccess(ref reader))
            {
                ResultJsonShared.ReadPropertyName(ref reader, ResultJsonShared.ValueName);
                reader.Read();
                T? value = JsonSerializer.Deserialize(ref reader, GetTypeInfo(options));
                ResultJsonShared.ReadEndObject(ref reader);

                return value is not null
                    ? Result.Success(value)
                    : Result.Failure<T>(DtoMessages.ContentJsonIsSuccessfulResultWithoutValue);
            }

            return Result.Failure<T>(ResultJsonShared.ReadFailure(ref reader, options));
        }
        catch (JsonException)
        {
            return Result.Failure<T>(DtoMessages.ContentJsonNotResult);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Result<T> result, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean(ResultJsonShared.IsSuccessName, result.IsSuccess);
        if (result.IsSuccess)
        {
            writer.WritePropertyName(ResultJsonShared.ValueName);
            JsonSerializer.Serialize(writer, result.Value, GetTypeInfo(options));
        }
        else
        {
            ResultJsonShared.WriteFailure(writer, result.Error, options);
        }
        writer.WriteEndObject();
    }

    private static JsonTypeInfo<T> GetTypeInfo(JsonSerializerOptions options) =>
        (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
}

/// <summary>
///     Creates <see cref="ResultJsonConverter{T}"/> instances for any <see cref="Result{T}"/>.
/// </summary>
/// <remarks>
///     Uses <see cref="Type.MakeGenericType"/>, which is not Native AOT compatible. For Native AOT, register
///     <see cref="ResultJsonConverter{T}"/> for each value type instead.
/// </remarks>
[RequiresDynamicCode("Creates ResultJsonConverter<T> with MakeGenericType. For Native AOT, register ResultJsonConverter<T> per value type.")]
internal sealed class ResultOfTJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(ResultJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;
}
