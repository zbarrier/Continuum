using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using GetRDone.Email;
using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;
using Orleans.Streams;

using static GetRDone.Orders.Grains.Tests.Data.ShippingTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for recording the notice of a shipped order. Each test uses a new order in the test tenant. The
///     first test checks what applying the slice's event does to the order state and the second checks every value of the
///     event the slice records; the behavior and query tests follow, then the tests of the Order Shipped Notifier's reaction
///     to a shipped order.
/// </summary>
public sealed class SendOrderShippedNoticeTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private readonly RecordingEmailSender _emailSender;

    public SendOrderShippedNoticeTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()))
        => _emailSender = fixture.EmailSender;

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.OrderShippedNotifier.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingOrderShippedNoticeSent_RecordsThatTheNoticeWasSent()
    {
        var state = new OrderState();
        foreach (var evt in ShippedEvents())
        {
            state.When(evt);
        }

        state.When(NoticeSent1(GrainId));

        Assert.True(state.OrderShippedNoticeSent);
        Assert.Equal(OrderStatus.Shipped, state.Status);
    }

    [Fact]
    public async Task Send_RecordsTheNoticeSentToTheClientWhoEnteredTheOrder()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenShipped();

        await When(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1()));

        Then<SendOrderShippedNoticeResponse>(response =>
        {
            Assert.Equal(13, response.Version);
            var recorded = Assert.IsType<OrderShippedNoticeSent>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(NoticeSent1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Send_SucceedsWithoutChangesWhenTheNoticeWasAlreadySent()
    {
        GivenSignedIn(Automation);
        await GivenShipped(NoticeSent1(GrainId));

        await When(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1()));

        Then<SendOrderShippedNoticeResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatIsNotShipped()
    {
        GivenSignedIn(Automation);
        await Given(DeliveredEvents());

        await When(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnotherDocument()
    {
        GivenSignedIn(Automation);
        await GivenShipped();

        await When(() => Order.SendOrderShippedNoticeAsync(
            NoticeCommand1() with { DocumentId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsARecipientWhoDidNotEnterTheOrder()
    {
        GivenSignedIn(Automation);
        await GivenShipped();

        await When(() => Order.SendOrderShippedNoticeAsync(
            NoticeCommand1() with { RecipientUserId = TestData.ClientUser2.UserId, RecipientEmail = TestData.ClientUser2.Email }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnInvalidRecipientEmail()
    {
        GivenSignedIn(Automation);
        await GivenShipped();

        await When(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1() with { RecipientEmail = "not an email" }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SendOrderShippedNoticeCommand.RecipientEmail), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(AutomationUser.ReportDeliveredNotifier.CreatePrincipal(TestData.TenantId));
        await GivenShipped();

        await When(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SendOrderShippedNoticeAsync(NoticeCommand1()));
    }

    // The harness's WhenQuery needs a reference type, so the query tests call the grain directly.

    [Fact]
    public async Task IsSent_IsFalseUntilTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenShipped();

        var sent = await Order.IsOrderShippedNoticeSentAsync(ShipCommand1.DocumentId);

        Assert.True(sent.IsSuccess);
        Assert.False(sent.Value);
    }

    [Fact]
    public async Task IsSent_IsTrueOnceTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenShipped(NoticeSent1(GrainId));

        var sent = await Order.IsOrderShippedNoticeSentAsync(ShipCommand1.DocumentId);

        Assert.True(sent.IsSuccess);
        Assert.True(sent.Value);
    }

    [Fact]
    public async Task IsSent_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenShipped();

        var sent = await Order.IsOrderShippedNoticeSentAsync(ShipCommand1.DocumentId);

        Assert.True(sent.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, Assert.IsType<RequestError>(sent.Error).Code);
    }

    /// <summary>
    ///     Checks the reaction the harness bypasses: the shipped order, produced on the order's notification stream as the
    ///     stream provider delivers it, has the Order Shipped Notifier email the client who entered the order a link to the
    ///     client order page and record the notice on the order. It is published signed out, as the stream provider delivers
    ///     it with no signed-in user.
    /// </summary>
    [Fact]
    public async Task OrderShipped_EmailsTheClientALinkToTheOrderAndRecordsTheNotice()
    {
        var orderNumber = NextOrderNumber();
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await Given([.. AssignedEvents(), AssignOrderNumberTestData.Assigned1(GrainId, orderNumber), InspectionTestData.Scheduled1(GrainId), InspectionTestData.Completed1(GrainId),
            InspectionTestData.Delivered1(GrainId, orderNumber), Shipped1(GrainId, orderNumber)]);
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Shipped1(GrainId, orderNumber)));

        var notice = await WaitForNoticeAsync();
        Assert.InRange(notice.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(NoticeSent1(GrainId, orderNumber, notice.OccurredAt), notice, strict: true);
        var email = Assert.Single(EmailsAbout(orderNumber));
        TenantKey.Split(GrainId, out _, out var orderGuid);
        Assert.Equal(TestData.ClientUser1.Email, email.To);
        Assert.Equal(NoticeSubject(orderNumber), email.Subject);
        Assert.Contains("Hi John Doe,", email.HtmlBody);
        Assert.Contains("Summit Valuation completed your order. You can download the report from the order.", email.HtmlBody);
        Assert.Contains($"<a href=\"https://getrdone.test/orders/{orderGuid}\">", email.HtmlBody);
    }

    private static long _position;
    private static long _orderNumber = AssignOrderNumberTestData.FirstOrderNumber + 100_000;

    /// <summary>A number no other test in the class uses, so each test finds only its own emails.</summary>
    private static long NextOrderNumber() => Interlocked.Increment(ref _orderNumber);

    private IEnumerable<EmailMessage> EmailsAbout(long orderNumber)
        => _emailSender.Sent.Where(email => email.Subject == NoticeSubject(orderNumber));

    /// <summary>The order's stream on the notification provider, which the order's notifiers subscribe to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> NotificationStream => Cluster.Client.GetStreamProvider(NotificationReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(NotificationReactors.OrderNamespace, GrainId));

    /// <summary>An order event as the stream provider delivers it from the order's own stream.</summary>
    private static StreamedEvent<object> OnOrderStream(OrderShipped shipped)
    {
        TenantKey.Split(shipped.OrderId, out var tenantId, out var key);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.OrderShipped.V1",
            new StreamName($"order-{shipped.OrderId}", NotificationReactors.OrderCategory, tenantId, key!),
            position,
            new StreamedEventCursor("kurrentdb", NotificationReactors.OrderCategory, shipped.OrderId, position, 0),
            shipped.OccurredAt.UtcDateTime,
            shipped);
    }

    /// <summary>Waits until the order has saved the notice, then returns it.</summary>
    private async Task<OrderShippedNoticeSent> WaitForNoticeAsync()
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var notices = (await Order.GetSavedEventsSince(0)).OfType<OrderShippedNoticeSent>().ToList();
            if (notices.Count > 0)
            {
                return Assert.Single(notices);
            }
            await Task.Delay(50, timeout.Token);
        }
    }

    private OrderEvent[] AssignedEvents() =>
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        DescriptionOfWorkTestData.Submitted1(GrainId),
        DescriptionOfWorkTestData.Approved(GrainId),
        VendorAssignmentTestData.Assigned1(GrainId)
    ];

    private OrderEvent[] DeliveredEvents() => [.. AssignedEvents(), InspectionTestData.Scheduled1(GrainId), InspectionTestData.Completed1(GrainId), InspectionTestData.Delivered1(GrainId)];

    private OrderEvent[] ShippedEvents() => [.. DeliveredEvents(), Shipped1(GrainId)];

    /// <summary>Sets up an order Director1 shipped, followed by <paramref name="more"/> events.</summary>
    private Task GivenShipped(params OrderEvent[] more) => Given([.. ShippedEvents(), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
