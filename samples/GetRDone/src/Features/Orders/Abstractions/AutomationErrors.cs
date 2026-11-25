using Continuum.CSharpFunctionalExtensions;

using Grpc.Core;

namespace GetRDone.Orders.Abstractions;

/// <summary>How an automation treats a command that failed while it handled an event.</summary>
/// <remarks>
///     The automations' work is required, such as giving every submitted order its number, so an event is skipped only
///     when it can never be handled: its data is corrupt or doesn't match the grain. Every other failure is retried,
///     holding the automation back until it is fixed.
/// </remarks>
public static class AutomationErrors
{
    /// <summary>
    ///     Whether a command that failed with <paramref name="error"/> may succeed when it is retried; <see langword="false"/>
    ///     only when the event's data is invalid, the grain doesn't exist or can't take the command, or it was already done.
    /// </summary>
    public static bool IsRetryable(Error error)
        => error.GrpcStatusCode is not (StatusCode.InvalidArgument or StatusCode.OutOfRange or StatusCode.NotFound
            or StatusCode.FailedPrecondition or StatusCode.AlreadyExists);
}
