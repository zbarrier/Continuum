using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.TestHarness;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.StartOrderDraftTestData;

namespace GetRDone.Orders.Grains.Tests.ReadModels;

/// <summary>
///     Tests for the staff dashboard's In progress projection. There is one projection per tenant, so the tests of this class
///     share the test tenant's, and each checks only the new orders it projects. The grain is <c>[Authorize]</c>d and
///     <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in first.
/// </summary>
public sealed class InProgressOrdersTests
    : ProjectionGrainTestHarness<IInProgressOrdersGrain, InProgressOrdersGrain, InProgressOrdersState>,
      IClassFixture<OrdersCluster>
{
    private static readonly string TenantGrainKey = TenantKey.Create(TestData.TenantId, ReadModelGrainFactoryExtensions.DefaultKey);

    private static readonly string Director1Id = RequestedByUsers.Director1.GetTenantUserId();

    public InProgressOrdersTests(OrdersCluster fixture) : base(fixture.Cluster, TenantGrainKey) { }

    [Fact]
    public async Task OrderAssigned_PutsTheOrderOnTheListWithItsNumberAndAssignee()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
        [
            .. Submitted(Command1, orderId),
            AssignOrderNumberTestData.Assigned(Command1, orderId, 610_800, TestData.At.AddMinutes(2)),
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(3))
        ]);

        Then(state =>
        {
            Assert.Equal(
                new InProgressOrderEntry(orderId, 610_800, TestData.LakeviewCreditUnionClientId,
                    "48 Birch Hollow Rd, Asheville, NC 28803", Director1Id, StaffOrderStatus.Assigned, DueDate: null),
                state.Orders[orderId]);
            Assert.False(state.Drafts.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task OrderNumberAssigned_AfterTheOrderIsAssigned_GivesTheOrderItsNumber()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
        [
            .. Submitted(Command1, orderId),
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(2)),
            AssignOrderNumberTestData.Assigned(Command1, orderId, 610_801, TestData.At.AddMinutes(3))
        ]);

        Then(state => Assert.Equal(610_801, state.Orders[orderId].OrderNumber));
    }

    [Fact]
    public async Task OrderSubmitted_IsNotOnTheListUntilItIsAssigned()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given([.. Submitted(Command1, orderId), AssignOrderNumberTestData.Assigned(Command1, orderId, 610_802, TestData.At)]);

        Then(state =>
        {
            Assert.False(state.Orders.ContainsKey(orderId));
            Assert.Equal(610_802, state.Drafts[orderId].OrderNumber);
        });
    }

    [Fact]
    public async Task OrderAcknowledged_MovesTheOrderToAcknowledged()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
        [
            .. Submitted(Command1, orderId),
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(2)),
            AcknowledgeOrderTestData.Acknowledged1(orderId, TestData.At.AddMinutes(3))
        ]);

        Then(state => Assert.Equal(StaffOrderStatus.Acknowledged, state.Orders[orderId].Status));
    }

    [Fact]
    public async Task DescriptionOfWorkSubmittedAndApproved_ShowTheOrdersProgress()
    {
        var (submittedOrder, approvedOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
        [
            .. Submitted(Command1, submittedOrder),
            AssignOrderTestData.OrderAssigned1(submittedOrder, TestData.At.AddMinutes(2)),
            AcknowledgeOrderTestData.Acknowledged1(submittedOrder, TestData.At.AddMinutes(3)),
            DescriptionOfWorkTestData.Submitted1(submittedOrder, occurredAt: TestData.At.AddMinutes(4)),
            .. Submitted(Command1, approvedOrder),
            AssignOrderTestData.OrderAssigned1(approvedOrder, TestData.At.AddMinutes(2)),
            AcknowledgeOrderTestData.Acknowledged1(approvedOrder, TestData.At.AddMinutes(3)),
            DescriptionOfWorkTestData.Submitted1(approvedOrder, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(approvedOrder, occurredAt: TestData.At.AddMinutes(5))
        ]);

        Then(state =>
        {
            Assert.Equal(StaffOrderStatus.DescriptionOfWorkAwaitingApproval, state.Orders[submittedOrder].Status);
            Assert.Equal(StaffOrderStatus.DescriptionOfWorkApproved, state.Orders[approvedOrder].Status);
        });
    }

    [Fact]
    public async Task VendorAssigned_ShowsTheOrderIsAssignedToAVendorAndWhenItIsDue()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given([.. VendorAssignedOrder(Command1, orderId, VendorAssignmentTestData.DueDate)]);

        Then(state =>
        {
            Assert.Equal(StaffOrderStatus.VendorAssigned, state.Orders[orderId].Status);
            Assert.Equal(VendorAssignmentTestData.DueDate, state.Orders[orderId].DueDate);
        });
    }

    [Fact]
    public async Task VendorsWork_MovesTheOrderThroughItsStatuses()
    {
        var (scheduledOrder, completedOrder, deliveredOrder) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
        [
            .. VendorAssignedOrder(Command1, scheduledOrder, VendorAssignmentTestData.DueDate),
            InspectionTestData.Scheduled1(scheduledOrder, TestData.At.AddMinutes(8)),
            .. VendorAssignedOrder(Command1, completedOrder, VendorAssignmentTestData.DueDate),
            InspectionTestData.Scheduled1(completedOrder, TestData.At.AddMinutes(8)),
            InspectionTestData.Completed1(completedOrder, TestData.At.AddMinutes(9)),
            .. VendorAssignedOrder(Command1, deliveredOrder, VendorAssignmentTestData.DueDate),
            InspectionTestData.Scheduled1(deliveredOrder, TestData.At.AddMinutes(8)),
            InspectionTestData.Completed1(deliveredOrder, TestData.At.AddMinutes(9)),
            InspectionTestData.Delivered1(deliveredOrder, occurredAt: TestData.At.AddMinutes(10))
        ]);

        Then(state =>
        {
            Assert.Equal(StaffOrderStatus.InspectionScheduled, state.Orders[scheduledOrder].Status);
            Assert.Equal(StaffOrderStatus.InspectionCompleted, state.Orders[completedOrder].Status);
            Assert.Equal(StaffOrderStatus.ReportDelivered, state.Orders[deliveredOrder].Status);
            Assert.Equal(VendorAssignmentTestData.DueDate, state.Orders[deliveredOrder].DueDate);
        });
    }

    [Fact]
    public async Task Orders_ListTheirClientsAndAssigneesNames()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await Given(
        [
            .. Submitted(Command2, order2),
            AssignOrderTestData.OrderAssigned(order2, RequestedByUsers.Coordinator1, RequestedByUsers.Director1, TestData.At.AddMinutes(2)),
            .. Submitted(Command1, order1),
            AssignOrderNumberTestData.Assigned(Command1, order1, 610_810, TestData.At.AddMinutes(2)),
            AssignOrderTestData.OrderAssigned1(order1, TestData.At.AddMinutes(3)),
            AcknowledgeOrderTestData.Acknowledged1(order1, TestData.At.AddMinutes(4))
        ]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<InProgressOrdersPage>(page => Assert.Equal(
        [
            new InProgressOrder(order1, 610_810, "Lakeview Credit Union", "48 Birch Hollow Rd, Asheville, NC 28803",
                Director1Id, "Tommy Jones", StaffOrderStatus.Acknowledged, DueDate: null),
            new InProgressOrder(order2, null, "Harbor National Bank", "715 Commerce Blvd, Greenville, SC 29601",
                RequestedByUsers.Coordinator1.GetTenantUserId(), "Maria Lopez", StaffOrderStatus.Assigned, DueDate: null)
        ], page.Orders.Where(order => order.OrderId == order1 || order.OrderId == order2)));
    }

    [Fact]
    public async Task Orders_ListTheOrdersDueFirstFirst_ThenTheOrdersWithoutADueDateByOrderNumber()
    {
        var (dueLater, dueSooner, numbered, notNumbered) = (NewOrderId(), NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
        [
            .. Submitted(Command2, notNumbered),
            AssignOrderTestData.OrderAssigned1(notNumbered, TestData.At.AddMinutes(2)),
            .. Submitted(Command1, numbered),
            AssignOrderNumberTestData.Assigned(Command1, numbered, 610_820, TestData.At.AddMinutes(2)),
            AssignOrderTestData.OrderAssigned1(numbered, TestData.At.AddMinutes(3)),
            .. VendorAssignedOrder(Command1, dueLater, VendorAssignmentTestData.DueDate.AddDays(3), 610_821),
            .. VendorAssignedOrder(Command3, dueSooner, VendorAssignmentTestData.DueDate, 610_822)
        ]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<InProgressOrdersPage>(page => Assert.Equal(
            [dueSooner, dueLater, numbered, notNumbered],
            page.Orders.Select(order => order.OrderId)
                .Where(orderId => orderId == dueLater || orderId == dueSooner || orderId == numbered || orderId == notNumbered)));
    }

    [Fact]
    public async Task Orders_ArePaged()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
        [
            .. Submitted(Command1, order1),
            AssignOrderTestData.OrderAssigned1(order1, TestData.At.AddMinutes(2)),
            .. Submitted(Command2, order2),
            AssignOrderTestData.OrderAssigned1(order2, TestData.At.AddMinutes(2))
        ]);
        var all = (await Grain.GetOrdersAsync(0, int.MaxValue)).Value;

        await When(() => Grain.GetOrdersAsync(1, 1));

        Then<InProgressOrdersPage>(page =>
        {
            Assert.Equal(all.Orders[1], Assert.Single(page.Orders));
            Assert.Equal(all.TotalCount, page.TotalCount);
        });
    }

    [Fact]
    public async Task Orders_CanOnlyBeReadByTheStaff()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);

        await When(() => Grain.GetOrdersAsync(0, 10));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    public async Task Orders_RejectAnInvalidPage(int skip, int take)
    {
        GivenSignedIn(RequestedByUsers.Director1);

        await When(() => Grain.GetOrdersAsync(skip, take));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    /// <summary>
    ///     Checks the stream wiring the harness bypasses: events produced on the tenant's orders stream, as the stream provider
    ///     does for the stream target the order grain names, reach the tenant's projection through its implicit subscription.
    ///     Events are published signed out, as the stream provider delivers them with no signed-in user, to check that
    ///     delivery isn't blocked by the grain's <c>[Authorize]</c>.
    /// </summary>
    [Fact]
    public async Task Orders_AreProjectedFromTheTenantsOrdersStream()
    {
        var orderId = NewOrderId();
        RequestContext.Clear();
        var stream = Cluster.Client.GetStreamProvider(OrderProjections.ProviderName)
            .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.TenantOrdersNamespace, TenantGrainKey));

        var started = DraftStarted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At);
        var assigned = AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderAssigned.V1", 1, assigned, assigned.OccurredAt));

        GivenSignedIn(RequestedByUsers.Director1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetOrdersAsync(0, int.MaxValue)).Value.Orders.Any(order => order.OrderId == orderId))
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>The events of a draft started with <paramref name="draft"/> and submitted.</summary>
    private static object[] Submitted(StartOrderDraftCommand draft, string orderId) =>
    [
        DraftStarted(draft, orderId, RequestedByUsers.ClientUser1, TestData.At),
        SubmitOrderTestData.Submitted(draft, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1))
    ];

    /// <summary>
    ///     The events of a draft started with <paramref name="draft"/>, submitted, given <paramref name="orderNumber"/> when
    ///     there is one, worked by Director1 and assigned to a vendor with the client's due date <paramref name="dueDate"/>.
    /// </summary>
    private static object[] VendorAssignedOrder(StartOrderDraftCommand draft, string orderId, DateOnly dueDate, long? orderNumber = null)
    {
        var assigned = VendorAssignmentTestData.Assigned1(orderId, orderNumber, TestData.At.AddMinutes(7));
        object[] numbered = orderNumber is { } number
            ? [AssignOrderNumberTestData.Assigned(draft, orderId, number, TestData.At.AddMinutes(2))]
            : [];
        return
        [
            .. Submitted(draft, orderId),
            .. numbered,
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(3)),
            AcknowledgeOrderTestData.Acknowledged1(orderId, TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Submitted1(orderId, orderNumber, TestData.At.AddMinutes(5)),
            DescriptionOfWorkTestData.Approved(orderId, orderNumber: orderNumber, occurredAt: TestData.At.AddMinutes(6)),
            new VendorAssigned
            {
                OrderId = assigned.OrderId,
                VendorId = assigned.VendorId,
                VendorFee = assigned.VendorFee,
                VendorDueDate = assigned.VendorDueDate,
                ServiceFee = assigned.ServiceFee,
                DueDate = dueDate,
                DescriptionOfWorkId = assigned.DescriptionOfWorkId,
                DescriptionOfWorkVersion = assigned.DescriptionOfWorkVersion,
                OrderNumber = assigned.OrderNumber,
                InitiatedBy = assigned.InitiatedBy,
                OccurredAt = assigned.OccurredAt
            }
        ];
    }

    /// <summary>An order event as the stream provider delivers it, read from the order's stream at <paramref name="revision"/>.</summary>
    private static StreamedEvent<object> OnOrderStream(
        string orderId, string eventType, ulong revision, object evt, DateTimeOffset occurredAt)
    {
        TenantKey.Split(orderId, out var tenantId, out var key);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            eventType,
            new StreamName($"order-{orderId}", "order", tenantId, key),
            revision,
            new StreamedEventCursor("kurrentdb", "order", orderId, NextPosition(), 0),
            occurredAt.UtcDateTime,
            evt);
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
