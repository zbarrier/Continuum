#nullable enable

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;

using GrpcStatusCodeEnum = Grpc.Core.StatusCode;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     An error that describes why a request failed, including the HTTP and gRPC status codes to return to the caller.
/// </summary>
/// <remarks>
///     <para>
///         Prefer the factories on <see cref="RequestErrors"/> (for example <c>RequestErrors.NewNotFound</c>), which pick the
///         recommended status code pair for you. Use the constructor only for cases the factories do not cover.
///     </para>
///     <para>
///         Both status codes are required. HTTP codes are more specific than gRPC codes, so one gRPC code can pair with several
///         HTTP codes. <see cref="Error.PriorityCode"/> is always derived from the gRPC code. Recommended pairs
///         (the CFE002 analyzer warns about other combinations):
///     </para>
///     <list type="table">
///         <listheader><term>HTTP</term><description>gRPC</description></listheader>
///         <item><term>400 Bad Request</term><description>InvalidArgument, FailedPrecondition, OutOfRange</description></item>
///         <item><term>401 Unauthorized</term><description>Unauthenticated</description></item>
///         <item><term>403 Forbidden</term><description>PermissionDenied</description></item>
///         <item><term>404 Not Found</term><description>NotFound</description></item>
///         <item><term>405 Method Not Allowed</term><description>Unimplemented</description></item>
///         <item><term>408 Request Timeout</term><description>DeadlineExceeded</description></item>
///         <item><term>409 Conflict</term><description>AlreadyExists, Aborted</description></item>
///         <item><term>410 Gone</term><description>NotFound</description></item>
///         <item><term>412 Precondition Failed</term><description>FailedPrecondition</description></item>
///         <item><term>413, 414, 415, 422, 431</term><description>InvalidArgument</description></item>
///         <item><term>416 Range Not Satisfiable</term><description>OutOfRange</description></item>
///         <item><term>428 Precondition Required</term><description>FailedPrecondition</description></item>
///         <item><term>429 Too Many Requests</term><description>ResourceExhausted</description></item>
///         <item><term>499 Client Closed Request</term><description>Cancelled</description></item>
///         <item><term>500 Internal Server Error</term><description>Internal, Unknown, DataLoss</description></item>
///         <item><term>501 Not Implemented</term><description>Unimplemented</description></item>
///         <item><term>502 Bad Gateway, 503 Service Unavailable</term><description>Unavailable</description></item>
///         <item><term>504 Gateway Timeout</term><description>DeadlineExceeded</description></item>
///         <item><term>507 Insufficient Storage</term><description>ResourceExhausted</description></item>
///     </list>
///     <para>
///         When <see cref="Format"/> is empty, the message is resolved from the localized template for <see cref="Error.Code"/>.
///     </para>
///     <para>
///         The format is validated on construction. Successful validations are cached per format and argument kinds, so a
///         format such as <c>"Order {0} was not found."</c> is validated once regardless of the argument values. Put identifiers in
///         the arguments, never in the format string (the CFE006 analyzer warns about interpolated formats). The cache is enabled by
///         default and is configured with the AppContext switch
///         <c>Continuum.CSharpFunctionalExtensions.RequestError.UseFormatValidationCache</c> (true/false) and the AppContext data
///         <c>Continuum.CSharpFunctionalExtensions.RequestError.FormatValidationCacheSize</c> (maximum entries, default 1024, 0 disables).
///     </para>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
public sealed class RequestError : Error
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestError"/> class.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code to return to the caller.</param>
    /// <param name="grpcStatusCode">The gRPC status code to return to the caller. Determines <see cref="Error.PriorityCode"/>.</param>
    /// <param name="code">A machine-readable error code, such as a value from <see cref="ErrorCodes"/>.</param>
    /// <param name="format">
    ///     A composite format string for the message, or null/empty to use the localized template for <paramref name="code"/>.
    /// </param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>. The array is used as-is and must not be modified afterward.</param>
    /// <exception cref="ArgumentException"><paramref name="code"/> is null, empty, or whitespace.</exception>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public RequestError(HttpStatusCode httpStatusCode, GrpcStatusCodeEnum grpcStatusCode, string code, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments)
        : this(httpStatusCode, grpcStatusCode, code, format, arguments, target: null, retryAfter: null)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestError"/> class.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code to return to the caller.</param>
    /// <param name="grpcStatusCode">The gRPC status code to return to the caller. Determines <see cref="Error.PriorityCode"/>.</param>
    /// <param name="code">A machine-readable error code, such as a value from <see cref="ErrorCodes"/>.</param>
    /// <param name="format">
    ///     A composite format string for the message, or null/empty to use the localized template for <paramref name="code"/>.
    /// </param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>. The array is used as-is and must not be modified afterward.</param>
    /// <param name="target">The resource, parameter or field the error is about, or null. See <see cref="Target"/>.</param>
    /// <param name="retryAfter">
    ///     How long the caller should wait before retrying, or null for no hint. See <see cref="RetryAfter"/>.
    ///     Only meaningful for retryable statuses; the CFE003 analyzer warns otherwise.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="code"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="retryAfter"/> is negative.</exception>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public RequestError(HttpStatusCode httpStatusCode, GrpcStatusCodeEnum grpcStatusCode, string code, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format, ErrorArgument[]? arguments,
        string? target, TimeSpan? retryAfter)
        : base(httpStatusCode, grpcStatusCode, code)
    {
        if (retryAfter is { } delay && delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(retryAfter), retryAfter, "RetryAfter cannot be negative.");
        }

        Format = format ?? string.Empty;
        ArgumentArray = arguments ?? [];
        Target = string.IsNullOrEmpty(target) ? null : target;
        RetryAfter = retryAfter;

        RequestErrorFormatValidationCache.Validate(Format, ArgumentArray);
    }

    /// <summary>Gets the composite format string for the message, or an empty string to use the localized template for <see cref="Error.Code"/>.</summary>
    public string Format { get; }

    /// <summary>Gets the arguments used to format the message.</summary>
    public ImmutableArray<ErrorArgument> Arguments => ImmutableCollectionsMarshal.AsImmutableArray(ArgumentArray);

    internal ErrorArgument[] ArgumentArray { get; }

    /// <summary>
    ///     Gets the resource, parameter or field the error is about, such as <c>orderId</c> or <c>items[2].quantity</c>, or null.
    /// </summary>
    public string? Target { get; }

    /// <summary>
    ///     Gets how long the caller should wait before retrying, or null when no timing hint is given.
    /// </summary>
    /// <remarks>
    ///     Null does not mean "do not retry": whether to retry is decided by the status code (for example Unavailable is usually
    ///     retryable, InvalidArgument never is). Serialized as whole seconds, matching the HTTP <c>Retry-After</c> header.
    /// </remarks>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    ///     Creates a copy of this error with a different <see cref="Error.Code"/>.
    /// </summary>
    /// <param name="code">The new error code.</param>
    /// <returns>A new <see cref="RequestError"/> with the same values and the specified code.</returns>
    public RequestError WithCode(string code) =>
        new(HttpStatusCode, GrpcStatusCode, code, Format, ArgumentArray, Target, RetryAfter);

    /// <summary>
    ///     Creates a copy of this error with a different <see cref="Target"/>.
    /// </summary>
    /// <param name="target">The new target, or null to clear it.</param>
    /// <returns>A new <see cref="RequestError"/> with the same values and the specified target.</returns>
    public RequestError WithTarget(string? target) =>
        new(HttpStatusCode, GrpcStatusCode, Code, Format, ArgumentArray, target, RetryAfter);

    /// <inheritdoc/>
    public override string GetFormattedMessage() =>
        LocalizedMessageFormatter.Format(Code, Format, ArgumentArray, false, CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture, null);
    /// <inheritdoc/>
    public override string GetFormattedMessage(CultureInfo culture, IErrorMessageLocalizer? localizer = null) =>
        LocalizedMessageFormatter.Format(Code, Format, ArgumentArray, false, culture, culture, localizer);

    /// <inheritdoc/>
    public override string ToString() => string.Create(CultureInfo.InvariantCulture,
        $"RequestError {{ Code = {Code}, PriorityCode = {PriorityCode}, HttpStatusCode = {(int)HttpStatusCode} ({HttpStatusCode}), GrpcStatusCode = {(int)GrpcStatusCode} ({GrpcStatusCode}), {(Target is null ? string.Empty : $"Target = {Target}, ")}{(RetryAfter is null ? string.Empty : string.Create(CultureInfo.InvariantCulture, $"RetryAfter = {RetryAfter}, "))}Message = \"{GetFormattedMessage(CultureInfo.InvariantCulture, InvariantLocalizer.Instance)}\" }}");

    /// <inheritdoc/>
    protected override bool EqualsCore(Error other)
    {
        var o = (RequestError)other;
        return string.Equals(Format, o.Format, StringComparison.Ordinal) &&
            string.Equals(Target, o.Target, StringComparison.Ordinal) &&
            RetryAfter == o.RetryAfter &&
            ArgumentArray.AsSpan().SequenceEqual(o.ArgumentArray);
    }

    /// <inheritdoc/>
    protected override void AddHashCodeCore(ref HashCode hash)
    {
        hash.Add(Format, StringComparer.Ordinal);
        hash.Add(Target, StringComparer.Ordinal);
        hash.Add(RetryAfter);
        foreach (var argument in ArgumentArray)
        {
            hash.Add(argument);
        }
    }
}
