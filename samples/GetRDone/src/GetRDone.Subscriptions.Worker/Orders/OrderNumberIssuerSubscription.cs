using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Automations;

using KurrentDB.Client;

namespace GetRDone.Subscriptions.Worker.Orders;

/// <summary>
///     Reads the order streams and has each tenant's generator issue numbers to the orders submitted in a batch (see
///     <see cref="OrderNumberIssuer"/>). Retries a batch until no tenant's numbers are waiting.
/// </summary>
public sealed class OrderNumberIssuerSubscription(IServiceProvider serviceProvider, OrderNumberIssuer issuer)
    : KurrentDBAllStreamCatchupSubscription<OrderNumberIssuerSubscription, KurrentDBCatchupSubscriptionOptions>(serviceProvider)
{
    public override IEventFilter? Filter => StreamFilter.Prefix("order-");

    protected override Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents)
        => issuer.IssueAsync(streamEvents.Select(streamEvent => streamEvent.Event).OfType<OrderSubmitted>());

    protected override bool IsTransient(Exception exception)
        => exception is OrderNumbersPendingException || base.IsTransient(exception);
}
