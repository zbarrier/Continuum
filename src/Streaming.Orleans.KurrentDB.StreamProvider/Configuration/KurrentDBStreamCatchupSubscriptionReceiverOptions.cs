namespace Orleans.Configuration;

/// <summary>
///     Receiver options for the KurrentDB single stream catch-up subscription strategy.
/// </summary>
/// <remarks>
///     <see cref="KurrentDBOptions.Queues" /> must contain exactly one entry, the name of the stream to subscribe to,
///     such as <c>$ce-{Category}</c> or a custom projection stream. It cannot be <c>$all</c>. Events are positioned
///     by their revision in that stream, and the events of a transaction are not grouped, because the stream may
///     contain only some of them.
/// </remarks>
public sealed class KurrentDBStreamCatchupSubscriptionReceiverOptions : KurrentDBCatchupSubscriptionReceiverOptions;
