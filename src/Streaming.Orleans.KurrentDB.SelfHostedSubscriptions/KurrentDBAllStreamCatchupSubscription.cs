using System.Diagnostics.CodeAnalysis;

using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     A hosted catch-up subscription that reads <c>$all</c>, optionally filtered on the server, and delivers events in
///     batches. Events appended in one transaction are delivered in the same batch, and progress is checkpointed by
///     <c>$all</c> commit position.
/// </summary>
/// <typeparam name="TSubscription">The concrete subscription type.</typeparam>
/// <typeparam name="TOptions">The subscription options type.</typeparam>
public abstract class KurrentDBAllStreamCatchupSubscription<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSubscription, TOptions>
    : KurrentDBCatchupSubscription<TSubscription, TOptions>
    where TSubscription : KurrentDBAllStreamCatchupSubscription<TSubscription, TOptions>
    where TOptions : KurrentDBCatchupSubscriptionOptions, new()
{
    /// <summary>
    ///     Initializes a new instance of the subscription.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve options and dependencies.</param>
    protected KurrentDBAllStreamCatchupSubscription(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    /// <summary>
    ///     The registration info for this subscription.
    /// </summary>
    public static KurrentDBAllStreamCatchupSubscriptionInfo Info => new(Name, Type);

    /// <summary>
    ///     Always <see cref="KurrentDBStreams.All" />.
    /// </summary>
    public sealed override string StreamName => KurrentDBStreams.All;

    /// <summary>
    ///     The server-side filter applied when reading <c>$all</c>. Defaults to
    ///     <see cref="EventTypeFilter.ExcludeSystemEvents" />, which skips <c>$</c>-prefixed system events such as
    ///     link events, metadata and checkpoints. Return <see langword="null" /> to read every event.
    /// </summary>
    public virtual IEventFilter? Filter => EventTypeFilter.ExcludeSystemEvents();

    private protected sealed override bool IsAllStream => true;

    private protected sealed override IEventFilter? FilterOptions => Filter;
}
