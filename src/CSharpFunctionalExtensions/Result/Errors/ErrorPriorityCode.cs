namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Priority values for <see cref="Error.PriorityCode"/>. Lower values are more severe, which allows
///     the most important error to be selected when several errors are combined.
/// </summary>
public static class ErrorPriorityCode
{
    /// <summary>
    /// Unrecoverable data loss or corruption.
    /// </summary>
    public const int DATA_LOSS = 1;

    /// <summary>
    /// Internal errors. This means that some invariants expected by the underlying system have been broken. 
    /// This error code is reserved for serious errors.
    /// </summary>
    public const int INTERNAL = 2;

    /// <summary>
    /// The operation is not implemented or is not supported/enabled in this service.
    /// </summary>
    public const int UNIMPLEMENTED = 3;

    /// <summary>
    /// Unknown error. For example, this error may be returned when a Status value received from another address 
    /// space belongs to an error space that is not known in this address space. Also errors raised by APIs that 
    /// do not return enough error information may be converted to this error.
    /// </summary>
    public const int UNKNOWN = 4;

    /// <summary>
    /// The caller does not have permission to execute the specified operation. PERMISSION_DENIED must not be 
    /// used for rejections caused by exhausting some resource (use RESOURCE_EXHAUSTED instead for those errors). 
    /// PERMISSION_DENIED must not be used if the caller can not be identified (use UNAUTHENTICATED instead for those errors). 
    /// This error code does not imply the request is valid or the requested entity exists or satisfies other pre-conditions.
    /// </summary>
    public const int PERMISSION_DENIED = 5;

    /// <summary>
    /// The request does not have valid authentication credentials for the operation.
    /// </summary>
    public const int UNAUTHENTICATED = 6;

    /// <summary>
    /// Some resource has been exhausted, perhaps a per-user quota, or perhaps the entire file system is out of space.
    /// </summary>
    public const int RESOURCE_EXHAUSTED = 7;

    /// <summary>
    /// The client specified an invalid argument. Note that this differs from FAILED_PRECONDITION. 
    /// INVALID_ARGUMENT indicates arguments that are problematic regardless of the state of the system (e.g., a malformed file name).
    /// </summary>
    public const int INVALID_ARGUMENT = 8;

    /// <summary>
    /// The operation was rejected because the system is not in a state required for the operation's execution. 
    /// For example, the directory to be deleted is non-empty, an rmdir operation is applied to a non-directory, etc. 
    /// Service implementors can use the following guidelines to decide between FAILED_PRECONDITION, ABORTED, and UNAVAILABLE: 
    /// (a) Use UNAVAILABLE if the client can retry just the failing call. 
    /// (b) Use ABORTED if the client should retry at a higher level (e.g., when a client-specified test-and-set fails, indicating 
    /// the client should restart a read-modify-write sequence). 
    /// (c) Use FAILED_PRECONDITION if the client should not retry until the system state has been explicitly fixed. 
    /// E.g., if an "rmdir" fails because the directory is non-empty, FAILED_PRECONDITION should be returned since the client 
    /// should not retry unless the files are deleted from the directory.
    /// </summary>
    public const int FAILED_PRECONDITION = 9;

    /// <summary>
    /// The operation was attempted past the valid range. E.g., seeking or reading past end-of-file. 
    /// Unlike INVALID_ARGUMENT, this error indicates a problem that may be fixed if the system state changes. 
    /// For example, a 32-bit file system will generate INVALID_ARGUMENT if asked to read at an offset that 
    /// is not in the range [0,2^32-1], but it will generate OUT_OF_RANGE if asked to read from an offset past 
    /// the current file size. There is a fair bit of overlap between FAILED_PRECONDITION and OUT_OF_RANGE. 
    /// We recommend using OUT_OF_RANGE (the more specific error) when it applies so that callers who are iterating 
    /// through a space can easily look for an OUT_OF_RANGE error to detect when they are done.
    /// </summary>
    public const int OUT_OF_RANGE = 10;

    /// <summary>
    /// Some requested entity (e.g., file or directory) was not found. Note to server developers: if a request is denied 
    /// for an entire class of users, such as gradual feature rollout or undocumented allow list, NOT_FOUND may be used. 
    /// If a request is denied for some users within a class of users, such as user-based access control, PERMISSION_DENIED must be used.
    /// </summary>
    public const int NOT_FOUND = 11;

    /// <summary>
    /// The entity that a client attempted to create (e.g., file or directory) already exists.
    /// </summary>
    public const int ALREADY_EXISTS = 12;

    /// <summary>
    /// The operation was aborted, typically due to a concurrency issue such as a sequencer check failure or transaction abort. 
    /// See the guidelines above for deciding between FAILED_PRECONDITION, ABORTED, and UNAVAILABLE.
    /// </summary>
    public const int ABORTED = 13;

    /// <summary>
    /// The deadline expired before the operation could complete. For operations that change the state of the 
    /// system, this error may be returned even if the operation has completed successfully. For example, a 
    /// successful response from a server could have been delayed long.
    /// </summary>
    public const int DEADLINE_EXCEEDED = 14;

    /// <summary>
    /// The operation was cancelled, typically by the caller.
    /// </summary>
    public const int CANCELLED = 15;

    /// <summary>
    /// The service is currently unavailable. This is most likely a transient condition, which can be corrected 
    /// by retrying with a backoff. Note that it is not always safe to retry non-idempotent operations.
    /// </summary>
    public const int UNAVAILABLE = 16;

    /// <summary>
    ///     Gets the priority for a gRPC status code. This is the value of <see cref="Error.PriorityCode"/>.
    /// </summary>
    /// <param name="grpcStatusCode">The gRPC status code.</param>
    /// <returns>The priority. Lower values are more severe.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="grpcStatusCode"/> is OK or not a defined status code.</exception>
    public static int FromGrpcStatusCode(Grpc.Core.StatusCode grpcStatusCode) => grpcStatusCode switch
    {
        Grpc.Core.StatusCode.DataLoss => DATA_LOSS,
        Grpc.Core.StatusCode.Internal => INTERNAL,
        Grpc.Core.StatusCode.Unimplemented => UNIMPLEMENTED,
        Grpc.Core.StatusCode.Unknown => UNKNOWN,
        Grpc.Core.StatusCode.PermissionDenied => PERMISSION_DENIED,
        Grpc.Core.StatusCode.Unauthenticated => UNAUTHENTICATED,
        Grpc.Core.StatusCode.ResourceExhausted => RESOURCE_EXHAUSTED,
        Grpc.Core.StatusCode.InvalidArgument => INVALID_ARGUMENT,
        Grpc.Core.StatusCode.FailedPrecondition => FAILED_PRECONDITION,
        Grpc.Core.StatusCode.OutOfRange => OUT_OF_RANGE,
        Grpc.Core.StatusCode.NotFound => NOT_FOUND,
        Grpc.Core.StatusCode.AlreadyExists => ALREADY_EXISTS,
        Grpc.Core.StatusCode.Aborted => ABORTED,
        Grpc.Core.StatusCode.DeadlineExceeded => DEADLINE_EXCEEDED,
        Grpc.Core.StatusCode.Cancelled => CANCELLED,
        Grpc.Core.StatusCode.Unavailable => UNAVAILABLE,
        _ => throw new ArgumentOutOfRangeException(nameof(grpcStatusCode), grpcStatusCode, "Not a valid error status code."),
    };
}
