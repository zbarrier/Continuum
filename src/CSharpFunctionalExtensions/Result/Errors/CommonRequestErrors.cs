using System.Diagnostics.CodeAnalysis;
using System.Net;

using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Factory methods for creating <see cref="RequestError"/> instances for common failure types.
/// </summary>
/// <remarks>
///     Prefer these factories over the <see cref="RequestError"/> constructor: each one uses the recommended HTTP and gRPC
///     status code pair and error code. Only the retryable statuses (Aborted, DeadlineExceeded, ResourceExhausted and
///     Unavailable) accept a <c>retryAfter</c> hint.
/// </remarks>
public static class RequestErrors
{
    /// <summary>
    ///     Creates a <see cref="RequestError"/> with the specified status codes. The error code is derived from <paramref name="grpcStatusCode"/>.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code to return to the caller. See <see cref="RequestError"/> for recommended pairs.</param>
    /// <param name="grpcStatusCode">The gRPC status code to return to the caller.</param>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for the derived error code.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError New(HttpStatusCode httpStatusCode, GrpcStatusCode grpcStatusCode, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments)
        => new(httpStatusCode, grpcStatusCode, ErrorCodes.FromGrpcStatusCode(grpcStatusCode), format, arguments);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating a concurrency conflict, using HTTP 409 (Conflict) and gRPC <see cref="GrpcStatusCode.Aborted"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.Aborted"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewAborted([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.Conflict, GrpcStatusCode.Aborted, ErrorCodes.Aborted, format, arguments);
    /// <inheritdoc cref="NewAborted(string?, ErrorArgument[])"/>
    /// <param name="retryAfter">How long the caller should wait before retrying, or null for no hint.</param>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.Aborted"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    public static RequestError NewAborted(TimeSpan? retryAfter, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.Conflict, GrpcStatusCode.Aborted, ErrorCodes.Aborted, format, arguments, null, retryAfter);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating a resource already exists, using HTTP 409 (Conflict) and gRPC <see cref="GrpcStatusCode.AlreadyExists"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.AlreadyExists"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewAlreadyExists([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.Conflict, GrpcStatusCode.AlreadyExists, ErrorCodes.AlreadyExists, format, arguments);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating an operation timed out, using HTTP 504 (Gateway Timeout) and gRPC <see cref="GrpcStatusCode.DeadlineExceeded"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.DeadlineExceeded"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewDeadlineExceeded([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.GatewayTimeout, GrpcStatusCode.DeadlineExceeded, ErrorCodes.DeadlineExceeded, format, arguments);
    /// <inheritdoc cref="NewDeadlineExceeded(string?, ErrorArgument[])"/>
    /// <param name="retryAfter">How long the caller should wait before retrying, or null for no hint.</param>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.DeadlineExceeded"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    public static RequestError NewDeadlineExceeded(TimeSpan? retryAfter, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.GatewayTimeout, GrpcStatusCode.DeadlineExceeded, ErrorCodes.DeadlineExceeded, format, arguments, null, retryAfter);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating the system is not in a state required for the operation, using HTTP 400 (Bad Request) and gRPC <see cref="GrpcStatusCode.FailedPrecondition"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.FailedPrecondition"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewFailedPrecondition([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.BadRequest, GrpcStatusCode.FailedPrecondition, ErrorCodes.FailedPrecondition, format, arguments);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating an invalid argument, using HTTP 422 (Unprocessable Entity) and gRPC <see cref="GrpcStatusCode.InvalidArgument"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.InvalidArgument"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewInvalidArg([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.UnprocessableEntity, GrpcStatusCode.InvalidArgument, ErrorCodes.InvalidArgument, format, arguments);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating a resource was not found, using HTTP 404 (Not Found) and gRPC <see cref="GrpcStatusCode.NotFound"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.NotFound"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewNotFound([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.NotFound, GrpcStatusCode.NotFound, ErrorCodes.NotFound, format, arguments);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating a quota or rate limit was exceeded, using HTTP 429 (Too Many Requests) and gRPC <see cref="GrpcStatusCode.ResourceExhausted"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.ResourceExhausted"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewResourceExhausted([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.TooManyRequests, GrpcStatusCode.ResourceExhausted, ErrorCodes.ResourceExhausted, format, arguments);
    /// <inheritdoc cref="NewResourceExhausted(string?, ErrorArgument[])"/>
    /// <param name="retryAfter">How long the caller should wait before retrying, or null for no hint.</param>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.ResourceExhausted"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    public static RequestError NewResourceExhausted(TimeSpan? retryAfter, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.TooManyRequests, GrpcStatusCode.ResourceExhausted, ErrorCodes.ResourceExhausted, format, arguments, null, retryAfter);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating the operation is not implemented, using HTTP 501 (Not Implemented) and gRPC <see cref="GrpcStatusCode.Unimplemented"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.Unimplemented"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewNotImplemented([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.NotImplemented, GrpcStatusCode.Unimplemented, ErrorCodes.Unimplemented, format, arguments);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating the service is unavailable, using HTTP 503 (Service Unavailable) and gRPC <see cref="GrpcStatusCode.Unavailable"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.Unavailable"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewUnavailable([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.ServiceUnavailable, GrpcStatusCode.Unavailable, ErrorCodes.Unavailable, format, arguments);
    /// <inheritdoc cref="NewUnavailable(string?, ErrorArgument[])"/>
    /// <param name="retryAfter">How long the caller should wait before retrying, or null for no hint.</param>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.Unavailable"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    public static RequestError NewUnavailable(TimeSpan? retryAfter, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.ServiceUnavailable, GrpcStatusCode.Unavailable, ErrorCodes.Unavailable, format, arguments, null, retryAfter);

    /// <summary>
    ///     Creates a <see cref="RequestError"/> indicating an unknown error, using HTTP 500 (Internal Server Error) and gRPC <see cref="GrpcStatusCode.Unknown"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null/empty to use the localized template for <see cref="ErrorCodes.Unknown"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewUnknown([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.InternalServerError, GrpcStatusCode.Unknown, ErrorCodes.Unknown, format, arguments);
}
