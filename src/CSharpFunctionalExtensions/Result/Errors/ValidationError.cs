#nullable enable

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     An error that contains one or more validation failures, each described by a <see cref="ValidationErrorEntry"/>.
/// </summary>
/// <remarks>
///     Always uses <see cref="ErrorCodes.ValidationFailed"/> as its <see cref="Error.Code"/>, HTTP 422 (Unprocessable Entity),
///     and gRPC <see cref="Grpc.Core.StatusCode.InvalidArgument"/>. Individual failures are identified by <see cref="ValidationErrorEntry.Code"/>.
/// </remarks>
[DebuggerDisplay("ValidationError {{ Code = {Code}, PriorityCode = {PriorityCode}, HttpStatusCode = {HttpStatusCode}, GrpcStatusCode = {GrpcStatusCode}, Entries = {Entries.Length} }}")]
public sealed class ValidationError : Error
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationError"/> class with a single entry of <see cref="ValidationSeverity.Error"/> severity.
    /// </summary>
    /// <param name="target">The name of the property or field that failed validation.</param>
    /// <param name="code">A machine-readable code for the failure, such as a value from <see cref="ValidationErrorCodes"/>.</param>
    /// <param name="errorMessage">A composite format string for the message, or null/empty to use the localized template for <paramref name="code"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="errorMessage"/>. The array is used as-is and must not be modified afterward.</param>
    /// <exception cref="ArgumentException"><paramref name="code"/> is null, empty, or whitespace.</exception>
    public ValidationError(string target, string code, string? errorMessage, params ErrorArgument[] arguments)
        : this(ValidationSeverity.Error, target, code, errorMessage, arguments)
    { }
    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationError"/> class with a single entry.
    /// </summary>
    /// <param name="severity">The severity of the failure.</param>
    /// <param name="target">The name of the property or field that failed validation.</param>
    /// <param name="code">A machine-readable code for the failure, such as a value from <see cref="ValidationErrorCodes"/>.</param>
    /// <param name="errorMessage">A composite format string for the message, or null/empty to use the localized template for <paramref name="code"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="errorMessage"/>. The array is used as-is and must not be modified afterward.</param>
    /// <exception cref="ArgumentException"><paramref name="code"/> is null, empty, or whitespace.</exception>
    public ValidationError(ValidationSeverity severity, string target, string code, string? errorMessage, params ErrorArgument[] arguments)
        : base(HttpStatusCode.UnprocessableEntity, Grpc.Core.StatusCode.InvalidArgument, ErrorCodes.ValidationFailed)
    {
        Entries = [new ValidationErrorEntry(severity, target, code, errorMessage, arguments)];
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationError"/> class with the specified entries.
    /// </summary>
    /// <param name="entries">The validation failures. Must contain at least one entry.</param>
    /// <exception cref="ArgumentException"><paramref name="entries"/> is empty or contains a null entry.</exception>
    public ValidationError(ImmutableArray<ValidationErrorEntry> entries)
        : base(HttpStatusCode.UnprocessableEntity, Grpc.Core.StatusCode.InvalidArgument, ErrorCodes.ValidationFailed)
    {
        if (entries.IsDefaultOrEmpty)
        {
            throw new ArgumentException("Entries cannot be null or empty.", nameof(entries));
        }
        if (entries.Contains(null!))
        {
            throw new ArgumentException("Entries cannot contain null.", nameof(entries));
        }

        Entries = entries;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationError"/> class with the specified entries.
    /// </summary>
    /// <param name="entries">The validation failures. Must contain at least one entry.</param>
    /// <exception cref="ArgumentException"><paramref name="entries"/> is null, empty, or contains a null entry.</exception>
    public ValidationError(IEnumerable<ValidationErrorEntry> entries)
        : this(entries?.ToImmutableArray() ?? default)
    { }

    /// <summary>Gets the individual validation failures.</summary>
    public ImmutableArray<ValidationErrorEntry> Entries { get; }

    /// <summary>
    ///     Gets the messages of all entries, separated by a line feed with no trailing newline, localized for <see cref="CultureInfo.CurrentUICulture"/> and formatted with <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    public override string GetFormattedMessage() => GetFormattedMessageCore(null, null);
    /// <summary>
    ///     Gets the messages of all entries, separated by a line feed with no trailing newline, localized and formatted for <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">The culture used to select message templates and to format arguments.</param>
    /// <param name="localizer">The localizer used to resolve message templates, or null to use the default.</param>
    /// <returns>The localized, formatted messages.</returns>
    public override string GetFormattedMessage(CultureInfo culture, IErrorMessageLocalizer? localizer = null) => GetFormattedMessageCore(culture, localizer);

    private string GetFormattedMessageCore(CultureInfo? culture, IErrorMessageLocalizer? localizer)
    {
        if (Entries.Length == 1)
        {
            return culture is null ? Entries[0].GetFormattedMessage() : Entries[0].GetFormattedMessage(culture, localizer);
        }

        var sb = new StringBuilder();
        for (int i = 0; i < Entries.Length; i++)
        {
            if (i > 0)
            {
                _ = sb.Append('\n');
            }
            _ = sb.Append(culture is null ? Entries[i].GetFormattedMessage() : Entries[i].GetFormattedMessage(culture, localizer));
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();
        _ = sb.Append(CultureInfo.InvariantCulture,
            $"ValidationError {{ Code = {Code}, PriorityCode = {PriorityCode}, HttpStatusCode = {(int)HttpStatusCode} ({HttpStatusCode}), " +
            $"GrpcStatusCode = {(int)GrpcStatusCode} ({GrpcStatusCode}), Entries = {Entries.Length} }}");

        if (Entries.Length > 0)
        {
            _ = sb.AppendLine();
        }

        foreach (var item in Entries)
        {
            _ = sb.AppendLine(item.ToString());
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    protected override bool EqualsCore(Error other) => Entries.SequenceEqual(((ValidationError)other).Entries);

    /// <inheritdoc/>
    protected override void AddHashCodeCore(ref HashCode hash)
    {
        foreach (var entry in Entries)
        {
            hash.Add(entry);
        }
    }
}

/// <summary>
///     A single validation failure within a <see cref="ValidationError"/>.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public sealed class ValidationErrorEntry : IEquatable<ValidationErrorEntry>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationErrorEntry"/> class.
    /// </summary>
    /// <remarks>
    ///     Unlike <see cref="RequestError"/>, <paramref name="format"/> is not validated at creation time. An invalid format
    ///     throws a <see cref="FormatException"/> when the message is formatted. Constant formats are checked at compile time
    ///     by analyzer CFE005; prefer constant formats over formats built per call (CFE006).
    /// </remarks>
    /// <param name="severity">The severity of the failure.</param>
    /// <param name="target">The name of the property or field that failed validation.</param>
    /// <param name="code">A machine-readable code for the failure, such as a value from <see cref="ValidationErrorCodes"/>.</param>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <paramref name="code"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>. By convention, the first argument is the property name. The array is used as-is and must not be modified afterward.</param>
    /// <exception cref="ArgumentException"><paramref name="code"/> is null, empty, or whitespace.</exception>
    public ValidationErrorEntry(ValidationSeverity severity, string target, string code, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format, ErrorArgument[]? arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Severity = severity;
        Target = target ?? string.Empty;
        Code = code;
        Format = format ?? string.Empty;
        ArgumentArray = arguments ?? [];
    }

    /// <summary>Gets the severity of the failure.</summary>
    public ValidationSeverity Severity { get; }
    /// <summary>Gets the name of the property or field that failed validation.</summary>
    public string Target { get; }
    /// <summary>Gets the machine-readable code for the failure. Also used as the key for message localization.</summary>
    public string Code { get; }
    /// <summary>Gets the composite format string for the message, or an empty string to use the localized template for <see cref="Code"/>.</summary>
    public string Format { get; }
    /// <summary>Gets the arguments used to format the message. By convention, the first argument is the property name.</summary>
    public ImmutableArray<ErrorArgument> Arguments => ImmutableCollectionsMarshal.AsImmutableArray(ArgumentArray);

    internal ErrorArgument[] ArgumentArray { get; }

    /// <summary>
    ///     Gets the message localized for <see cref="CultureInfo.CurrentUICulture"/> and formatted with <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    public string GetFormattedMessage() =>
        LocalizedMessageFormatter.Format(Code, Format, ArgumentArray, true, CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture, null);
    /// <summary>
    ///     Gets the message localized and formatted for <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">The culture used to select the message template and to format arguments.</param>
    /// <param name="localizer">The localizer used to resolve message templates, or null to use the default.</param>
    /// <returns>The localized, formatted message.</returns>
    public string GetFormattedMessage(CultureInfo culture, IErrorMessageLocalizer? localizer = null) =>
        LocalizedMessageFormatter.Format(Code, Format, ArgumentArray, true, culture, culture, localizer);

    /// <inheritdoc/>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture,
        $"{{ Severity = {GetSeverityName()}, Target = {Target}, Code = {Code}, Message = \"{GetFormattedMessage(CultureInfo.InvariantCulture, InvariantLocalizer.Instance)}\" }}");

    #region IEquatable

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ValidationErrorEntry entry && Equals(entry);
    /// <inheritdoc/>
    public bool Equals(ValidationErrorEntry? other) =>
        other is not null &&
        Severity == other.Severity &&
        string.Equals(Target, other.Target, StringComparison.Ordinal) &&
        string.Equals(Code, other.Code, StringComparison.Ordinal) &&
        string.Equals(Format, other.Format, StringComparison.Ordinal) &&
        ArgumentArray.AsSpan().SequenceEqual(other.ArgumentArray);

    /// <summary>Determines whether two entries are equal.</summary>
    public static bool operator ==(ValidationErrorEntry? left, ValidationErrorEntry? right) => left is null ? right is null : left.Equals(right);
    /// <summary>Determines whether two entries are not equal.</summary>
    public static bool operator !=(ValidationErrorEntry? left, ValidationErrorEntry? right) => !(left == right);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Severity);
        hash.Add(Target, StringComparer.Ordinal);
        hash.Add(Code, StringComparer.Ordinal);
        hash.Add(Format, StringComparer.Ordinal);
        foreach (var argument in ArgumentArray)
        {
            hash.Add(argument);
        }

        return hash.ToHashCode();
    }

    #endregion

    /// <summary>Gets the display name of <see cref="Severity"/>.</summary>
    /// <returns>"Error", "Warning" or "Info".</returns>
    public string GetSeverityName()
        => Severity switch
        {
            ValidationSeverity.Error => "Error",
            ValidationSeverity.Warning => "Warning",
            ValidationSeverity.Info => "Info",
            _ => throw new ArgumentOutOfRangeException("The validation severity type is invalid.")
        };
}

/// <summary>
///     The severity of a <see cref="ValidationErrorEntry"/>.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>The value is invalid.</summary>
    Error = 0,
    /// <summary>The value is accepted but questionable.</summary>
    Warning = 1,
    /// <summary>Informational only.</summary>
    Info = 2,
}
