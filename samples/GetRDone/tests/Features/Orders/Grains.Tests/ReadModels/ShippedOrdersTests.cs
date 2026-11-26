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
///     Tests for the staff dashboard's Shipped projection. There is one projection per tenant, so the tests of this class
///     share the test tenant's, and each checks only the new orders it projects. The grain is <c>[Authorize]</c>d and
///     <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in first.
/// </summary>
public sealed class ShippedOrdersTests
    : ProjectionGrainTestHarness<IShippedOrdersGrain, ShippedOrdersGrain, ShippedOrdersState>,
      IClassFixture<OrdersCluster>
{
    private static readonly string TenantGrainKey = TenantKey.Create(TestData.TenantId, ReadModelGrainFactoryExtensions.DefaultKey);

    public ShippedOrdersTests(OrdersCluster fixture) : base(fixture.Cluster, TenantGrainKey) { }

    [Fact]
    public async Task OrderShipped_PutsTheOrderOnTheListWithWhenItShipped()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given([.. Shipped(Command1, orderId, TestData.At.AddMinutes(11), orderNumber: 610_830)]);

        Then(state =>
        {
            Assert.Equal(
                new ShippedOrderEntry(orderId, 610_830, TestData.LakeviewCreditUnionClientId,
                    "48 Birch Hollow Rd, Asheville, NC 28803", TestData.At.AddMinutes(11)),
                state.Orders[orderId]);
            Assert.False(state.Pending.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task OrderNumberAssigned_AfterTheOrderShips_GivesTheOrderItsNumber()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given(
        [
            .. Shipped(Command1, orderId, TestData.At.AddMinutes(11)),
            AssignOrderNumberTestData.Assigned(Command1, orderId, 610_831, TestData.At.AddMinutes(12))
        ]);

        Then(state => Assert.Equal(610_831, state.Orders[orderId].OrderNumber));
    }

    [Fact]
    public async Task ReportDelivered_IsNotOnTheListUntilTheOrderShips()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.Director1);

        await Given([.. Delivered(Command1, orderId)]);

        Then(state =>
        {
            Assert.False(state.Orders.ContainsKey(orderId));
            Assert.True(state.Pending.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task Orders_ListTheMostRecentlyShippedFirstWithTheirClientsNames()
    {
        var (shippedFirst, shippedLast) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await Given(
        [
            .. Shipped(Command1, shippedFirst, TestData.At.AddMinutes(11), orderNumber: 610_832),
            .. Shipped(Command2, shippedLast, TestData.At.AddMinutes(12))
        ]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<ShippedOrdersPage>(page => Assert.Equal(
        [
            new ShippedOrder(shippedLast, null, "Harbor National Bank", "715 Commerce Blvd, Greenville, SC 29601", TestData.At.AddMinutes(12)),
            new ShippedOrder(shippedFirst, 610_832, "Lakeview Credit Union", "48 Birch Hollow Rd, Asheville, NC 28803",
                TestData.At.AddMinutes(11))
        ], page.Orders.Where(order => order.OrderId == shippedFirst || order.OrderId == shippedLast)));
    }

    [Fact]
    public async Task Orders_ArePaged()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.Director1);
        await Given([.. Shipped(Command1, order1, TestData.At.AddMinutes(11)), .. Shipped(Command2, order2, TestData.At.AddMinutes(12))]);
        var all = (await Grain.GetOrdersAsync(0, int.MaxValue)).Value;

        await When(() => Grain.GetOrdersAsync(1, 1));

        Then<ShippedOrdersPage>(page =>
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
        var shipped = ShippingTestData.Shipped1(orderId, occurredAt: TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderShipped.V1", 1, shipped, shipped.OccurredAt));

        GivenSignedIn(RequestedByUsers.Director1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetOrdersAsync(0, int.MaxValue)).Value.Orders.Any(order => order.OrderId == orderId))
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>
    ///     The events of a draft started with <paramref name="draft"/>, submitted, given <paramref name="orderNumber"/> when
    ///     there is one, worked by Director1 and Ridge Appraisal Group, up to the delivered report.
    /// </summary>
    private static object[] Delivered(StartOrderDraftCommand draft, string orderId, long? orderNumber = null)
    {
        object[] numbered = orderNumber is { } number
            ? [AssignOrderNumberTestData.Assigned(draft, orderId, number, TestData.At.AddMinutes(2))]
            : [];
        return
        [
            DraftStarted(draft, orderId, RequestedByUsers.ClientUser1, TestData.At),
            SubmitOrderTestData.Submitted(draft, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1)),
            .. numbered,
            AssignOrderTestData.OrderAssigned1(orderId, TestData.At.AddMinutes(3)),
            AcknowledgeOrderTestData.Acknowledged1(orderId, TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Submitted1(orderId, orderNumber, TestData.At.AddMinutes(5)),
            DescriptionOfWorkTestData.Approved(orderId, orderNumber: orderNumber, occurredAt: TestData.At.AddMinutes(6)),
            VendorAssignmentTestData.Assigned1(orderId, orderNumber, TestData.At.AddMinutes(7)),
            InspectionTestData.Scheduled1(orderId, TestData.At.AddMinutes(8)),
            InspectionTestData.Completed1(orderId, TestData.At.AddMinutes(9)),
            InspectionTestData.Delivered1(orderId, orderNumber, TestData.At.AddMinutes(10))
        ];
    }

    /// <summary>The events of <see cref="Delivered"/>, then Director1 ships the order at <paramref name="shippedAt"/>.</summary>
    private static object[] Shipped(StartOrderDraftCommand draft, string orderId, DateTimeOffset shippedAt, long? orderNumber = null)
        => [.. Delivered(draft, orderId, orderNumber), ShippingTestData.Shipped1(orderId, orderNumber, shippedAt)];

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
