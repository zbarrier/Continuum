using Continuum.CSharpFunctionalExtensions;

using Grpc.Core;

namespace GetRDone.Orders.Automations;

/// <summary>
///     Thrown when some orders of a batch are still waiting for their order number because a call failed in a way
///     that may succeed later, such as a timeout, an unavailable silo or store, a bug, or a missing permission. The
///     subscription retries the batch until none are waiting, and only then moves its checkpoint past it.
/// </summary>
/// <remarks>
///     Order numbers are required, so an order is skipped only when its event can never be handled: the event is corrupt
///     or doesn't match the order. Every other failure is retried, holding the subscription back until it is fixed.
/// </remarks>
public sealed class OrderNumbersPendingException(string message) : Exception(message)
{
    /// <summary>
    ///     Whether a call that failed with <paramref name="error"/> may succeed when it is retried; <see langword="false"/>
    ///     only when the event's data is invalid, the order doesn't exist or can't take a number, or it was already done.
    /// </summary>
    public static bool IsRetryable(Error error)
        => error.GrpcStatusCode is not (StatusCode.InvalidArgument or StatusCode.OutOfRange or StatusCode.NotFound
            or StatusCode.FailedPrecondition or StatusCode.AlreadyExists);
}
