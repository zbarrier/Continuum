using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;

using GetRDone.Orders.Abstractions.OrderNumbers;
using GetRDone.Orders.Automations;

using KurrentDB.Client;

namespace GetRDone.Subscriptions.Worker.Orders;

/// <summary>
///     Reads the order number generator streams and gives each order the number it was issued (see
///     <see cref="OrderNumberAssigner"/>). Retries a batch, calling only the orders still waiting, until none are.
/// </summary>
public sealed class OrderNumberAssignerSubscription(IServiceProvider serviceProvider, OrderNumberAssigner assigner)
    : KurrentDBAllStreamCatchupSubscription<OrderNumberAssignerSubscription, KurrentDBCatchupSubscriptionOptions>(serviceProvider)
{
    public override IEventFilter? Filter => StreamFilter.Prefix("orderNumberGenerator-");

    protected override Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents)
        => assigner.AssignAsync(streamEvents
            .Select(streamEvent => streamEvent.Event)
            .OfType<OrderNumberIssued>());

    protected override bool IsTransient(Exception exception)
        => exception is OrderNumbersPendingException || base.IsTransient(exception);
}
