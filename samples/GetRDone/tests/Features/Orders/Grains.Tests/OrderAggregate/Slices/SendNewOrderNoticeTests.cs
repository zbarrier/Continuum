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

using static GetRDone.Orders.Grains.Tests.Data.SendNewOrderNoticeTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for recording the new order notice on an order that has its number. Each test uses a new
///     order in the test tenant. The first test checks what applying the slice's event does to the order state and the
///     second checks every value of the event the slice records; the behavior and query tests follow, then the tests of
///     the New Order Notifier's reaction to an order getting its number.
/// </summary>
public sealed class SendNewOrderNoticeTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private readonly RecordingEmailSender _emailSender;

    public SendNewOrderNoticeTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()))
        => _emailSender = fixture.EmailSender;

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.NewOrderNotifier.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingNewOrderNoticeSent_RecordsThatTheNoticeWasSent()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderNumberTestData.Assigned1(GrainId));

        state.When(Sent1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Submitted,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)AssignOrderNumberTestData.FirstOrderNumber,
            NewOrderNoticeSent = true,
            AssignedTo = (string?)null,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)null
        }, state, strict: true);
    }

    [Fact]
    public async Task Send_RecordsTheNoticeSentAboutTheNumberedOrder()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenNumbered();

        await When(() => Order.SendNewOrderNoticeAsync(Command1));

        Then<SendNewOrderNoticeResponse>(response =>
        {
            Assert.Equal(5, response.Version);
            var recorded = Assert.IsType<NewOrderNoticeSent>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Sent1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Send_SucceedsWithoutChangesWhenTheNoticeWasAlreadySent()
    {
        GivenSignedIn(Automation);
        await GivenNumbered(Sent1(GrainId));

        await When(() => Order.SendNewOrderNoticeAsync(Command1));

        Then<SendNewOrderNoticeResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Send_RejectsAnOrderWithoutANumber()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.SendNewOrderNoticeAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.SendNewOrderNoticeAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnInvalidRecipientEmail()
    {
        GivenSignedIn(Automation);
        await GivenNumbered();

        await When(() => Order.SendNewOrderNoticeAsync(Command1 with { RecipientEmail = "not an email" }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SendNewOrderNoticeCommand.RecipientEmail), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(AutomationUser.OrderNumberAssigner.CreatePrincipal(TestData.TenantId));
        await GivenNumbered();

        await When(() => Order.SendNewOrderNoticeAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SendNewOrderNoticeAsync(Command1));
    }

    // The harness's WhenQuery needs a reference type, so the query tests call the grain directly.

    [Fact]
    public async Task IsSent_IsFalseUntilTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenNumbered();

        var sent = await Order.IsNewOrderNoticeSentAsync();

        Assert.True(sent.IsSuccess);
        Assert.False(sent.Value);
    }

    [Fact]
    public async Task IsSent_IsTrueOnceTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenNumbered(Sent1(GrainId));

        var sent = await Order.IsNewOrderNoticeSentAsync();

        Assert.True(sent.IsSuccess);
        Assert.True(sent.Value);
    }

    [Fact]
    public async Task IsSent_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenNumbered();

        var sent = await Order.IsNewOrderNoticeSentAsync();

        Assert.True(sent.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, Assert.IsType<RequestError>(sent.Error).Code);
    }

    /// <summary>
    ///     Checks the reaction the harness bypasses: the number assignment, produced on the order's notification stream as
    ///     the stream provider delivers it, has the New Order Notifier email the test tenant's Client Services Director and
    ///     record the notice on the order. It is published signed out, as the stream provider delivers it with no
    ///     signed-in user.
    /// </summary>
    [Fact]
    public async Task OrderNumberAssigned_EmailsTheClientServicesDirectorAndRecordsTheNotice()
    {
        var orderNumber = NextOrderNumber();
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await GivenSubmitted(AssignOrderNumberTestData.Assigned1(GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(AssignOrderNumberTestData.Assigned1(GrainId, orderNumber)));

        var notice = await WaitForNoticeAsync();
        Assert.InRange(notice.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(Sent1(GrainId, orderNumber, notice.OccurredAt), notice, strict: true);
        var email = Assert.Single(EmailsAbout(orderNumber));
        Assert.Equal(TestData.Director1.Email, email.To);
        Assert.Equal(Subject(orderNumber), email.Subject);
        Assert.Contains("Hi Tommy Jones,", email.HtmlBody);
        Assert.Contains($"Lakeview Credit Union submitted order <strong>{orderNumber}</strong>", email.HtmlBody);
    }

    /// <summary>
    ///     An event for another order is ignored without holding back the order's stream, as the stream provider delivers
    ///     the events of one stream one after another.
    /// </summary>
    [Fact]
    public async Task OrderNumberAssigned_IgnoresAnEventForAnotherOrder()
    {
        var otherOrderNumber = NextOrderNumber();
        var orderNumber = NextOrderNumber();
        GivenSignedIn(Automation);
        await GivenSubmitted(AssignOrderNumberTestData.Assigned1(GrainId, orderNumber));
        RequestContext.Clear();
        var otherOrderId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

        await NotificationStream.OnNextAsync(OnOrderStream(AssignOrderNumberTestData.Assigned1(otherOrderId, otherOrderNumber)));
        await NotificationStream.OnNextAsync(OnOrderStream(AssignOrderNumberTestData.Assigned1(GrainId, orderNumber)));

        Assert.Equal(orderNumber, (await WaitForNoticeAsync()).OrderNumber);
        Assert.Empty(EmailsAbout(otherOrderNumber));
    }

    private static long _position;
    private static long _orderNumber = AssignOrderNumberTestData.FirstOrderNumber;

    /// <summary>A number no other test in the class uses, so each test finds only its own emails.</summary>
    private static long NextOrderNumber() => Interlocked.Increment(ref _orderNumber);

    private IEnumerable<EmailMessage> EmailsAbout(long orderNumber) => _emailSender.Sent.Where(email => email.Subject == Subject(orderNumber));

    /// <summary>The order's stream on the notification provider, which the order's New Order Notifier subscribes to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> NotificationStream => Cluster.Client.GetStreamProvider(NotificationReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(NotificationReactors.OrderNamespace, GrainId));

    /// <summary>An order event as the stream provider delivers it from the order's own stream.</summary>
    private static StreamedEvent<object> OnOrderStream(OrderNumberAssigned assigned)
    {
        TenantKey.Split(assigned.OrderId, out var tenantId, out var key);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.OrderNumberAssigned.V1",
            new StreamName($"order-{assigned.OrderId}", NotificationReactors.OrderCategory, tenantId, key!),
            position,
            new StreamedEventCursor("kurrentdb", NotificationReactors.OrderCategory, assigned.OrderId, position, 0),
            assigned.OccurredAt.UtcDateTime,
            assigned);
    }

    /// <summary>Waits until the order has saved the new order notice, then returns it.</summary>
    private async Task<NewOrderNoticeSent> WaitForNoticeAsync()
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var notices = (await Order.GetSavedEventsSince(0)).OfType<NewOrderNoticeSent>().ToList();
            if (notices.Count > 0)
            {
                return Assert.Single(notices);
            }
            await Task.Delay(50, timeout.Token);
        }
    }

    /// <summary>Sets up a submitted order, followed by <paramref name="more"/> events.</summary>
    private Task GivenSubmitted(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        .. more
    ]);

    /// <summary>Sets up a submitted order that has its first number, followed by <paramref name="more"/> events.</summary>
    private Task GivenNumbered(params OrderEvent[] more) => GivenSubmitted([AssignOrderNumberTestData.Assigned1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
