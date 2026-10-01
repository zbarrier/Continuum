#nullable enable
using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

/// <summary>
///     Helpers for writing custom <see cref="ErrorJsonConverter{TError}"/> implementations.
/// </summary>
public static class ErrorJson
{
    /// <summary>The property name of the machine-readable error code.</summary>
    public const string CodePropertyName = "Code";
    /// <summary>The property name of the HTTP status code.</summary>
    public const string HttpStatusCodePropertyName = "HttpStatusCode";
    /// <summary>The property name of the gRPC status code.</summary>
    public const string GrpcStatusCodePropertyName = "GrpcStatusCode";
    /// <summary>The property name of the output-only, localized message.</summary>
    public const string MessagePropertyName = "Message";

    private const string KindPropertyName = "Kind";
    private const string ValuePropertyName = "Value";

    /// <summary>
    ///     Gets the name to write for <paramref name="name"/>, applying <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="name">The PascalCase property name.</param>
    /// <returns>The converted name.</returns>
    public static string Name(JsonSerializerOptions options, string name) =>
        options.PropertyNamingPolicy?.ConvertName(name) ?? name;

    /// <summary>
    ///     Gets whether the current property name of <paramref name="reader"/> matches <paramref name="name"/>, ignoring case.
    /// </summary>
    /// <param name="reader">A reader positioned on a <see cref="JsonTokenType.PropertyName"/> token.</param>
    /// <param name="name">The PascalCase property name.</param>
    /// <returns><see langword="true"/> when the names match.</returns>
    public static bool IsProperty(ref Utf8JsonReader reader, string name) =>
        string.Equals(reader.GetString(), name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Reads a JSON string or null.
    /// </summary>
    /// <param name="reader">A reader positioned on the value.</param>
    /// <returns>The string, or null.</returns>
    public static string? ReadString(ref Utf8JsonReader reader) =>
        reader.TokenType == JsonTokenType.Null ? null : reader.GetString();

    /// <summary>
    ///     Writes an array of error arguments. Each argument is written as <c>{ "Kind": "...", "Value": ... }</c>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="propertyName">The PascalCase property name.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="options">The serializer options.</param>
    public static void WriteArguments(Utf8JsonWriter writer, string propertyName, ErrorArgument[] arguments, JsonSerializerOptions options)
    {
        writer.WriteStartArray(Name(options, propertyName));
        foreach (var argument in arguments)
        {
            writer.WriteStartObject();
            writer.WriteString(Name(options, KindPropertyName), argument.Kind.ToString());
            writer.WritePropertyName(Name(options, ValuePropertyName));
            WriteArgumentValue(writer, argument);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    /// <summary>
    ///     Reads an array of error arguments written by <see cref="WriteArguments"/>.
    /// </summary>
    /// <param name="reader">A reader positioned on the <see cref="JsonTokenType.StartArray"/> or null token.</param>
    /// <returns>The arguments.</returns>
    /// <exception cref="JsonException">The JSON is not a valid argument array.</exception>
    public static ErrorArgument[] ReadArguments(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }
        Expect(ref reader, JsonTokenType.StartArray);

        var arguments = new List<ErrorArgument>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            Expect(ref reader, JsonTokenType.StartObject);

            ErrorArgumentKind? kind = null;
            string? rawValue = null;
            bool isNull = true;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                Expect(ref reader, JsonTokenType.PropertyName);
                if (IsProperty(ref reader, KindPropertyName))
                {
                    reader.Read();
                    kind = Enum.TryParse<ErrorArgumentKind>(reader.GetString(), out var k)
                        ? k
                        : throw new JsonException($"Unknown error argument kind '{reader.GetString()}'.");
                }
                else if (IsProperty(ref reader, ValuePropertyName))
                {
                    reader.Read();
                    isNull = reader.TokenType == JsonTokenType.Null;
                    rawValue = reader.TokenType switch
                    {
                        JsonTokenType.Null => null,
                        JsonTokenType.String => reader.GetString(),
                        JsonTokenType.Number => GetRawNumber(ref reader),
                        JsonTokenType.True => bool.TrueString,
                        JsonTokenType.False => bool.FalseString,
                        _ => throw new JsonException("Invalid error argument value."),
                    };
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }
            }

            if (kind is null)
            {
                throw new JsonException("Error argument is missing its kind.");
            }
            arguments.Add(isNull ? ErrorArgument.Null : ParseArgument(kind.Value, rawValue!));
        }

        return [.. arguments];
    }

    private static string GetRawNumber(ref Utf8JsonReader reader)
    {
        ReadOnlySpan<byte> span = reader.HasValueSequence
            ? System.Buffers.BuffersExtensions.ToArray(reader.ValueSequence)
            : reader.ValueSpan;
        return System.Text.Encoding.UTF8.GetString(span);
    }

    internal static void Expect(ref Utf8JsonReader reader, JsonTokenType tokenType)
    {
        if (reader.TokenType != tokenType)
        {
            throw new JsonException($"Expected {tokenType} token but found {reader.TokenType}.");
        }
    }

    internal static void WriteError(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
    {
        if (!ErrorJsonTypeRegistry.TryGetConverter(error.GetType(), out var converter))
        {
            throw new NotSupportedException(
                $"The error type '{error.GetType()}' is not registered. Register it with ErrorJsonTypeRegistry.Register.");
        }

        writer.WriteStartObject();
        writer.WriteString(Name(options, CodePropertyName), error.Code);
        writer.WriteNumber(Name(options, HttpStatusCodePropertyName), (int)error.HttpStatusCode);
        writer.WriteNumber(Name(options, GrpcStatusCodePropertyName), (int)error.GrpcStatusCode);
        converter.WriteProperties(writer, error, options);
        writer.WriteString(Name(options, MessagePropertyName), error.GetFormattedMessage());
        writer.WriteEndObject();
    }

    internal static string GetTypeDiscriminator(Error error) =>
        ErrorJsonTypeRegistry.TryGetConverter(error.GetType(), out var converter)
            ? converter.TypeDiscriminator
            : throw new NotSupportedException(
                $"The error type '{error.GetType()}' is not registered. Register it with ErrorJsonTypeRegistry.Register.");

    internal static Error ReadError(ref Utf8JsonReader reader, string discriminator, JsonSerializerOptions options)
    {
        if (!ErrorJsonTypeRegistry.TryGetConverter(discriminator, out var converter))
        {
            throw new NotSupportedException(
                $"The error type discriminator '{discriminator}' is not registered. Register it with ErrorJsonTypeRegistry.Register.");
        }

        Expect(ref reader, JsonTokenType.StartObject);
        return converter.Read(ref reader, options);
    }

    private static void WriteArgumentValue(Utf8JsonWriter writer, ErrorArgument argument)
    {
        var inv = CultureInfo.InvariantCulture;
        switch (argument.Value)
        {
            case null: writer.WriteNullValue(); break;
            case string s: writer.WriteStringValue(s); break;
            case long l: writer.WriteNumberValue(l); break;
            case double d when double.IsFinite(d): writer.WriteNumberValue(d); break;
            case double d: writer.WriteStringValue(d.ToString("R", inv)); break;
            case decimal m: writer.WriteNumberValue(m); break;
            case bool b: writer.WriteBooleanValue(b); break;
            case DateTime dt: writer.WriteStringValue(dt); break;
            case DateTimeOffset dto: writer.WriteStringValue(dto); break;
            case TimeSpan ts: writer.WriteStringValue(ts.ToString("c", inv)); break;
            case Guid g: writer.WriteStringValue(g); break;
            case DateOnly d: writer.WriteStringValue(d.ToString("O", inv)); break;
            case TimeOnly t: writer.WriteStringValue(t.ToString("O", inv)); break;
            case BigInteger bi: writer.WriteStringValue(bi.ToString(inv)); break;
            case IFormattable f: writer.WriteStringValue(f.ToString(null, inv)); break;
            default: writer.WriteStringValue(argument.Value.ToString()); break;
        }
    }

    private static ErrorArgument ParseArgument(ErrorArgumentKind kind, string raw)
    {
        var inv = CultureInfo.InvariantCulture;
        return kind switch
        {
            ErrorArgumentKind.String => ErrorArgument.From(raw),
            ErrorArgumentKind.Int64 => ErrorArgument.From(long.Parse(raw, inv)),
            ErrorArgumentKind.Double => ErrorArgument.From(double.Parse(raw, NumberStyles.Float, inv)),
            ErrorArgumentKind.Decimal => ErrorArgument.From(decimal.Parse(raw, NumberStyles.Float, inv)),
            ErrorArgumentKind.Boolean => ErrorArgument.From(bool.Parse(raw)),
            ErrorArgumentKind.DateTime => ErrorArgument.From(DateTime.Parse(raw, inv, DateTimeStyles.RoundtripKind)),
            ErrorArgumentKind.DateTimeOffset => ErrorArgument.From(DateTimeOffset.Parse(raw, inv, DateTimeStyles.RoundtripKind)),
            ErrorArgumentKind.TimeSpan => ErrorArgument.From(TimeSpan.ParseExact(raw, "c", inv)),
            ErrorArgumentKind.Guid => ErrorArgument.From(Guid.Parse(raw)),
            ErrorArgumentKind.DateOnly => ErrorArgument.From(DateOnly.ParseExact(raw, "O", inv)),
            ErrorArgumentKind.TimeOnly => ErrorArgument.From(TimeOnly.ParseExact(raw, "O", inv)),
            ErrorArgumentKind.BigInteger => ErrorArgument.From(BigInteger.Parse(raw, inv)),
            _ => ErrorArgument.Null,
        };
    }
}
