namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Default (English) message templates for request errors, keyed by <see cref="ErrorCodes"/>.
///     Used when a <see cref="RequestError"/> has no custom format.
/// </summary>
public static class RequestErrorStrings
{
    /// <summary>Default message for <see cref="ErrorCodes.Aborted"/>.</summary>
    public const string Aborted = "The operation was aborted due to a conflict. Please retry.";
    /// <summary>Default message for <see cref="ErrorCodes.AlreadyExists"/>.</summary>
    public const string AlreadyExists = "The resource already exists.";
    /// <summary>Default message for <see cref="ErrorCodes.Cancelled"/>.</summary>
    public const string Cancelled = "The operation was cancelled.";
    /// <summary>Default message for <see cref="ErrorCodes.DataLoss"/>.</summary>
    public const string DataLoss = "Unrecoverable data loss or corruption.";
    /// <summary>Default message for <see cref="ErrorCodes.DeadlineExceeded"/>.</summary>
    public const string DeadlineExceeded = "The operation timed out.";
    /// <summary>Default message for <see cref="ErrorCodes.FailedPrecondition"/>.</summary>
    public const string FailedPrecondition = "The system is not in a state required for the operation.";
    /// <summary>Default message for <see cref="ErrorCodes.Internal"/>.</summary>
    public const string Internal = "An internal error occurred.";
    /// <summary>Default message for <see cref="ErrorCodes.InvalidArgument"/>.</summary>
    public const string InvalidArgument = "The request contains an invalid argument.";
    /// <summary>Default message for <see cref="ErrorCodes.NotFound"/>.</summary>
    public const string NotFound = "The requested resource was not found.";
    /// <summary>Default message for <see cref="ErrorCodes.OutOfRange"/>.</summary>
    public const string OutOfRange = "The operation was attempted past the valid range.";
    /// <summary>Default message for <see cref="ErrorCodes.PermissionDenied"/>.</summary>
    public const string PermissionDenied = "You do not have permission to perform this operation.";
    /// <summary>Default message for <see cref="ErrorCodes.ResourceExhausted"/>.</summary>
    public const string ResourceExhausted = "A resource has been exhausted.";
    /// <summary>Default message for <see cref="ErrorCodes.Unauthenticated"/>.</summary>
    public const string Unauthenticated = "The request does not have valid authentication credentials.";
    /// <summary>Default message for <see cref="ErrorCodes.Unavailable"/>.</summary>
    public const string Unavailable = "The service is currently unavailable.";
    /// <summary>Default message for <see cref="ErrorCodes.Unimplemented"/>.</summary>
    public const string Unimplemented = "Not Implemented.";
    /// <summary>Default message for <see cref="ErrorCodes.Unknown"/>.</summary>
    public const string Unknown = "An unknown error occurred.";
    /// <summary>Default message for <see cref="ErrorCodes.ValidationFailed"/>.</summary>
    public const string ValidationFailed = "One or more validation errors occurred.";
}
