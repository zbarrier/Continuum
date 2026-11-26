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
///     Tests for the staff dashboard's Unassigned projection. There is one projection per tenant, so the tests of this class
///     share the test tenant's, and each checks only the new orders it projects. The grain is <c>[Authorize]</c>d and
///     <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in first.
/// </summary>
public sealed class UnassignedOrdersTests
    : ProjectionGrainTestHarness<IUnassignedOrdersGrain, UnassignedOrdersGrain, UnassignedOrdersState>,
      IClassFixture<OrdersCluster>
{
    private static readonly string TenantGrainKey = TenantKey.Create(TestData.TenantId, ReadModelGrainFactoryExtensions.DefaultKey);

    public UnassignedOrdersTests(OrdersCluster fixture) : base(fixture.Cluster, TenantGrainKey) { }

    [Fact]
    public async Task OrderNumberAssigned_PutsTheSubmittedOrderOnTheList()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(Numbered(Command1, orderId, 610_700));

        Then(state =>
        {
            Assert.Equal(
                new UnassignedOrderEntry(orderId, 610_700, TestData.LakeviewCreditUnionClientId,
                    "48 Birch Hollow Rd, Asheville, NC 28803", TestData.UniformResidentialAppraisal.FormTypeName),
                state.Orders[orderId]);
            Assert.False(state.Drafts.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task OrderSubmitted_IsNotOnTheListUntilItHasItsNumber()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
            DraftStarted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At),
            SubmitOrderTestData.Submitted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1)));

        Then(state =>
        {
            Assert.False(state.Orders.ContainsKey(orderId));
            Assert.True(state.Drafts.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task RequestAndPropertySaved_BeforeTheOrderIsOnTheList_ChangesWhatTheListShows()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
            DraftStarted(Command3, orderId, RequestedByUsers.ClientUser1, TestData.At),
            SaveRequestAndPropertyTestData.Saved(
                SaveRequestAndPropertyTestData.Command3 with { PropertyStreet = "11 Laurel Ridge Ct" },
                orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1)),
            SubmitOrderTestData.Submitted(Command3, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(2)),
            AssignOrderNumberTestData.Assigned(Command3, orderId, 610_701, TestData.At.AddMinutes(3)));

        Then(state => Assert.Equal("11 Laurel Ridge Ct, Black Mountain, NC 28711", state.Orders[orderId].PropertyAddress));
    }

    [Fact]
    public async Task OrderAssigned_TakesTheOrderOffTheList()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given([.. Numbered(Command1, orderId, 610_702), AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(5))]);

        Then(state =>
        {
            Assert.False(state.Orders.ContainsKey(orderId));
            Assert.False(state.Drafts.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task OrderAssigned_BeforeTheOrderHasItsNumber_KeepsItOffTheList()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
            DraftStarted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At),
            SubmitOrderTestData.Submitted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1)),
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(2)),
            AssignOrderNumberTestData.Assigned(Command1, orderId, 610_703, TestData.At.AddMinutes(3)));

        Then(state =>
        {
            Assert.False(state.Orders.ContainsKey(orderId));
            Assert.False(state.Drafts.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task Orders_ListTheOldestOrdersFirstWithTheirClientsNames()
    {
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await Given(
        [
            .. Numbered(Command1, order1, 610_710),
            .. Numbered(Command2, order2, 610_711),
            .. Numbered(Command3, order3, 610_712)
        ]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<UnassignedOrdersPage>(page => Assert.Equal(
        [
            new UnassignedOrder(order1, 610_710, "Lakeview Credit Union", "48 Birch Hollow Rd, Asheville, NC 28803",
                TestData.UniformResidentialAppraisal.FormTypeName),
            new UnassignedOrder(order2, 610_711, "Harbor National Bank", "715 Commerce Blvd, Greenville, SC 29601",
                TestData.RestrictedAppraisal.FormTypeName),
            new UnassignedOrder(order3, 610_712, "Lakeview Credit Union", "9 Laurel Ridge Ct, Black Mountain, NC 28711",
                TestData.DesktopAppraisal.FormTypeName)
        ], page.Orders.Where(order => order.OrderId == order1 || order.OrderId == order2 || order.OrderId == order3)));
    }

    [Fact]
    public async Task Orders_AreListedByOrderNumber_NotByWhenTheyWereProjected()
    {
        var (later, earlier) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Director1);
        await Given([.. Numbered(Command1, later, 610_721), .. Numbered(Command1, earlier, 610_720)]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<UnassignedOrdersPage>(page => Assert.Equal(
            [earlier, later],
            page.Orders.Select(order => order.OrderId).Where(orderId => orderId == later || orderId == earlier)));
    }

    [Fact]
    public async Task Orders_ArePaged()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given([.. Numbered(Command1, NewOrderId(), 610_730), .. Numbered(Command2, NewOrderId(), 610_731)]);
        var all = (await Grain.GetOrdersAsync(0, int.MaxValue)).Value;

        await When(() => Grain.GetOrdersAsync(1, 1));

        Then<UnassignedOrdersPage>(page =>
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
        var numbered = AssignOrderNumberTestData.Assigned(Command1, orderId, 610_740, TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderNumberAssigned.V1", 1, numbered, numbered.OccurredAt));

        GivenSignedIn(RequestedByUsers.Director1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetOrdersAsync(0, int.MaxValue)).Value.Orders.Any(order => order.OrderId == orderId))
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>The events of a draft started with <paramref name="draft"/>, submitted and given <paramref name="orderNumber"/>.</summary>
    private static object[] Numbered(StartOrderDraftCommand draft, string orderId, long orderNumber) =>
    [
        DraftStarted(draft, orderId, RequestedByUsers.ClientUser1, TestData.At),
        SubmitOrderTestData.Submitted(draft, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1)),
        AssignOrderNumberTestData.Assigned(draft, orderId, orderNumber, TestData.At.AddMinutes(2))
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

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
