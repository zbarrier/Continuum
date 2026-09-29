namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Machine-readable codes assigned to <see cref="Error.Code"/> for request errors.
///     These codes mirror the gRPC status codes and are the keys used to look up localized message templates.
/// </summary>
public static class ErrorCodes
{
    /// <summary>The operation was aborted, typically due to a concurrency conflict. The caller may retry.</summary>
    public const string Aborted = "aborted";
    /// <summary>The resource the client tried to create already exists.</summary>
    public const string AlreadyExists = "already_exists";
    /// <summary>The operation was cancelled, typically by the caller.</summary>
    public const string Cancelled = "cancelled";
    /// <summary>Unrecoverable data loss or corruption.</summary>
    public const string DataLoss = "data_loss";
    /// <summary>The deadline expired before the operation could complete.</summary>
    public const string DeadlineExceeded = "deadline_exceeded";
    /// <summary>The system is not in a state required for the operation.</summary>
    public const string FailedPrecondition = "failed_precondition";
    /// <summary>An internal error occurred.</summary>
    public const string Internal = "internal";
    /// <summary>The client specified an invalid argument.</summary>
    public const string InvalidArgument = "invalid_argument";
    /// <summary>The requested resource was not found.</summary>
    public const string NotFound = "not_found";
    /// <summary>The operation completed successfully.</summary>
    public const string Ok = "ok";
    /// <summary>The operation was attempted past the valid range.</summary>
    public const string OutOfRange = "out_of_range";
    /// <summary>The caller does not have permission to execute the operation.</summary>
    public const string PermissionDenied = "permission_denied";
    /// <summary>A resource, such as a quota, has been exhausted.</summary>
    public const string ResourceExhausted = "resource_exhausted";
    /// <summary>The request does not have valid authentication credentials.</summary>
    public const string Unauthenticated = "unauthenticated";
    /// <summary>The service is currently unavailable.</summary>
    public const string Unavailable = "unavailable";
    /// <summary>The operation is not implemented or not supported.</summary>
    public const string Unimplemented = "unimplemented";
    /// <summary>An unknown error occurred.</summary>
    public const string Unknown = "unknown";

    /// <summary>One or more validation errors occurred. Used by <see cref="ValidationError"/>.</summary>
    public const string ValidationFailed = "validation_failed";

    /// <summary>
    ///     Maps a numeric gRPC status code to its corresponding error code.
    /// </summary>
    /// <param name="grpcStatusCode">The numeric value of a <see cref="Grpc.Core.StatusCode"/>.</param>
    /// <returns>The matching error code, or <see cref="Unknown"/> if the status code is not recognized.</returns>
    public static string FromGrpcStatusCode(int grpcStatusCode) => FromGrpcStatusCode((Grpc.Core.StatusCode)grpcStatusCode);

    /// <summary>
    ///     Maps a gRPC status code to its corresponding error code.
    /// </summary>
    /// <param name="grpcStatusCode">The gRPC status code.</param>
    /// <returns>The matching error code, or <see cref="Unknown"/> if the status code is not recognized.</returns>
    public static string FromGrpcStatusCode(Grpc.Core.StatusCode grpcStatusCode) => grpcStatusCode switch
    {
        Grpc.Core.StatusCode.OK => Ok,
        Grpc.Core.StatusCode.Cancelled => Cancelled,
        Grpc.Core.StatusCode.InvalidArgument => InvalidArgument,
        Grpc.Core.StatusCode.DeadlineExceeded => DeadlineExceeded,
        Grpc.Core.StatusCode.NotFound => NotFound,
        Grpc.Core.StatusCode.AlreadyExists => AlreadyExists,
        Grpc.Core.StatusCode.PermissionDenied => PermissionDenied,
        Grpc.Core.StatusCode.ResourceExhausted => ResourceExhausted,
        Grpc.Core.StatusCode.FailedPrecondition => FailedPrecondition,
        Grpc.Core.StatusCode.OutOfRange => OutOfRange,
        Grpc.Core.StatusCode.Unimplemented => Unimplemented,
        Grpc.Core.StatusCode.Internal => Internal,
        Grpc.Core.StatusCode.Unavailable => Unavailable,
        Grpc.Core.StatusCode.DataLoss => DataLoss,
        Grpc.Core.StatusCode.Unauthenticated => Unauthenticated,
        Grpc.Core.StatusCode.Aborted => Aborted,
        _ => Unknown,
    };
}
