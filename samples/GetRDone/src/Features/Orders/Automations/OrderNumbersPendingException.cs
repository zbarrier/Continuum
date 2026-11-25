using GetRDone.Orders.Abstractions;

namespace GetRDone.Orders.Automations;

/// <summary>
///     Thrown when some orders of a batch are still waiting to be issued their order numbers because a call failed in a
///     way that may succeed later, such as a timeout, an unavailable silo or store, a bug, or a missing permission. The
///     subscription retries the batch until none are waiting, and only then moves its checkpoint past it.
/// </summary>
/// <remarks>
///     Order numbers are required, so an order is skipped only when its event can never be handled (see
///     <see cref="AutomationErrors.IsRetryable"/>). Every other failure is retried, holding the subscription back until
///     it is fixed.
/// </remarks>
public sealed class OrderNumbersPendingException(string message) : Exception(message);
