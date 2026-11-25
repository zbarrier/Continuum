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

using static GetRDone.Orders.Grains.Tests.Data.DocumentRequestTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for recording the notice of a document request. Each test uses a new order in the test
///     tenant. The first test checks what applying the slice's event does to the order state and the second checks every
///     value of the event the slice records; the behavior and query tests follow, then the tests of the Document Request
///     Notifier's reaction to a request.
/// </summary>
public sealed class SendDocumentRequestNoticeTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private readonly RecordingEmailSender _emailSender;

    public SendDocumentRequestNoticeTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()))
        => _emailSender = fixture.EmailSender;

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.DocumentRequestNotifier.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingDocumentRequestNoticeSent_RecordsThatTheRequestsNoticeWasSent()
    {
        var state = new OrderState();
        state.When(StartOrderDraftTestData.DraftStarted1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(Requested1(GrainId));

        state.When(NoticeSent1(GrainId));

        Assert.True(state.DocumentRequests[Command1.RequestId].NoticeSent);
        Assert.False(state.DocumentRequests[Command1.RequestId].Provided);
    }

    [Fact]
    public async Task Send_RecordsTheNoticeSentToTheClient()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenRequested();

        await When(() => Order.SendDocumentRequestNoticeAsync(NoticeCommand1()));

        Then<SendDocumentRequestNoticeResponse>(response =>
        {
            Assert.Equal(7, response.Version);
            var recorded = Assert.IsType<DocumentRequestNoticeSent>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(NoticeSent1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Send_SucceedsWithoutChangesWhenTheNoticeWasAlreadySent()
    {
        GivenSignedIn(Automation);
        await GivenRequested(NoticeSent1(GrainId));

        await When(() => Order.SendDocumentRequestNoticeAsync(NoticeCommand1()));

        Then<SendDocumentRequestNoticeResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Send_RejectsARequestThatDoesNotExist()
    {
        GivenSignedIn(Automation);
        await GivenRequested();

        await When(() => Order.SendDocumentRequestNoticeAsync(
            NoticeCommand1() with { RequestId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsARecipientTheDocumentsWereNotRequestedFrom()
    {
        GivenSignedIn(Automation);
        await GivenRequested();

        await When(() => Order.SendDocumentRequestNoticeAsync(
            NoticeCommand1() with { RecipientUserId = TestData.ClientUser2.UserId, RecipientEmail = TestData.ClientUser2.Email }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnInvalidRecipientEmail()
    {
        GivenSignedIn(Automation);
        await GivenRequested();

        await When(() => Order.SendDocumentRequestNoticeAsync(NoticeCommand1() with { RecipientEmail = "not an email" }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SendDocumentRequestNoticeCommand.RecipientEmail), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.SendDocumentRequestNoticeAsync(NoticeCommand1()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(AutomationUser.NewOrderNotifier.CreatePrincipal(TestData.TenantId));
        await GivenRequested();

        await When(() => Order.SendDocumentRequestNoticeAsync(NoticeCommand1()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SendDocumentRequestNoticeAsync(NoticeCommand1()));
    }

    // The harness's WhenQuery needs a reference type, so the query tests call the grain directly.

    [Fact]
    public async Task IsSent_IsFalseUntilTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenRequested();

        var sent = await Order.IsDocumentRequestNoticeSentAsync(Command1.RequestId);

        Assert.True(sent.IsSuccess);
        Assert.False(sent.Value);
    }

    [Fact]
    public async Task IsSent_IsTrueOnceTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenRequested(NoticeSent1(GrainId));

        var sent = await Order.IsDocumentRequestNoticeSentAsync(Command1.RequestId);

        Assert.True(sent.IsSuccess);
        Assert.True(sent.Value);
    }

    [Fact]
    public async Task IsSent_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenRequested();

        var sent = await Order.IsDocumentRequestNoticeSentAsync(Command1.RequestId);

        Assert.True(sent.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, Assert.IsType<RequestError>(sent.Error).Code);
    }

    /// <summary>
    ///     Checks the reaction the harness bypasses: the request, produced on the order's notification stream as the stream
    ///     provider delivers it, has the Document Request Notifier email the client a link to the order and record the
    ///     notice on the order. It is published signed out, as the stream provider delivers it with no signed-in user.
    /// </summary>
    [Fact]
    public async Task ClientDocumentRequested_EmailsTheClientALinkToTheOrderAndRecordsTheNotice()
    {
        var orderNumber = NextOrderNumber();
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await GivenAcknowledged(AssignOrderNumberTestData.Assigned1(GrainId, orderNumber), Requested1(GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Requested1(GrainId, orderNumber)));

        var notice = await WaitForNoticeAsync(Command1.RequestId);
        Assert.InRange(notice.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(NoticeSent1(GrainId, orderNumber, notice.OccurredAt), notice, strict: true);
        var email = Assert.Single(EmailsAbout(orderNumber));
        TenantKey.Split(GrainId, out _, out var orderGuid);
        Assert.Equal(TestData.ClientUser1.Email, email.To);
        Assert.Equal(Subject(orderNumber), email.Subject);
        Assert.Contains("Hi John Doe,", email.HtmlBody);
        Assert.Contains("please upload a <strong>Survey</strong>", email.HtmlBody);
        Assert.Contains(Command1.Message, email.HtmlBody);
        Assert.Contains($"<a href=\"https://getrdone.test/orders/{orderGuid}\">", email.HtmlBody);
    }

    /// <summary>
    ///     The request event is delivered again, such as after a failure: the order already has the notice, so the client
    ///     isn't emailed twice. Another request follows it on the stream; the stream provider delivers one order's events one
    ///     after another, so once that request's notice is recorded, the delivery before it was handled.
    /// </summary>
    [Fact]
    public async Task ClientDocumentRequested_DoesNotEmailTheClientAgainOnceTheNoticeIsRecorded()
    {
        var orderNumber = NextOrderNumber();
        GivenSignedIn(Automation);
        await GivenAcknowledged(
            AssignOrderNumberTestData.Assigned1(GrainId, orderNumber),
            Requested1(GrainId, orderNumber),
            NoticeSent1(GrainId, orderNumber),
            Requested(Command2, GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Requested1(GrainId, orderNumber)));
        await NotificationStream.OnNextAsync(OnOrderStream(Requested(Command2, GrainId, orderNumber)));

        await WaitForNoticeAsync(Command2.RequestId);
        Assert.Empty(EmailsAbout(orderNumber));
    }

    private static long _position;
    private static long _orderNumber = AssignOrderNumberTestData.FirstOrderNumber + 50_000;

    /// <summary>A number no other test in the class uses, so each test finds only its own emails.</summary>
    private static long NextOrderNumber() => Interlocked.Increment(ref _orderNumber);

    private IEnumerable<EmailMessage> EmailsAbout(long orderNumber) => _emailSender.Sent.Where(email => email.Subject == Subject(orderNumber));

    /// <summary>The order's stream on the notification provider, which the order's notifiers subscribe to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> NotificationStream => Cluster.Client.GetStreamProvider(NotificationReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(NotificationReactors.OrderNamespace, GrainId));

    /// <summary>An order event as the stream provider delivers it from the order's own stream.</summary>
    private static StreamedEvent<object> OnOrderStream(ClientDocumentRequested requested)
    {
        TenantKey.Split(requested.OrderId, out var tenantId, out var key);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.ClientDocumentRequested.V1",
            new StreamName($"order-{requested.OrderId}", NotificationReactors.OrderCategory, tenantId, key!),
            position,
            new StreamedEventCursor("kurrentdb", NotificationReactors.OrderCategory, requested.OrderId, position, 0),
            requested.OccurredAt.UtcDateTime,
            requested);
    }

    /// <summary>Waits until the order has saved the notice of <paramref name="requestId"/>, then returns it.</summary>
    private async Task<DocumentRequestNoticeSent> WaitForNoticeAsync(string requestId)
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var notices = (await Order.GetSavedEventsSince(0)).OfType<DocumentRequestNoticeSent>()
                .Where(notice => notice.RequestId == requestId).ToList();
            if (notices.Count > 0)
            {
                return Assert.Single(notices);
            }
            await Task.Delay(50, timeout.Token);
        }
    }

    /// <summary>Sets up a submitted order that Director1 was assigned and acknowledged, followed by <paramref name="more"/> events.</summary>
    private Task GivenAcknowledged(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        .. more
    ]);

    /// <summary>Sets up an acknowledged order with the request <see cref="Command1"/>, followed by <paramref name="more"/> events.</summary>
    private Task GivenRequested(params OrderEvent[] more) => GivenAcknowledged([Requested1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
