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
///     Tests for the vendor dashboard's Orders projection. There is one projection per tenant, so the tests of this class
///     share the test tenant's, and each checks only the new orders it projects. The grain is <c>[Authorize]</c>d and
///     <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in first.
/// </summary>
public sealed class VendorOrdersTests
    : ProjectionGrainTestHarness<IVendorOrdersGrain, VendorOrdersGrain, VendorOrdersState>,
      IClassFixture<OrdersCluster>
{
    private static readonly string TenantGrainKey = TenantKey.Create(TestData.TenantId, ReadModelGrainFactoryExtensions.DefaultKey);

    public VendorOrdersTests(OrdersCluster fixture) : base(fixture.Cluster, TenantGrainKey) { }

    [Fact]
    public async Task VendorAssigned_PutsTheOrderOnTheListWithItsVendor()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.VendorUser1);

        await Given(
        [
            .. Submitted(orderId),
            AssignOrderNumberTestData.Assigned(Command1, orderId, 610_960, TestData.At.AddMinutes(2)),
            VendorAssignmentTestData.Assigned1(orderId, 610_960, TestData.At.AddMinutes(3))
        ]);

        Then(state =>
        {
            Assert.Equal(
                new VendorOrderEntry(orderId, 610_960, TestData.RidgeAppraisalGroupVendorId, "48 Birch Hollow Rd, Asheville, NC 28803",
                    TestData.UniformResidentialAppraisal.FormTypeName, VendorOrderStatus.Assigned, VendorAssignmentTestData.VendorDueDate),
                state.Orders[orderId]);
            Assert.False(state.Drafts.ContainsKey(orderId));
        });
    }

    [Fact]
    public async Task OrderNumberAssigned_AfterTheVendorIsAssigned_GivesTheOrderItsNumber()
    {
        var orderId = NewOrderId();
        GivenSignedIn(RequestedByUsers.VendorUser1);

        await Given(
        [
            .. Submitted(orderId),
            VendorAssignmentTestData.Assigned1(orderId, occurredAt: TestData.At.AddMinutes(2)),
            AssignOrderNumberTestData.Assigned(Command1, orderId, 610_961, TestData.At.AddMinutes(3))
        ]);

        Then(state => Assert.Equal(610_961, state.Orders[orderId].OrderNumber));
    }

    [Fact]
    public async Task Orders_ListOnlyTheSignedInUsersVendorsOrdersDueFirstFirst()
    {
        var (laterOrder, soonerOrder, otherVendorOrder) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given(
        [
            .. Submitted(laterOrder),
            Assigned(laterOrder, TestData.RidgeAppraisalGroupVendorId, new DateOnly(2099, 12, 20)),
            .. Submitted(soonerOrder),
            Assigned(soonerOrder, TestData.RidgeAppraisalGroupVendorId, new DateOnly(2099, 12, 10)),
            .. Submitted(otherVendorOrder),
            Assigned(otherVendorOrder, TestData.CedarPointAppraisalsVendorId, new DateOnly(2099, 12, 1))
        ]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<VendorOrdersPage>(page => Assert.Equal(
        [
            new VendorOrder(soonerOrder, null, "48 Birch Hollow Rd, Asheville, NC 28803",
                TestData.UniformResidentialAppraisal.FormTypeName, VendorOrderStatus.Assigned, new DateOnly(2099, 12, 10)),
            new VendorOrder(laterOrder, null, "48 Birch Hollow Rd, Asheville, NC 28803",
                TestData.UniformResidentialAppraisal.FormTypeName, VendorOrderStatus.Assigned, new DateOnly(2099, 12, 20))
        ], page.Orders.Where(order => order.OrderId == laterOrder || order.OrderId == soonerOrder || order.OrderId == otherVendorOrder)));
    }

    [Fact]
    public async Task Orders_ShowAnotherVendorsUsersOnlyItsOrders()
    {
        var (ridgeOrder, cedarPointOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.OtherVendorUser1);
        await Given(
        [
            .. Submitted(ridgeOrder),
            Assigned(ridgeOrder, TestData.RidgeAppraisalGroupVendorId, new DateOnly(2099, 12, 10)),
            .. Submitted(cedarPointOrder),
            Assigned(cedarPointOrder, TestData.CedarPointAppraisalsVendorId, new DateOnly(2099, 12, 10))
        ]);

        await When(() => Grain.GetOrdersAsync(0, int.MaxValue));

        Then<VendorOrdersPage>(page =>
        {
            Assert.Contains(page.Orders, order => order.OrderId == cedarPointOrder);
            Assert.DoesNotContain(page.Orders, order => order.OrderId == ridgeOrder);
        });
    }

    [Fact]
    public async Task Orders_ArePaged()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given(
        [
            .. Submitted(order1),
            VendorAssignmentTestData.Assigned1(order1, occurredAt: TestData.At.AddMinutes(2)),
            .. Submitted(order2),
            VendorAssignmentTestData.Assigned1(order2, occurredAt: TestData.At.AddMinutes(2))
        ]);
        var all = (await Grain.GetOrdersAsync(0, int.MaxValue)).Value;

        await When(() => Grain.GetOrdersAsync(1, 1));

        Then<VendorOrdersPage>(page =>
        {
            Assert.Equal(all.Orders[1], Assert.Single(page.Orders));
            Assert.Equal(all.TotalCount, page.TotalCount);
        });
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.Director1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Orders_CanOnlyBeReadByVendorUsers(string userName)
    {
        GivenSignedIn(userName == nameof(RequestedByUsers.Director1) ? RequestedByUsers.Director1 : RequestedByUsers.ClientUser1);

        await When(() => Grain.GetOrdersAsync(0, 10));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    public async Task Orders_RejectAnInvalidPage(int skip, int take)
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);

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
        var assigned = VendorAssignmentTestData.Assigned1(orderId, occurredAt: TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream(orderId, "Orders.VendorAssigned.V1", 1, assigned, assigned.OccurredAt));

        GivenSignedIn(RequestedByUsers.VendorUser1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetOrdersAsync(0, int.MaxValue)).Value.Orders.Any(order => order.OrderId == orderId))
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>The events of a draft started with <c>Command1</c> and submitted.</summary>
    private static object[] Submitted(string orderId) =>
    [
        DraftStarted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At),
        SubmitOrderTestData.Submitted(Command1, orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1))
    ];

    /// <summary>The order assigned to <paramref name="vendorId"/>, due on <paramref name="vendorDueDate"/>.</summary>
    private static VendorAssigned Assigned(string orderId, string vendorId, DateOnly vendorDueDate)
    {
        var assigned = VendorAssignmentTestData.Assigned1(orderId, occurredAt: TestData.At.AddMinutes(2));
        return new VendorAssigned
        {
            OrderId = assigned.OrderId,
            VendorId = vendorId,
            VendorFee = assigned.VendorFee,
            VendorDueDate = vendorDueDate,
            DescriptionOfWorkId = assigned.DescriptionOfWorkId,
            DescriptionOfWorkVersion = assigned.DescriptionOfWorkVersion,
            OrderNumber = assigned.OrderNumber,
            InitiatedBy = assigned.InitiatedBy,
            OccurredAt = assigned.OccurredAt
        };
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
