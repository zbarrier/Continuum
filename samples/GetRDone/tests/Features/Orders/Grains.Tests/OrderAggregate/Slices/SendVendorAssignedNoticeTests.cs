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

using static GetRDone.Orders.Grains.Tests.Data.VendorAssignmentTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for recording the notice of a vendor assignment. Each test uses a new order in the test tenant.
///     The first test checks what applying the slice's event does to the order state and the second checks every value of
///     the event the slice records; the behavior and query tests follow, then the tests of the Vendor Assignment Notifier's
///     reaction to an assignment.
/// </summary>
public sealed class SendVendorAssignedNoticeTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private readonly RecordingEmailSender _emailSender;

    public SendVendorAssignedNoticeTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()))
        => _emailSender = fixture.EmailSender;

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.VendorAssignmentNotifier.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingVendorAssignedNoticeSent_RecordsThatTheNoticeWasSent()
    {
        var state = new OrderState();
        state.When(StartOrderDraftTestData.DraftStarted1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(DescriptionOfWorkTestData.Submitted1(GrainId));
        state.When(DescriptionOfWorkTestData.Approved(GrainId));
        state.When(Assigned1(GrainId));

        state.When(NoticeSent(GrainId));

        Assert.Equal(new OrderVendorAssignment(TestData.RidgeAppraisalGroupVendorId, NoticeSent: true), state.VendorAssignment);
        Assert.Equal(OrderStatus.VendorAssigned, state.Status);
    }

    [Fact]
    public async Task Send_RecordsTheNoticeSentToTheVendorsUsers()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenAssigned();

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand()));

        Then<SendVendorAssignedNoticeResponse>(response =>
        {
            Assert.Equal(9, response.Version);
            var recorded = Assert.IsType<VendorAssignedNoticeSent>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(NoticeSent(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Send_SucceedsWithoutChangesWhenTheNoticeWasAlreadySent()
    {
        GivenSignedIn(Automation);
        await GivenAssigned(NoticeSent(GrainId));

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand()));

        Then<SendVendorAssignedNoticeResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatIsNotAssignedToAVendor()
    {
        GivenSignedIn(Automation);
        await GivenAcknowledged(DescriptionOfWorkTestData.Submitted1(GrainId), DescriptionOfWorkTestData.Approved(GrainId));

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnotherVendor()
    {
        GivenSignedIn(Automation);
        await GivenAssigned();

        await When(() => Order.SendVendorAssignedNoticeAsync(
            NoticeCommand() with { VendorId = TestData.CedarPointAppraisalsVendorId }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsARecipientWhoIsNotAUserOfTheVendor()
    {
        GivenSignedIn(Automation);
        await GivenAssigned();

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand() with
        {
            Recipients = [.. Recipients, new VendorNoticeRecipient(TestData.OtherVendorUser1.UserId, TestData.OtherVendorUser1.Email)]
        }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Send_RejectsANoticeWithoutRecipients()
    {
        GivenSignedIn(Automation);
        await GivenAssigned();

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand() with { Recipients = [] }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SendVendorAssignedNoticeCommand.Recipients), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAnInvalidRecipientEmail()
    {
        GivenSignedIn(Automation);
        await GivenAssigned();

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand() with
        {
            Recipients = [Recipients[0], Recipients[1] with { Email = "not an email" }]
        }));

        ThenError<ValidationError>(error => Assert.Equal("Recipients[1].Email", Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Send_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(AutomationUser.DescriptionOfWorkApprovedNotifier.CreatePrincipal(TestData.TenantId));
        await GivenAssigned();

        await When(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Send_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SendVendorAssignedNoticeAsync(NoticeCommand()));
    }

    // The harness's WhenQuery needs a reference type, so the query tests call the grain directly.

    [Fact]
    public async Task IsSent_IsFalseUntilTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenAssigned();

        var sent = await Order.IsVendorAssignedNoticeSentAsync(TestData.RidgeAppraisalGroupVendorId);

        Assert.True(sent.IsSuccess);
        Assert.False(sent.Value);
    }

    [Fact]
    public async Task IsSent_IsTrueOnceTheNoticeIsSent()
    {
        GivenSignedIn(Automation);
        await GivenAssigned(NoticeSent(GrainId));

        var sent = await Order.IsVendorAssignedNoticeSentAsync(TestData.RidgeAppraisalGroupVendorId);

        Assert.True(sent.IsSuccess);
        Assert.True(sent.Value);
    }

    [Fact]
    public async Task IsSent_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAssigned();

        var sent = await Order.IsVendorAssignedNoticeSentAsync(TestData.RidgeAppraisalGroupVendorId);

        Assert.True(sent.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, Assert.IsType<RequestError>(sent.Error).Code);
    }

    /// <summary>
    ///     Checks the reaction the harness bypasses: the assignment, produced on the order's notification stream as the stream
    ///     provider delivers it, has the Vendor Assignment Notifier email each of the vendor's users a link to the vendor order
    ///     page and record the notice on the order. It is published signed out, as the stream provider delivers it with no
    ///     signed-in user.
    /// </summary>
    [Fact]
    public async Task VendorAssigned_EmailsTheVendorsUsersALinkToTheOrderAndRecordsTheNotice()
    {
        var orderNumber = NextOrderNumber();
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await GivenAcknowledged(
            AssignOrderNumberTestData.Assigned1(GrainId, orderNumber),
            DescriptionOfWorkTestData.Submitted1(GrainId, orderNumber),
            DescriptionOfWorkTestData.Approved(GrainId, orderNumber: orderNumber),
            Assigned1(GrainId, orderNumber));
        RequestContext.Clear();

        await NotificationStream.OnNextAsync(OnOrderStream(Assigned1(GrainId, orderNumber)));

        var notice = await WaitForNoticeAsync();
        Assert.InRange(notice.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(NoticeSent(GrainId, orderNumber, notice.OccurredAt), notice, strict: true);
        var emails = EmailsAbout(orderNumber).ToList();
        Assert.Equal([TestData.VendorUser1.Email, TestData.VendorUser2.Email], emails.Select(email => email.To));
        TenantKey.Split(GrainId, out _, out var orderGuid);
        var email = emails[0];
        Assert.Equal($"New order {orderNumber} from Summit Valuation", email.Subject);
        Assert.Contains("Hi Ryan Brooks,", email.HtmlBody);
        Assert.Contains("Summit Valuation assigned an order to you, due December 4, 2099.", email.HtmlBody);
        Assert.Contains($"<a href=\"https://getrdone.test/vendor/orders/{orderGuid}\">", email.HtmlBody);
        Assert.Contains("Hi Lena Brooks,", emails[1].HtmlBody);
    }

    private static long _position;
    private static long _orderNumber = AssignOrderNumberTestData.FirstOrderNumber + 80_000;

    /// <summary>A number no other test in the class uses, so each test finds only its own emails.</summary>
    private static long NextOrderNumber() => Interlocked.Increment(ref _orderNumber);

    private IEnumerable<EmailMessage> EmailsAbout(long orderNumber)
        => _emailSender.Sent.Where(email => email.Subject == NoticeSubject(orderNumber));

    /// <summary>The order's stream on the notification provider, which the order's notifiers subscribe to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> NotificationStream => Cluster.Client.GetStreamProvider(NotificationReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(NotificationReactors.OrderNamespace, GrainId));

    /// <summary>An order event as the stream provider delivers it from the order's own stream.</summary>
    private static StreamedEvent<object> OnOrderStream(VendorAssigned assigned)
    {
        TenantKey.Split(assigned.OrderId, out var tenantId, out var key);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.VendorAssigned.V1",
            new StreamName($"order-{assigned.OrderId}", NotificationReactors.OrderCategory, tenantId, key!),
            position,
            new StreamedEventCursor("kurrentdb", NotificationReactors.OrderCategory, assigned.OrderId, position, 0),
            assigned.OccurredAt.UtcDateTime,
            assigned);
    }

    /// <summary>Waits until the order has saved the notice, then returns it.</summary>
    private async Task<VendorAssignedNoticeSent> WaitForNoticeAsync()
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var notices = (await Order.GetSavedEventsSince(0)).OfType<VendorAssignedNoticeSent>().ToList();
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

    /// <summary>Sets up an order Director1 assigned to Ridge Appraisal Group, followed by <paramref name="more"/> events.</summary>
    private Task GivenAssigned(params OrderEvent[] more) => GivenAcknowledged(
        [DescriptionOfWorkTestData.Submitted1(GrainId), DescriptionOfWorkTestData.Approved(GrainId), Assigned1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
