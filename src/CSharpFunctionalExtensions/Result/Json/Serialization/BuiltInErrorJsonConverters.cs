#nullable enable
using System.Net;
using System.Text.Json;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

/// <summary>
///     Reads and writes <see cref="RequestError"/> using the discriminator <c>"RequestError"</c>.
/// </summary>
internal sealed class RequestErrorJsonConverter() : ErrorJsonConverter<RequestError>("RequestError")
{
    private const string FormatName = "Format";
    private const string ArgumentsName = "Arguments";
    private const string TargetName = "Target";
    private const string RetryAfterName = "RetryAfter";

    public override RequestError Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        string? code = null, format = null, target = null;
        HttpStatusCode http = HttpStatusCode.InternalServerError;
        Grpc.Core.StatusCode grpc = Grpc.Core.StatusCode.Unknown;
        ErrorArgument[] arguments = [];
        TimeSpan? retryAfter = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            ErrorJson.Expect(ref reader, JsonTokenType.PropertyName);
            if (ErrorJson.IsProperty(ref reader, ErrorJson.CodePropertyName)) { reader.Read(); code = ErrorJson.ReadString(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, ErrorJson.HttpStatusCodePropertyName)) { reader.Read(); http = (HttpStatusCode)reader.GetInt32(); }
            else if (ErrorJson.IsProperty(ref reader, ErrorJson.GrpcStatusCodePropertyName)) { reader.Read(); grpc = (Grpc.Core.StatusCode)reader.GetInt32(); }
            else if (ErrorJson.IsProperty(ref reader, FormatName)) { reader.Read(); format = ErrorJson.ReadString(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, ArgumentsName)) { reader.Read(); arguments = ErrorJson.ReadArguments(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, TargetName)) { reader.Read(); target = ErrorJson.ReadString(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, RetryAfterName))
            {
                reader.Read();
                retryAfter = reader.TokenType == JsonTokenType.Null ? null : TimeSpan.FromSeconds(reader.GetInt64());
            }
            else { reader.Read(); reader.Skip(); }
        }

        return new RequestError(http, grpc, code ?? throw new JsonException("RequestError is missing its code."),
            format, arguments, target, retryAfter);
    }

    public override void WriteProperties(Utf8JsonWriter writer, RequestError error, JsonSerializerOptions options)
    {
        writer.WriteString(ErrorJson.Name(options, FormatName), error.Format);
        ErrorJson.WriteArguments(writer, ArgumentsName, error.Arguments, options);
        if (error.Target is not null)
        {
            writer.WriteString(ErrorJson.Name(options, TargetName), error.Target);
        }
        if (error.RetryAfter is { } retryAfter)
        {
            writer.WriteNumber(ErrorJson.Name(options, RetryAfterName), (long)Math.Ceiling(retryAfter.TotalSeconds));
        }
    }
}

/// <summary>
///     Reads and writes <see cref="ValidationError"/> using the discriminator <c>"ValidationError"</c>.
/// </summary>
internal sealed class ValidationErrorJsonConverter() : ErrorJsonConverter<ValidationError>("ValidationError")
{
    private const string EntriesName = "Entries";
    private const string SeverityName = "Severity";
    private const string TargetName = "Target";
    private const string FormatName = "Format";
    private const string ArgumentsName = "Arguments";

    public override ValidationError Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        List<ValidationErrorEntry>? entries = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            ErrorJson.Expect(ref reader, JsonTokenType.PropertyName);
            if (ErrorJson.IsProperty(ref reader, EntriesName))
            {
                reader.Read();
                ErrorJson.Expect(ref reader, JsonTokenType.StartArray);
                entries = [];
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    entries.Add(ReadEntry(ref reader));
                }
            }
            else { reader.Read(); reader.Skip(); }
        }

        return new ValidationError(entries ?? throw new JsonException("ValidationError is missing its entries."));
    }

    public override void WriteProperties(Utf8JsonWriter writer, ValidationError error, JsonSerializerOptions options)
    {
        writer.WriteStartArray(ErrorJson.Name(options, EntriesName));
        foreach (var entry in error.Entries)
        {
            writer.WriteStartObject();
            writer.WriteString(ErrorJson.Name(options, SeverityName), entry.GetSeverityName());
            writer.WriteString(ErrorJson.Name(options, TargetName), entry.Target);
            writer.WriteString(ErrorJson.Name(options, ErrorJson.CodePropertyName), entry.Code);
            writer.WriteString(ErrorJson.Name(options, FormatName), entry.Format);
            ErrorJson.WriteArguments(writer, ArgumentsName, entry.Arguments, options);
            writer.WriteString(ErrorJson.Name(options, ErrorJson.MessagePropertyName), entry.GetFormattedMessage());
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static ValidationErrorEntry ReadEntry(ref Utf8JsonReader reader)
    {
        ErrorJson.Expect(ref reader, JsonTokenType.StartObject);

        var severity = ValidationSeverity.Error;
        string? target = null, code = null, format = null;
        ErrorArgument[] arguments = [];

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            ErrorJson.Expect(ref reader, JsonTokenType.PropertyName);
            if (ErrorJson.IsProperty(ref reader, SeverityName))
            {
                reader.Read();
                severity = Enum.TryParse<ValidationSeverity>(reader.GetString(), true, out var s)
                    ? s
                    : throw new JsonException($"Unknown validation severity '{reader.GetString()}'.");
            }
            else if (ErrorJson.IsProperty(ref reader, TargetName)) { reader.Read(); target = ErrorJson.ReadString(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, ErrorJson.CodePropertyName)) { reader.Read(); code = ErrorJson.ReadString(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, FormatName)) { reader.Read(); format = ErrorJson.ReadString(ref reader); }
            else if (ErrorJson.IsProperty(ref reader, ArgumentsName)) { reader.Read(); arguments = ErrorJson.ReadArguments(ref reader); }
            else { reader.Read(); reader.Skip(); }
        }

        return new ValidationErrorEntry(severity, target ?? string.Empty,
            code ?? throw new JsonException("Validation entry is missing its code."), format, arguments);
    }
}
