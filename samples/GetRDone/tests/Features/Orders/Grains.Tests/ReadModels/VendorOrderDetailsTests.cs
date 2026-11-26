using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.TestHarness;

using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.VendorAssignmentTestData;

namespace GetRDone.Orders.Grains.Tests.ReadModels;

/// <summary>
///     Tests for the vendor order page's VendorOrderDetails projection. Each test projects a new order, so it starts with
///     nothing. The grain is <c>[Authorize]</c>d and <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in
///     first.
/// </summary>
public sealed class VendorOrderDetailsTests
    : ProjectionGrainTestHarness<IVendorOrderDetailsGrain, VendorOrderDetailsGrain, VendorOrderDetailsState>,
      IClassFixture<OrdersCluster>
{
    private readonly string _orderId;

    public VendorOrderDetailsTests(OrdersCluster fixture) : this(fixture, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private VendorOrderDetailsTests(OrdersCluster fixture, string orderId) : base(fixture.Cluster, orderId) => _orderId = orderId;

    [Fact]
    public async Task VendorOrderDetails_ShowTheVendorWhatItNeedsAndTheTimelineSinceTheAssignment()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned(AssignOrderNumberTestData.Assigned1(_orderId, 610_950, TestData.At.AddMinutes(7)));

        await When(() => Grain.GetVendorOrderDetailsAsync());

        Then<VendorOrderDetails>(details => Assert.Equivalent(new VendorOrderDetails(
            _orderId,
            610_950,
            VendorOrderStatus.Assigned,
            "Lakeview Credit Union",
            TestData.UniformResidentialAppraisal.FormTypeName,
            "48 Birch Hollow Rd, Asheville, NC 28803",
            "Marcus Doyle",
            "8285550142",
            425.00m,
            VendorDueDate,
            new DescriptionOfWorkSummary(
                DescriptionOfWorkTestData.DescriptionOfWorkId, 1, DescriptionOfWorkTestData.Command1.DescriptionOfWorkHtml,
                Approved: true, TestData.At.AddMinutes(4)),
            null,
            null,
            [
                new OrderTimelineEntry(TestData.At.AddMinutes(7), "Order number 610950"),
                new OrderTimelineEntry(TestData.At.AddMinutes(6), "Assigned to you by Summit Valuation")
            ],
            VendorOrderActions.ScheduleInspection), details, strict: true));
    }

    [Fact]
    public async Task VendorOrderDetails_ShowTheScheduledInspectionAndLetTheVendorCompleteIt()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned(InspectionTestData.Scheduled1(_orderId, TestData.At.AddMinutes(7)));

        await When(() => Grain.GetVendorOrderDetailsAsync());

        Then<VendorOrderDetails>(details =>
        {
            Assert.Equal(VendorOrderStatus.InspectionScheduled, details.Status);
            Assert.Equal(new OrderInspectionSummary(InspectionTestData.InspectionTime, "Marcus Doyle", CompletedOn: null), details.Inspection);
            Assert.Null(details.Report);
            Assert.Equal(VendorOrderActions.CompleteInspection, details.AllowedActions);
            Assert.Equal(
                new OrderTimelineEntry(TestData.At.AddMinutes(7), "Inspection scheduled by Ryan Brooks"), details.Timeline[0]);
        });
    }

    [Fact]
    public async Task VendorOrderDetails_ShowTheCompletedInspectionAndLetTheVendorDeliverTheReport()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned(
            InspectionTestData.Scheduled1(_orderId, TestData.At.AddMinutes(7)),
            InspectionTestData.Completed1(_orderId, TestData.At.AddMinutes(8)));

        await When(() => Grain.GetVendorOrderDetailsAsync());

        Then<VendorOrderDetails>(details =>
        {
            Assert.Equal(VendorOrderStatus.InspectionCompleted, details.Status);
            Assert.Equal(InspectionTestData.CompleteCommand1.CompletedOn, details.Inspection!.CompletedOn);
            Assert.Equal(VendorOrderActions.DeliverReport, details.AllowedActions);
            Assert.Equal(
                new OrderTimelineEntry(TestData.At.AddMinutes(8), "Inspection completed by Ryan Brooks"), details.Timeline[0]);
        });
    }

    [Fact]
    public async Task VendorOrderDetails_ShowTheDeliveredReportAndAllowNothingMore()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned(
            InspectionTestData.Scheduled1(_orderId, TestData.At.AddMinutes(7)),
            InspectionTestData.Completed1(_orderId, TestData.At.AddMinutes(8)),
            InspectionTestData.Delivered1(_orderId, occurredAt: TestData.At.AddMinutes(9)));

        await When(() => Grain.GetVendorOrderDetailsAsync());

        Then<VendorOrderDetails>(details =>
        {
            Assert.Equal(VendorOrderStatus.ReportDelivered, details.Status);
            Assert.Equal(
                new OrderReportSummary(
                    InspectionTestData.DeliverCommand1.DocumentId, InspectionTestData.DeliverCommand1.FileName, TestData.At.AddMinutes(9)),
                details.Report);
            Assert.Equal(VendorOrderActions.None, details.AllowedActions);
            Assert.Equal(new OrderTimelineEntry(TestData.At.AddMinutes(9), "Report delivered by Ryan Brooks"), details.Timeline[0]);
        });
    }

    [Fact]
    public async Task VendorOrderDetails_CanBeReadByAnyUserOfTheVendor()
    {
        GivenSignedIn(RequestedByUsers.VendorUser2);
        await GivenAssigned();

        await When(() => Grain.GetVendorOrderDetailsAsync());

        Then<VendorOrderDetails>(details => Assert.Equal(_orderId, details.OrderId));
    }

    [Fact]
    public async Task VendorOrderDetails_AreNotFoundUntilTheOrderIsAssignedToAVendor()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenApproved();

        await When(() => Grain.GetVendorOrderDetailsAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.OtherVendorUser1))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task VendorOrderDetails_CanOnlyBeReadByTheUsersOfTheAssignedVendor(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenAssigned();

        await When(() => Grain.GetVendorOrderDetailsAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    /// <summary>
    ///     Checks the stream wiring the harness bypasses: events produced on the order's own stream reach the order's
    ///     vendor projection through its implicit subscription. Events are published signed out, as the stream provider
    ///     delivers them with no signed-in user, to check that delivery isn't blocked by the grain's <c>[Authorize]</c>.
    /// </summary>
    [Fact]
    public async Task VendorOrderDetails_AreProjectedFromTheOrdersStream()
    {
        RequestContext.Clear();
        var stream = Cluster.Client.GetStreamProvider(OrderProjections.ProviderName)
            .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.OrderEventsNamespace, _orderId));

        var started = StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At);
        var submitted = DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(1));
        var assigned = Assigned1(_orderId, occurredAt: TestData.At.AddMinutes(2));
        await stream.OnNextAsync(OnOrderStream("Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream("Orders.DescriptionOfWorkSubmitted.V1", 1, submitted, submitted.OccurredAt));
        await stream.OnNextAsync(OnOrderStream("Orders.VendorAssigned.V1", 2, assigned, assigned.OccurredAt));

        GivenSignedIn(RequestedByUsers.VendorUser1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetVendorOrderDetailsAsync()).IsSuccess)
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.OtherVendorUser1) => RequestedByUsers.OtherVendorUser1,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>
    ///     Sets up the order <c>Submitted1</c> submits, assigned to and acknowledged by Director1, whose description of work
    ///     ClientUser1 approved, followed by <paramref name="more"/> events.
    /// </summary>
    private Task GivenApproved(params object[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At),
        SaveBorrowerAndAccessContactTestData.Saved1(_orderId, TestData.At),
        SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)),
        AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(2)),
        AcknowledgeOrderTestData.Acknowledged1(_orderId, TestData.At.AddMinutes(3)),
        DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
        DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)),
        .. more
    ]);

    /// <summary>Sets up the approved order Director1 assigned to Ridge Appraisal Group, followed by <paramref name="more"/> events.</summary>
    private Task GivenAssigned(params object[] more) => GivenApproved([Assigned1(_orderId, occurredAt: TestData.At.AddMinutes(6)), .. more]);

    /// <summary>An event of the order as the stream provider delivers it, read from the order's stream at <paramref name="revision"/>.</summary>
    private StreamedEvent<object> OnOrderStream(string eventType, ulong revision, object evt, DateTimeOffset occurredAt)
    {
        TenantKey.Split(_orderId, out var tenantId, out var key);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            eventType,
            new StreamName($"order-{_orderId}", "order", tenantId, key),
            revision,
            new StreamedEventCursor("kurrentdb", "order", _orderId, NextPosition(), 0),
            occurredAt.UtcDateTime,
            evt);
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
