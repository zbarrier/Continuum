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
///     Given/When/Then tests for recording the notice of a submitted description of work. Each test uses a new order in the
///     test tenant. The first test checks what applying the slice's event does to the order state and the second checks
///     every value of the event the slice records; the behavior and query tests follow, then the tests of the Description of
///     Work Approval Notifier's reaction to a submission.
/// </summary>
public sealed class SendDescriptionOfWorkSubmittedNoticeTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private readonly RecordingEmailSender _emailSender;

    public SendDescriptionOfWorkSubmittedNoticeTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()))
        => _emailSender = fixture.EmailSender;

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.DescriptionOfWorkApprovalNotifier.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingDescriptionOfWorkSubmittedNoticeSent_RecordsWhichVersionsNoticeWasSent()
    {
        var state = new OrderState();
        state.When(StartOrderDraftTestData.DraftStarted1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(Submitted1(GrainId));

        state.When(SubmittedNoticeSent(GrainId));

        Assert.Equal(1, state.DescriptionOfWork!.NoticeSentVersion);
        Assert.Equal(OrderStatus.DescriptionOfWorkAwaitingApproval, state.Status);
    }

    [Fact]
    public async Task Send_RecordsTheNoticeSentToTheClient()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand()));

        Then<SendDescriptionOfWorkSubmittedNoticeResponse>(response =>
        {
            Assert.Equal(7, response.Version);
            var recorded = Assert.IsType<DescriptionOfWorkSubmittedNoticeSent>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(SubmittedNoticeSent(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Send_SucceedsWithoutChangesWhenTheNoticeWasAlreadySent()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted(SubmittedNoticeSent(GrainId));

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand()));

        Then<SendDescriptionOfWorkSubmittedNoticeResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Send_RecordsTheNoticeOfARevision()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted(SubmittedNoticeSent(GrainId), Submitted(Command2, GrainId));

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand(version: 2)));

        Then<SendDescriptionOfWorkSubmittedNoticeResponse>(response => Assert.Equal(
            2, Assert.IsType<DescriptionOfWorkSubmittedNoticeSent>(SingleEvent(response.Changes)).DescriptionOfWorkVersion));
    }

    [Fact]
    public async Task Send_RecordsTheNoticeOfAVersionThatWasRevisedBeforeItWasRecorded()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted(Submitted(Command2, GrainId));

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand()));

        Then<SendDescriptionOfWorkSubmittedNoticeResponse>(response => Assert.Equal(
            1, Assert.IsType<DescriptionOfWorkSubmittedNoticeSent>(SingleEvent(response.Changes)).DescriptionOfWorkVersion));
    }

    [Theory]
    [InlineData("another description of work")]
    [InlineData("a version not submitted yet")]
    public async Task Send_RejectsAVersionThatWasNotSubmitted(string problem)
    {
        var command = problem == "another description of work"
            ? SubmittedNoticeCommand() with { DescriptionOfWorkId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }
            : SubmittedNoticeCommand(version: 2);
        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(command));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsARecipientWhoDoesNotApproveTheDescriptionOfWork()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(
            SubmittedNoticeCommand() with { RecipientUserId = TestData.ClientUser2.UserId, RecipientEmail = TestData.ClientUser2.Email }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnInvalidRecipientEmail()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand() with { RecipientEmail = "not an email" }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SendDescriptionOfWorkSubmittedNoticeCommand.RecipientEmail), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(AutomationUser.DescriptionOfWorkApprovedNotifier.CreatePrincipal(TestData.TenantId));
        await GivenSubmitted();

        await When(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SendDescriptionOfWorkSubmittedNoticeAsync(SubmittedNoticeCommand()));
    }

    // The harness's WhenQuery needs a reference type, so the query tests call the grain directly.

    [Fact]
    public async Task IsDue_IsTrueUntilTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted();

        var due = await Order.IsDescriptionOfWorkSubmittedNoticeDueAsync(DescriptionOfWorkTestData.DescriptionOfWorkId, 1);

        Assert.True(due.IsSuccess);
        Assert.True(due.Value);
    }

    [Theory]
    [InlineData("its notice was sent")]
    [InlineData("it was revised")]
    [InlineData("it was approved")]
    public async Task IsDue_IsFalseForAVersionWhen(string reason)
    {
        OrderEvent[] more = reason switch
        {
            "its notice was sent" => [SubmittedNoticeSent(GrainId)],
            "it was revised" => [Submitted(Command2, GrainId)],
            _ => [Approved(GrainId)]
        };
        GivenSignedIn(Automation);
        await GivenSubmitted(more);

        var due = await Order.IsDescriptionOfWorkSubmittedNoticeDueAsync(DescriptionOfWorkTestData.DescriptionOfWorkId, 1);

        Assert.True(due.IsSuccess);
        Assert.False(due.Value);
    }

    [Fact]
    public async Task IsDue_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted();

        var due = await Order.IsDescriptionOfWorkSubmittedNoticeDueAsync(DescriptionOfWorkTestData.DescriptionOfWorkId, 1);

        Assert.True(due.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, Assert.IsType<RequestError>(due.Error).Code);
    }

    /// <summary>
    ///     Checks the reaction the harness bypasses: the submission, produced on the order's notification stream as the
    ///     stream provider delivers it, has the Description of Work Approval Notifier email the client a link to the order and
    ///     record the notice on the order. It is published signed out, as the stream provider delivers it with no signed-in
    ///     user.
    /// </summary>
    [Fact]
    public async Task DescriptionOfWorkSubmitted_EmailsTheClientALinkToTheOrderAndRecordsTheNotice()
    {
        var orderNumber = NextOrderNumber();
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await GivenAcknowledged(AssignOrderNumberTestData.Assigned1(GrainId, orderNumber), Submitted1(GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Submitted1(GrainId, orderNumber)));

        var notice = await WaitForNoticeAsync(1);
        Assert.InRange(notice.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(SubmittedNoticeSent(GrainId, 1, orderNumber, notice.OccurredAt), notice, strict: true);
        var email = Assert.Single(EmailsAbout(orderNumber));
        TenantKey.Split(GrainId, out _, out var orderGuid);
        Assert.Equal(TestData.ClientUser1.Email, email.To);
        Assert.Equal(SubmittedNoticeSubject(orderNumber), email.Subject);
        Assert.Contains("Hi John Doe,", email.HtmlBody);
        Assert.Contains("is ready for your approval", email.HtmlBody);
        Assert.Contains($"<a href=\"https://getrdone.test/orders/{orderGuid}\">", email.HtmlBody);
    }

    [Fact]
    public async Task DescriptionOfWorkSubmitted_TellsTheClientARevisionIsReady()
    {
        var orderNumber = NextOrderNumber();
        GivenSignedIn(Automation);
        await GivenAcknowledged(
            AssignOrderNumberTestData.Assigned1(GrainId, orderNumber),
            Submitted1(GrainId, orderNumber),
            SubmittedNoticeSent(GrainId, 1, orderNumber),
            Submitted(Command2, GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Submitted(Command2, GrainId, orderNumber)));

        await WaitForNoticeAsync(2);
        Assert.Contains("was revised (version 2) and is ready for your approval", Assert.Single(EmailsAbout(orderNumber)).HtmlBody);
    }

    /// <summary>
    ///     A version that was revised before its submission is delivered isn't emailed: the client is emailed about the
    ///     revision instead. The revision follows it on the stream; the stream provider delivers one order's events one after
    ///     another, so once the revision's notice is recorded, the delivery before it was handled.
    /// </summary>
    [Fact]
    public async Task DescriptionOfWorkSubmitted_DoesNotEmailTheClientAboutAVersionThatWasRevised()
    {
        var orderNumber = NextOrderNumber();
        GivenSignedIn(Automation);
        await GivenAcknowledged(
            AssignOrderNumberTestData.Assigned1(GrainId, orderNumber),
            Submitted1(GrainId, orderNumber),
            Submitted(Command2, GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Submitted1(GrainId, orderNumber)));
        await NotificationStream.OnNextAsync(OnOrderStream(Submitted(Command2, GrainId, orderNumber)));

        await WaitForNoticeAsync(2);
        Assert.Contains("(version 2)", Assert.Single(EmailsAbout(orderNumber)).HtmlBody);
    }

    private static long _position;
    private static long _orderNumber = AssignOrderNumberTestData.FirstOrderNumber + 60_000;

    /// <summary>A number no other test in the class uses, so each test finds only its own emails.</summary>
    private static long NextOrderNumber() => Interlocked.Increment(ref _orderNumber);

    private IEnumerable<EmailMessage> EmailsAbout(long orderNumber)
        => _emailSender.Sent.Where(email => email.Subject == SubmittedNoticeSubject(orderNumber));

    /// <summary>The order's stream on the notification provider, which the order's notifiers subscribe to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> NotificationStream => Cluster.Client.GetStreamProvider(NotificationReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(NotificationReactors.OrderNamespace, GrainId));

    /// <summary>An order event as the stream provider delivers it from the order's own stream.</summary>
    private static StreamedEvent<object> OnOrderStream(DescriptionOfWorkSubmitted submitted)
    {
        TenantKey.Split(submitted.OrderId, out var tenantId, out var key);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.DescriptionOfWorkSubmitted.V1",
            new StreamName($"order-{submitted.OrderId}", NotificationReactors.OrderCategory, tenantId, key!),
            position,
            new StreamedEventCursor("kurrentdb", NotificationReactors.OrderCategory, submitted.OrderId, position, 0),
            submitted.OccurredAt.UtcDateTime,
            submitted);
    }

    /// <summary>Waits until the order has saved the notice of <paramref name="version"/>, then returns it.</summary>
    private async Task<DescriptionOfWorkSubmittedNoticeSent> WaitForNoticeAsync(int version)
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var notices = (await Order.GetSavedEventsSince(0)).OfType<DescriptionOfWorkSubmittedNoticeSent>()
                .Where(notice => notice.DescriptionOfWorkVersion == version).ToList();
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

    /// <summary>Sets up an acknowledged order with the description of work <see cref="DescriptionOfWorkTestData.Command1"/>, followed by <paramref name="more"/> events.</summary>
    private Task GivenSubmitted(params OrderEvent[] more) => GivenAcknowledged([Submitted1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
