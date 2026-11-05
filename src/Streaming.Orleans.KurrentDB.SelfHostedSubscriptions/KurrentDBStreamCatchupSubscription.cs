using System.Diagnostics.CodeAnalysis;

using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     A hosted catch-up subscription that reads one named stream, such as a <c>$ce-</c> category stream or a custom
///     projection stream, and delivers events in batches. Link events are resolved, and progress is checkpointed by
///     stream revision.
/// </summary>
/// <typeparam name="TSubscription">The concrete subscription type.</typeparam>
/// <typeparam name="TOptions">The subscription options type.</typeparam>
public abstract class KurrentDBStreamCatchupSubscription<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSubscription, TOptions>
    : KurrentDBCatchupSubscription<TSubscription, TOptions>
    where TSubscription : KurrentDBStreamCatchupSubscription<TSubscription, TOptions>
    where TOptions : KurrentDBCatchupSubscriptionOptions, new()
{
    /// <summary>
    ///     Initializes a new instance of the subscription.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve options and dependencies.</param>
    protected KurrentDBStreamCatchupSubscription(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    /// <summary>
    ///     The registration info for this subscription.
    /// </summary>
    public static KurrentDBStreamCatchupSubscriptionInfo Info => new(Name, Type);

    private protected sealed override bool IsAllStream => false;

    private protected sealed override IEventFilter? FilterOptions => null;
}
