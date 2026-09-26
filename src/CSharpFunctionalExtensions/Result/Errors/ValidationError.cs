#nullable enable

using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions;

public sealed class ValidationError : Error, IEquatable<ValidationError>
{
    private const int DefaultHttpStatusCode = (int)System.Net.HttpStatusCode.UnprocessableEntity;
    private const int DefaultGrpcStatusCode = (int)Grpc.Core.StatusCode.InvalidArgument;

    public ValidationError(string identifier, string errorMessage)
        : this(ValidationSeverity.Error, identifier, errorMessage, [])
    { }
    public ValidationError(string identifier, string errorMessage, params object[] arguments)
        : this(ValidationSeverity.Error, identifier, errorMessage, arguments)
    { }
    public ValidationError(ValidationSeverity severity, string identifier, string errorMessage, params object[] arguments)
        : base(ErrorPriorityCode.INVALID_ARGUMENT)
    {
        Entries = new() { new ValidationErrorEntry(severity, identifier, errorMessage, arguments) };
    }

    [JsonConstructor]
    public ValidationError(List<ValidationErrorEntry> entries)
        : base(ErrorPriorityCode.INVALID_ARGUMENT)
    {
        if (entries is null || entries.Count == 0)
        {
            throw new ArgumentException("Entries cannot be null or empty.", nameof(entries));
        }

        Entries = entries;
    }

    [JsonIgnore]
    public int HttpStatusCode => DefaultHttpStatusCode;
    [JsonIgnore]
    public int GrpcStatusCode => DefaultGrpcStatusCode;
    [JsonInclude]
    public List<ValidationErrorEntry> Entries { get; }

    [JsonIgnore]
    public override bool SupportsFormattedMessage => true;
    //public override string GetFormattedMessage() => throw new NotSupportedException("Validation errors do not support formatted messages.");
    public override string GetFormattedMessage()
    {
        var sb = new StringBuilder();

        var listSpan = CollectionsMarshal.AsSpan(Entries);
        ref var searchSpace = ref MemoryMarshal.GetReference(listSpan);
        for (int i = 0; i < listSpan.Length; i++)
        {
            var item = Unsafe.Add(ref searchSpace, i);
            _ = sb.AppendLine(item.GetFormattedMessage());
        }

        return sb.ToString();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        _ = sb.AppendFormat("PriorityCode: {0}, HttpStatusCode: {1}, GrpcStatusCode: {2}", PriorityCode, HttpStatusCode, GrpcStatusCode);

        if (Entries.Count > 0)
        {
            _ = sb.AppendLine();
        }

        var listSpan = CollectionsMarshal.AsSpan(Entries);
        ref var searchSpace = ref MemoryMarshal.GetReference(listSpan);
        for (int i = 0; i < listSpan.Length; i++)
        {
            var item = Unsafe.Add(ref searchSpace, i);
            _ = sb.AppendLine(item.ToString());
        }

        return sb.ToString();
    }

    #region IEquatable

    public override bool Equals(object obj) => obj is ValidationError errors && Equals(errors);
    public bool Equals(ValidationError other) => other is not null && EqualsCore(other);
    bool EqualsCore(ValidationError other) =>
        PriorityCode == other.PriorityCode &&
        HttpStatusCode == other.HttpStatusCode &&
        GrpcStatusCode == other.GrpcStatusCode &&
        Entries.SequenceEqual(other.Entries);

    public static bool operator ==(ValidationError left, ValidationError right) =>
        !(left is null ^ right is null) && (left is null || left.EqualsCore(right!));
    public static bool operator !=(ValidationError left, ValidationError right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(PriorityCode, HttpStatusCode, GrpcStatusCode, Entries);

    #endregion
}

public sealed class ValidationErrorEntry : IEquatable<ValidationErrorEntry>
{
    public ValidationErrorEntry(ValidationSeverity severity, string identifier, string format, object[] arguments)
    {
        Severity = severity;
        Identifier = identifier;
        Format = format;
        Arguments = arguments;
    }

    [JsonInclude]
    public ValidationSeverity Severity { get; }
    [JsonInclude]
    public string Identifier { get; }
    [JsonInclude]
    public string Format { get; }
    [JsonInclude]
    public object[] Arguments { get; }

    public string GetFormattedMessage() => string.Format(Format, Arguments);

    public override string ToString() => $"{GetSeverityName()}, {Identifier}, {GetFormattedMessage()}";

    #region IEquatable

    public override bool Equals(object? obj) => obj is ValidationErrorEntry entry && Equals(entry);
    public bool Equals(ValidationErrorEntry? other) => other is not null && EqualsCore(other);
    bool EqualsCore(ValidationErrorEntry other) =>
        Severity == other.Severity &&
        Identifier == other.Identifier &&
        Format == other.Format &&
        Arguments.SequenceEqual(other.Arguments);

    public static bool operator ==(ValidationErrorEntry left, ValidationErrorEntry right) =>
        !(left is null ^ right is null) && (left is null || left.EqualsCore(right!));
    public static bool operator !=(ValidationErrorEntry left, ValidationErrorEntry right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(Severity, Identifier, Format, Arguments);

    #endregion

    public string GetSeverityName()
        => Severity switch
        {
            ValidationSeverity.Error => "Error",
            ValidationSeverity.Warning => "Warning",
            ValidationSeverity.Info => "Info",
            _ => throw new ArgumentOutOfRangeException("The validation severity type is invalid.")
        };
}

public enum ValidationSeverity
{
    Error = 0,
    Warning = 1,
    Info = 2,
}