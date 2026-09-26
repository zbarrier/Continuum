#nullable enable

using System.Net;
using System.Text.Json.Serialization;

using Continuum.CSharpFunctionalExtensions.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions;

public sealed class RequestError : Error, IEquatable<RequestError>
{
    public RequestError(int priorityCode, HttpStatusCode httpStatusCode, Grpc.Core.StatusCode grpcStatusCode)
        : this(priorityCode, (int)httpStatusCode, (int)grpcStatusCode, string.Empty, [])
    { }
    public RequestError(int priorityCode, HttpStatusCode httpStatusCode, Grpc.Core.StatusCode grpcStatusCode, string? format)
        : this(priorityCode, (int)httpStatusCode, (int)grpcStatusCode, format, [])
    { }
    public RequestError(int priorityCode, int httpStatusCode, int grpcStatusCode)
        : this(priorityCode, httpStatusCode, grpcStatusCode, string.Empty, [])
    { }
    public RequestError(int priorityCode, int httpStatusCode, int grpcStatusCode, string? format)
        : this(priorityCode, httpStatusCode, grpcStatusCode, format, [])
    { }
    public RequestError(int priorityCode, HttpStatusCode httpStatusCode, Grpc.Core.StatusCode grpcStatusCode, string? format, params object[] arguments)
        : this(priorityCode, (int)httpStatusCode, (int)grpcStatusCode, format, arguments)
    { }
    [JsonConstructor]
    public RequestError(int priorityCode, int httpStatusCode, int grpcStatusCode, string? format, params object[] arguments)
        : base(priorityCode)
    {
        HttpStatusCode = httpStatusCode;
        GrpcStatusCode = grpcStatusCode;
        Format = format ?? string.Empty;
        Arguments = arguments ?? [];

        // Validate, this will throw FormatException if invalid.
        _ = string.Format(Format, Arguments);
    }

    [JsonInclude]
    public int HttpStatusCode { get; init; }
    [JsonInclude]
    public int GrpcStatusCode { get; init; }
    [JsonInclude]
    public string Format { get; init; }
    [JsonInclude]
    public object[] Arguments { get; init; }

    [JsonIgnore]
    public override bool SupportsFormattedMessage => true;
    public override string GetFormattedMessage() => string.Format(Format, Arguments);

    public override string ToString() => $"{PriorityCode}, {HttpStatusCode}, {GrpcStatusCode}, {GetFormattedMessage()}";

    #region IEquatable

    public override bool Equals(object? obj) => obj is RequestError apiError && Equals(apiError);
    public bool Equals(RequestError? other) => other is not null && EqualsCore(other);
    bool EqualsCore(RequestError other) =>
        PriorityCode == other.PriorityCode &&
        HttpStatusCode == other.HttpStatusCode &&
        GrpcStatusCode == other.GrpcStatusCode &&
        Format.Equals(other.Format, StringComparison.InvariantCulture) &&
        Arguments.SequenceEqual(other.Arguments);

    public static bool operator ==(RequestError left, RequestError right) =>
        !(left is null ^ right is null) && (left is null || left.EqualsCore(right!));
    public static bool operator !=(RequestError left, RequestError right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(PriorityCode, HttpStatusCode, GrpcStatusCode, Format, Arguments);

    #endregion
}
