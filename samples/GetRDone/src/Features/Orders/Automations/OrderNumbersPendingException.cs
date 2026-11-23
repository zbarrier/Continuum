using Continuum.CSharpFunctionalExtensions;

using Grpc.Core;

namespace GetRDone.Orders.Automations;

/// <summary>
///     Thrown when some orders of a batch are still waiting for their order number because a call failed in a way
///     that is worth retrying, such as a timeout or an unavailable silo or store. The subscription retries the batch
///     until none are waiting, and only then moves its checkpoint past it.
/// </summary>
public sealed class OrderNumbersPendingException(string message) : Exception(message)
{
    /// <summary>Whether a call that failed with <paramref name="error"/> may succeed when it is retried.</summary>
    public static bool IsRetryable(Error error)
        => error.GrpcStatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded or StatusCode.Aborted or StatusCode.ResourceExhausted;
}
