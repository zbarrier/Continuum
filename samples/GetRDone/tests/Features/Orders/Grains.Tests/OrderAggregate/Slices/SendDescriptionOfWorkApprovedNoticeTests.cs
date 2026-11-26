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

using static GetRDone.Orders.Grains.Tests.Data.DescriptionOfWorkTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for recording the notice of an approved description of work. Each test uses a new order in the
///     test tenant. The first test checks what applying the slice's event does to the order state and the second checks
///     every value of the event the slice records; the behavior and query tests follow, then the tests of the Description of
///     Work Approved Notifier's reaction to an approval.
/// </summary>
public sealed class SendDescriptionOfWorkApprovedNoticeTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private readonly RecordingEmailSender _emailSender;

    public SendDescriptionOfWorkApprovedNoticeTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()))
        => _emailSender = fixture.EmailSender;

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.DescriptionOfWorkApprovedNotifier.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingDescriptionOfWorkApprovedNoticeSent_RecordsThatTheNoticeWasSent()
    {
        var state = new OrderState();
        state.When(StartOrderDraftTestData.DraftStarted1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(Submitted1(GrainId));
        state.When(Approved(GrainId));

        state.When(ApprovedNoticeSent(GrainId));

        Assert.True(state.DescriptionOfWork!.ApprovedNoticeSent);
        Assert.Equal(OrderStatus.DescriptionOfWorkApproved, state.Status);
    }

    [Fact]
    public async Task Send_RecordsTheNoticeSentToTheAssignee()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenApproved();

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand()));

        Then<SendDescriptionOfWorkApprovedNoticeResponse>(response =>
        {
            Assert.Equal(8, response.Version);
            var recorded = Assert.IsType<DescriptionOfWorkApprovedNoticeSent>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(ApprovedNoticeSent(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Send_SucceedsWithoutChangesWhenTheNoticeWasAlreadySent()
    {
        GivenSignedIn(Automation);
        await GivenApproved(ApprovedNoticeSent(GrainId));

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand()));

        Then<SendDescriptionOfWorkApprovedNoticeResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Send_RejectsADescriptionOfWorkThatIsNotApproved()
    {
        GivenSignedIn(Automation);
        await GivenAcknowledged(Submitted1(GrainId));

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnotherDescriptionOfWork()
    {
        GivenSignedIn(Automation);
        await GivenApproved();

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(
            ApprovedNoticeCommand() with { DescriptionOfWorkId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsARecipientTheOrderIsNotAssignedTo()
    {
        GivenSignedIn(Automation);
        await GivenApproved();

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(
            ApprovedNoticeCommand() with { RecipientUserId = TestData.Coordinator1.UserId, RecipientEmail = TestData.Coordinator1.Email }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnInvalidRecipientEmail()
    {
        GivenSignedIn(Automation);
        await GivenApproved();

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand() with { RecipientEmail = "not an email" }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SendDescriptionOfWorkApprovedNoticeCommand.RecipientEmail), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(AutomationUser.DescriptionOfWorkApprovalNotifier.CreatePrincipal(TestData.TenantId));
        await GivenApproved();

        await When(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SendDescriptionOfWorkApprovedNoticeAsync(ApprovedNoticeCommand()));
    }

    // The harness's WhenQuery needs a reference type, so the query tests call the grain directly.

    [Fact]
    public async Task IsSent_IsFalseUntilTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenApproved();

        var sent = await Order.IsDescriptionOfWorkApprovedNoticeSentAsync(DescriptionOfWorkTestData.DescriptionOfWorkId);

        Assert.True(sent.IsSuccess);
        Assert.False(sent.Value);
    }

    [Fact]
    public async Task IsSent_IsTrueOnceTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenApproved(ApprovedNoticeSent(GrainId));

        var sent = await Order.IsDescriptionOfWorkApprovedNoticeSentAsync(DescriptionOfWorkTestData.DescriptionOfWorkId);

        Assert.True(sent.IsSuccess);
        Assert.True(sent.Value);
    }

    [Fact]
    public async Task IsSent_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        var sent = await Order.IsDescriptionOfWorkApprovedNoticeSentAsync(DescriptionOfWorkTestData.DescriptionOfWorkId);

        Assert.True(sent.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, Assert.IsType<RequestError>(sent.Error).Code);
    }

    /// <summary>
    ///     Checks the reaction the harness bypasses: the approval, produced on the order's notification stream as the stream
    ///     provider delivers it, has the Description of Work Approved Notifier email the assignee a link to the staff order
    ///     page and record the notice on the order. It is published signed out, as the stream provider delivers it with no
    ///     signed-in user.
    /// </summary>
    [Fact]
    public async Task DescriptionOfWorkApproved_EmailsTheAssigneeALinkToTheOrderAndRecordsTheNotice()
    {
        var orderNumber = NextOrderNumber();
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await GivenAcknowledged(
            AssignOrderNumberTestData.Assigned1(GrainId, orderNumber),
            Submitted1(GrainId, orderNumber),
            Approved(GrainId, orderNumber: orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Approved(GrainId, orderNumber: orderNumber)));

        var notice = await WaitForNoticeAsync();
        Assert.InRange(notice.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(ApprovedNoticeSent(GrainId, orderNumber, notice.OccurredAt), notice, strict: true);
        var email = Assert.Single(EmailsAbout(orderNumber));
        TenantKey.Split(GrainId, out _, out var orderGuid);
        Assert.Equal(TestData.Director1.Email, email.To);
        Assert.Equal(ApprovedNoticeSubject(orderNumber), email.Subject);
        Assert.Contains("Hi Tommy Jones,", email.HtmlBody);
        Assert.Contains("John Doe approved version 1 of the description of work.", email.HtmlBody);
        Assert.Contains($"<a href=\"https://getrdone.test/staff/orders/{orderGuid}\">", email.HtmlBody);
    }

    private static long _position;
    private static long _orderNumber = AssignOrderNumberTestData.FirstOrderNumber + 70_000;

    /// <summary>A number no other test in the class uses, so each test finds only its own emails.</summary>
    private static long NextOrderNumber() => Interlocked.Increment(ref _orderNumber);

    private IEnumerable<EmailMessage> EmailsAbout(long orderNumber)
        => _emailSender.Sent.Where(email => email.Subject == ApprovedNoticeSubject(orderNumber));

    /// <summary>The order's stream on the notification provider, which the order's notifiers subscribe to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> NotificationStream => Cluster.Client.GetStreamProvider(NotificationReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(NotificationReactors.OrderNamespace, GrainId));

    /// <summary>An order event as the stream provider delivers it from the order's own stream.</summary>
    private static StreamedEvent<object> OnOrderStream(DescriptionOfWorkApproved approved)
    {
        TenantKey.Split(approved.OrderId, out var tenantId, out var key);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.DescriptionOfWorkApproved.V1",
            new StreamName($"order-{approved.OrderId}", NotificationReactors.OrderCategory, tenantId, key!),
            position,
            new StreamedEventCursor("kurrentdb", NotificationReactors.OrderCategory, approved.OrderId, position, 0),
            approved.OccurredAt.UtcDateTime,
            approved);
    }

    /// <summary>Waits until the order has saved the notice, then returns it.</summary>
    private async Task<DescriptionOfWorkApprovedNoticeSent> WaitForNoticeAsync()
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var notices = (await Order.GetSavedEventsSince(0)).OfType<DescriptionOfWorkApprovedNoticeSent>().ToList();
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

    /// <summary>Sets up an order whose description of work ClientUser1 approved, followed by <paramref name="more"/> events.</summary>
    private Task GivenApproved(params OrderEvent[] more) => GivenAcknowledged([Submitted1(GrainId), Approved(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
