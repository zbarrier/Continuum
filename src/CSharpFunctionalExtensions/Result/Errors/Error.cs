#nullable enable

using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;

using GrpcStatusCodeEnum = Grpc.Core.StatusCode;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Base class for all errors carried by a failed <see cref="Result"/> or <see cref="Result{T}"/>.
/// </summary>
/// <remarks>
///     <para>
///         Use <see cref="Code"/> for programmatic handling and <see cref="GetFormattedMessage()"/> for display.
///         <see cref="ToString"/> returns a culture-invariant diagnostic representation that is not intended for end users.
///     </para>
///     <para>
///         <see cref="PriorityCode"/> is always derived from <see cref="GrpcStatusCode"/> and determines which error wins when
///         several are combined. Errors compare by priority through <see cref="IComparable{T}"/>: the most severe error sorts first.
///     </para>
/// </remarks>
public abstract class Error : IEquatable<Error>, IComparable<Error>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code to return to the caller.</param>
    /// <param name="grpcStatusCode">The gRPC status code to return to the caller. Also determines <see cref="PriorityCode"/>.</param>
    /// <param name="code">A machine-readable error code, such as a value from <see cref="ErrorCodes"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="code"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="grpcStatusCode"/> is <see cref="GrpcStatusCodeEnum.OK"/>.</exception>
    protected Error(HttpStatusCode httpStatusCode, GrpcStatusCodeEnum grpcStatusCode, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (grpcStatusCode == GrpcStatusCodeEnum.OK)
        {
            throw new ArgumentOutOfRangeException(nameof(grpcStatusCode), "An error cannot have the gRPC status code OK.");
        }

        HttpStatusCode = httpStatusCode;
        GrpcStatusCode = grpcStatusCode;
        Code = code;
    }

    /// <summary>
    ///     Gets the priority of the error, derived from <see cref="GrpcStatusCode"/>. Lower values are more severe. See <see cref="ErrorPriorityCode"/>.
    /// </summary>
    [JsonIgnore]
    public int PriorityCode => ErrorPriorityCode.FromGrpcStatusCode(GrpcStatusCode);

    /// <summary>
    ///     Gets the machine-readable error code. Also used as the key for message localization.
    /// </summary>
    public string Code { get; }

    /// <summary>Gets the HTTP status code to return to the caller.</summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>Gets the gRPC status code to return to the caller.</summary>
    public GrpcStatusCodeEnum GrpcStatusCode { get; }

    /// <summary>
    ///     Gets the message localized for
    /// </summary>
    public abstract string GetFormattedMessage();

    /// <summary>
    ///     Gets the message localized and formatted for <paramref name="culture"/>.
    ///     Uses <see cref="ErrorLocalization.Default"/> when <paramref name="localizer"/> is null.
    /// </summary>
    /// <param name="culture">The culture used to select the message template and to format arguments.</param>
    /// <param name="localizer">The localizer used to resolve message templates, or null to use the default.</param>
    /// <returns>The localized, formatted message.</returns>
    public virtual string GetFormattedMessage(CultureInfo culture, IErrorMessageLocalizer? localizer = null) => GetFormattedMessage();

    /// <summary>
    ///     Returns a culture-invariant diagnostic representation of the error, intended for logging and debugging.
    /// </summary>
    public abstract override string ToString();

    #region IEquatable / IComparable

    /// <summary>
    ///     Compares the members declared by a derived type. Called only when <paramref name="other"/> has the same runtime type
    ///     and the same <see cref="Code"/>, <see cref="HttpStatusCode"/> and <see cref="GrpcStatusCode"/>.
    /// </summary>
    /// <param name="other">The error to compare with, of the same runtime type as this instance.</param>
    /// <returns><see langword="true"/> when the derived members are equal.</returns>
    protected abstract bool EqualsCore(Error other);

    /// <summary>
    ///     Adds the members declared by a derived type to <paramref name="hash"/>.
    /// </summary>
    /// <param name="hash">The hash code builder.</param>
    protected abstract void AddHashCodeCore(ref HashCode hash);

    /// <inheritdoc/>
    public bool Equals(Error? other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         (other.GetType() == GetType() &&
          HttpStatusCode == other.HttpStatusCode &&
          GrpcStatusCode == other.GrpcStatusCode &&
          string.Equals(Code, other.Code, StringComparison.Ordinal) &&
          EqualsCore(other)));

    /// <inheritdoc/>
    public sealed override bool Equals(object? obj) => obj is Error other && Equals(other);

    /// <inheritdoc/>
    public sealed override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GetType());
        hash.Add(Code, StringComparer.Ordinal);
        hash.Add(HttpStatusCode);
        hash.Add(GrpcStatusCode);
        AddHashCodeCore(ref hash);
        return hash.ToHashCode();
    }

    /// <summary>
    ///     Compares errors by <see cref="PriorityCode"/>. The more severe error (lower priority code) sorts first; null sorts last.
    /// </summary>
    /// <param name="other">The error to compare with.</param>
    /// <returns>A negative value when this error is more severe than <paramref name="other"/>, zero when equal, otherwise positive.</returns>
    public int CompareTo(Error? other) => other is null ? 1 : PriorityCode.CompareTo(other.PriorityCode);

    /// <summary>Determines whether two errors are equal.</summary>
    public static bool operator ==(Error? left, Error? right) => left is null ? right is null : left.Equals(right);
    /// <summary>Determines whether two errors are not equal.</summary>
    public static bool operator !=(Error? left, Error? right) => !(left == right);

    #endregion
}
