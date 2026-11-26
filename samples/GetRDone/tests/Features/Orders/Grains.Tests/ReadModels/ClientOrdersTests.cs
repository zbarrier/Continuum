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
///     Tests for the ClientOrders projection. Each test projects the orders of a new client user, so it starts with none.
///     The grain is <c>[Authorize]</c>d and <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in first.
/// </summary>
public sealed class ClientOrdersTests
    : ProjectionGrainTestHarness<IClientOrdersGrain, ClientOrdersGrain, ClientOrdersState>,
      IClassFixture<OrdersCluster>
{
    private readonly ClaimsPrincipal _user;

    public ClientOrdersTests(OrdersCluster fixture) : this(fixture, RequestedByUsers.NewClientUser()) { }

    private ClientOrdersTests(OrdersCluster fixture, ClaimsPrincipal user) : base(fixture.Cluster, user.GetTenantUserId())
        => _user = user;

    [Fact]
    public async Task OrderSubmitted_AddsTheOrderWithoutANumber()
    {
        var orderId = NewOrderId();
        GivenSignedIn(_user);

        await Given(StartedAndSubmitted(Command1, orderId, TestData.At.AddMinutes(3)));

        Then(state =>
        {
            Assert.Equal(
                new Dictionary<string, ClientOrder>
                {
                    [orderId] = new(
                        orderId, null, "48 Birch Hollow Rd, Asheville, NC 28803", TestData.UniformResidentialAppraisal.FormTypeName,
                        ClientOrderStatus.Submitted, TestData.At.AddMinutes(3))
                },
                state.Orders);
            Assert.Empty(state.Drafts);
        });
    }

    [Fact]
    public async Task OrderNumberAssigned_GivesTheOrderItsNumber()
    {
        var (numberedOrder, otherOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(_user);

        await Given(
        [
            .. StartedAndSubmitted(Command1, numberedOrder, TestData.At.AddMinutes(3)),
            .. StartedAndSubmitted(Command2, otherOrder, TestData.At.AddMinutes(4)),
            AssignOrderNumberTestData.Assigned(Command1, numberedOrder, 610_696, TestData.At.AddMinutes(5))
        ]);

        Then(state =>
        {
            Assert.Equal(610_696, state.Orders[numberedOrder].OrderNumber);
            Assert.Null(state.Orders[otherOrder].OrderNumber);
        });
    }

    [Fact]
    public async Task OrderAssignedAndAcknowledged_ShowTheOrdersProgress()
    {
        var (assignedOrder, acknowledgedOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(_user);

        await Given(
        [
            .. StartedAndSubmitted(Command1, assignedOrder, TestData.At.AddMinutes(3)),
            .. StartedAndSubmitted(Command2, acknowledgedOrder, TestData.At.AddMinutes(4)),
            AssignOrderTestData.OrderAssigned1(assignedOrder, TestData.At.AddMinutes(5)),
            AssignOrderTestData.OrderAssigned1(acknowledgedOrder, TestData.At.AddMinutes(6)),
            AcknowledgeOrderTestData.Acknowledged1(acknowledgedOrder, TestData.At.AddMinutes(7))
        ]);

        Then(state =>
        {
            Assert.Equal(ClientOrderStatus.Assigned, state.Orders[assignedOrder].Status);
            Assert.Equal(ClientOrderStatus.Acknowledged, state.Orders[acknowledgedOrder].Status);
        });
    }

    [Fact]
    public async Task DescriptionOfWorkSubmittedAndApproved_ShowTheOrdersProgress()
    {
        var (submittedOrder, approvedOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(_user);

        await Given(
        [
            .. StartedAndSubmitted(Command1, submittedOrder, TestData.At.AddMinutes(3)),
            .. StartedAndSubmitted(Command2, approvedOrder, TestData.At.AddMinutes(4)),
            AssignOrderTestData.OrderAssigned1(submittedOrder, TestData.At.AddMinutes(5)),
            AssignOrderTestData.OrderAssigned1(approvedOrder, TestData.At.AddMinutes(6)),
            AcknowledgeOrderTestData.Acknowledged1(submittedOrder, TestData.At.AddMinutes(7)),
            AcknowledgeOrderTestData.Acknowledged1(approvedOrder, TestData.At.AddMinutes(8)),
            DescriptionOfWorkTestData.Submitted1(submittedOrder, occurredAt: TestData.At.AddMinutes(9)),
            DescriptionOfWorkTestData.Submitted1(approvedOrder, occurredAt: TestData.At.AddMinutes(10)),
            DescriptionOfWorkTestData.Approved(approvedOrder, occurredAt: TestData.At.AddMinutes(11))
        ]);

        Then(state =>
        {
            Assert.Equal(ClientOrderStatus.DescriptionOfWorkAwaitingApproval, state.Orders[submittedOrder].Status);
            Assert.Equal(ClientOrderStatus.DescriptionOfWorkApproved, state.Orders[approvedOrder].Status);
        });
    }

    [Fact]
    public async Task VendorAssigned_ShowsTheAppraiserIsAssigned()
    {
        var orderId = NewOrderId();
        GivenSignedIn(_user);

        await Given(
        [
            .. StartedAndSubmitted(Command1, orderId, TestData.At.AddMinutes(3)),
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(4)),
            AcknowledgeOrderTestData.Acknowledged1(orderId, TestData.At.AddMinutes(5)),
            DescriptionOfWorkTestData.Submitted1(orderId, occurredAt: TestData.At.AddMinutes(6)),
            DescriptionOfWorkTestData.Approved(orderId, occurredAt: TestData.At.AddMinutes(7)),
            VendorAssignmentTestData.Assigned1(orderId, occurredAt: TestData.At.AddMinutes(8))
        ]);

        Then(state => Assert.Equal(ClientOrderStatus.VendorAssigned, state.Orders[orderId].Status));
    }

    [Fact]
    public async Task Inspection_ShowsItsProgressButNotTheReportUntilTheOrderShips()
    {
        var (scheduledOrder, deliveredOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(_user);

        await Given(
        [
            .. VendorAssigned(scheduledOrder),
            InspectionTestData.Scheduled1(scheduledOrder, TestData.At.AddMinutes(9)),
            .. VendorAssigned(deliveredOrder),
            InspectionTestData.Scheduled1(deliveredOrder, TestData.At.AddMinutes(9)),
            InspectionTestData.Completed1(deliveredOrder, TestData.At.AddMinutes(10)),
            InspectionTestData.Delivered1(deliveredOrder, occurredAt: TestData.At.AddMinutes(11))
        ]);

        Then(state =>
        {
            Assert.Equal(ClientOrderStatus.InspectionScheduled, state.Orders[scheduledOrder].Status);
            Assert.Equal(ClientOrderStatus.InspectionCompleted, state.Orders[deliveredOrder].Status);
        });
    }

    [Fact]
    public async Task OrderShipped_ShowsTheOrderIsShipped()
    {
        var orderId = NewOrderId();
        GivenSignedIn(_user);

        await Given(
        [
            .. VendorAssigned(orderId),
            InspectionTestData.Scheduled1(orderId, TestData.At.AddMinutes(9)),
            InspectionTestData.Completed1(orderId, TestData.At.AddMinutes(10)),
            InspectionTestData.Delivered1(orderId, occurredAt: TestData.At.AddMinutes(11)),
            ShippingTestData.Shipped1(orderId, occurredAt: TestData.At.AddMinutes(12))
        ]);

        Then(state => Assert.Equal(ClientOrderStatus.Shipped, state.Orders[orderId].Status));
    }

    private object[] VendorAssigned(string orderId) =>
    [
        .. StartedAndSubmitted(Command1, orderId, TestData.At.AddMinutes(3)),
        AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(4)),
        AcknowledgeOrderTestData.Acknowledged1(orderId, TestData.At.AddMinutes(5)),
        DescriptionOfWorkTestData.Submitted1(orderId, occurredAt: TestData.At.AddMinutes(6)),
        DescriptionOfWorkTestData.Approved(orderId, occurredAt: TestData.At.AddMinutes(7)),
        VendorAssignmentTestData.Assigned1(orderId, occurredAt: TestData.At.AddMinutes(8))
    ];

    [Fact]
    public async Task DraftStarted_IsNotAnOrderUntilItIsSubmitted()
    {
        var (submittedOrder, draftOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(_user);

        await Given(
            DraftStarted(Command1, submittedOrder, _user, TestData.At),
            DraftStarted(Command2, draftOrder, _user, TestData.At.AddMinutes(1)),
            SubmitOrderTestData.Submitted(Command1, submittedOrder, _user, TestData.At.AddMinutes(2)));

        Then(state =>
        {
            Assert.Equal([submittedOrder], state.Orders.Keys);
            Assert.Equal([draftOrder], state.Drafts.Keys);
        });
    }

    [Fact]
    public async Task RequestAndPropertySaved_BeforeSubmitting_ChangesTheOrdersAddressAndFormType()
    {
        var orderId = NewOrderId();
        GivenSignedIn(_user);

        await Given(
            DraftStarted(Command3, orderId, _user, TestData.At),
            SaveRequestAndPropertyTestData.Saved(
                SaveRequestAndPropertyTestData.Command3 with { PropertyStreet = "11 Laurel Ridge Ct" },
                orderId, _user, TestData.At.AddMinutes(5)),
            SubmitOrderTestData.Submitted(Command3, orderId, _user, TestData.At.AddMinutes(6)));

        Then(state =>
        {
            var order = state.Orders[orderId];
            Assert.Equal("11 Laurel Ridge Ct, Black Mountain, NC 28711", order.PropertyAddress);
            Assert.Equal(TestData.UniformResidentialAppraisal.FormTypeName, order.FormTypeName);
        });
    }

    [Fact]
    public async Task Orders_ListTheUsersOrdersMostRecentlySubmittedFirst()
    {
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(_user);
        await Given(
        [
            .. StartedAndSubmitted(Command1, order1, TestData.At.AddHours(1)),
            .. StartedAndSubmitted(Command2, order2, TestData.At.AddHours(3)),
            .. StartedAndSubmitted(Command3, order3, TestData.At.AddHours(2))
        ]);

        await When(() => Grain.GetOrdersAsync(0, 10));

        Then<ClientOrdersPage>(page =>
        {
            Assert.Equal(
            [
                new ClientOrder(
                    order2, null, "715 Commerce Blvd, Greenville, SC 29601", TestData.RestrictedAppraisal.FormTypeName,
                    ClientOrderStatus.Submitted, TestData.At.AddHours(3)),
                new ClientOrder(
                    order3, null, "9 Laurel Ridge Ct, Black Mountain, NC 28711", TestData.DesktopAppraisal.FormTypeName,
                    ClientOrderStatus.Submitted, TestData.At.AddHours(2)),
                new ClientOrder(
                    order1, null, "48 Birch Hollow Rd, Asheville, NC 28803", TestData.UniformResidentialAppraisal.FormTypeName,
                    ClientOrderStatus.Submitted, TestData.At.AddHours(1))
            ], page.Orders);
            Assert.Equal(3, page.TotalCount);
        });
    }

    [Fact]
    public async Task Orders_ArePaged()
    {
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(_user);
        await Given(
        [
            .. StartedAndSubmitted(Command1, order1, TestData.At.AddHours(1)),
            .. StartedAndSubmitted(Command2, order2, TestData.At.AddHours(3)),
            .. StartedAndSubmitted(Command3, order3, TestData.At.AddHours(2))
        ]);

        await When(() => Grain.GetOrdersAsync(1, 1));

        Then<ClientOrdersPage>(page =>
        {
            Assert.Equal(order3, Assert.Single(page.Orders).OrderId);
            Assert.Equal(3, page.TotalCount);
        });
    }

    [Fact]
    public async Task Orders_CanOnlyBeReadByTheirUser()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartedAndSubmitted(Command1, NewOrderId(), TestData.At.AddMinutes(1)));

        await When(() => Grain.GetOrdersAsync(0, 10));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    public async Task Orders_RejectAnInvalidPage(int skip, int take)
    {
        GivenSignedIn(_user);

        await When(() => Grain.GetOrdersAsync(skip, take));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    /// <summary>
    ///     Checks the stream wiring the harness bypasses: events produced on the user's client orders stream, as the stream
    ///     provider does for the stream target the order grain names, reach the user's projection through its implicit
    ///     subscription. Events are published signed out, as the stream provider delivers them with no signed-in user, to
    ///     check that delivery isn't blocked by the grain's <c>[Authorize]</c>.
    /// </summary>
    [Fact]
    public async Task Orders_AreProjectedFromTheUsersClientOrdersStream()
    {
        var orderId = NewOrderId();
        RequestContext.Clear();
        var stream = Cluster.Client.GetStreamProvider(OrderProjections.ProviderName)
            .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.ClientUserOrdersNamespace, _user.GetTenantUserId()));

        var started = DraftStarted(Command1, orderId, _user, TestData.At);
        var submitted = SubmitOrderTestData.Submitted(Command1, orderId, _user, TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderSubmitted.V1", 1, submitted, submitted.OccurredAt));

        GivenSignedIn(_user);
        var page = await WaitForOrdersAsync(totalCount: 1);
        Assert.Equal(orderId, Assert.Single(page.Orders).OrderId);
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>The events of a draft <see cref="_user"/> started with <paramref name="draft"/> and then submitted.</summary>
    private object[] StartedAndSubmitted(StartOrderDraftCommand draft, string orderId, DateTimeOffset submittedAt) =>
    [
        DraftStarted(draft, orderId, _user, TestData.At),
        SubmitOrderTestData.Submitted(draft, orderId, _user, submittedAt)
    ];

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

    /// <summary>Waits until the projection has applied <paramref name="totalCount"/> orders, then returns them all.</summary>
    private async Task<ClientOrdersPage> WaitForOrdersAsync(int totalCount)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var result = await Grain.GetOrdersAsync(0, int.MaxValue);
            Assert.True(result.IsSuccess);
            if (result.Value.TotalCount >= totalCount)
            {
                Assert.Equal(totalCount, result.Value.TotalCount);
                return result.Value;
            }
            await Task.Delay(50, timeout.Token);
        }
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
