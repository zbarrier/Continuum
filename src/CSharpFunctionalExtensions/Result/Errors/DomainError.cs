#nullable enable

using System.Net;
using System.Text.Json.Serialization;

using Continuum.CSharpFunctionalExtensions.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions;

public sealed class DomainError : Error, IEquatable<DomainError>
{
    public DomainError(int priorityCode, long domainErrorCode)
        : this(priorityCode, domainErrorCode, string.Empty, [])
    { }
    public DomainError(int priorityCode, long domainErrorCode, string? format)
        : this(priorityCode, domainErrorCode, format, [])
    { }
    [JsonConstructor]
    public DomainError(int priorityCode, long domainErrorCode, string? format, params object[] arguments)
        : base(priorityCode)
    {
        DomainErrorCode = domainErrorCode;
        Format = format ?? string.Empty;
        Arguments = arguments ?? [];

        // Validate, this will throw FormatException if invalid.
        _ = string.Format(Format, Arguments);
    }

    [JsonInclude]
    public long DomainErrorCode { get; init; }
    [JsonInclude]
    public string Format { get; init; }
    [JsonInclude]
    public object[] Arguments { get; init; }

    [JsonIgnore]
    public override bool SupportsFormattedMessage => true;
    public override string GetFormattedMessage() => string.Format(Format, Arguments);

    public override string ToString() => $"{PriorityCode}, {DomainErrorCode} {GetFormattedMessage()}";

    #region IEquatable

    public override bool Equals(object? obj) => obj is RequestError apiError && Equals(apiError);
    public bool Equals(DomainError? other) => other is not null && EqualsCore(other);
    bool EqualsCore(DomainError other) =>
        PriorityCode == other.PriorityCode &&
        DomainErrorCode == other.DomainErrorCode &&
        Format.Equals(other.Format, StringComparison.InvariantCulture) &&
        Arguments.SequenceEqual(other.Arguments);

    public static bool operator ==(DomainError left, DomainError right) =>
        !(left is null ^ right is null) && (left is null || left.EqualsCore(right!));
    public static bool operator !=(DomainError left, DomainError right) => !(left == right);

    public override int GetHashCode() => HashCode.Combine(PriorityCode, DomainErrorCode, Format, Arguments);

    #endregion
}
