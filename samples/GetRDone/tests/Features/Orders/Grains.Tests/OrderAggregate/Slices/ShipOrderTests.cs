using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.InspectionTestData;
using static GetRDone.Orders.Grains.Tests.Data.ShippingTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the AMC shipping the delivered report to the client. Each test uses a new order in the test
///     tenant. The first test checks what applying the slice's event does to the order state and the second checks every
///     value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class ShipOrderTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public ShipOrderTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingOrderShipped_ShipsTheOrder()
    {
        var state = new OrderState();
        foreach (var evt in DeliveredEvents())
        {
            state.When(evt);
        }

        state.When(Shipped1(GrainId));

        Assert.Equal(OrderStatus.Shipped, state.Status);
        Assert.Equal(new OrderReport(DeliverCommand1.DocumentId, NoticeSent: false), state.Report);
        Assert.False(state.OrderShippedNoticeSent);
    }

    [Fact]
    public async Task Ship_RecordsTheShippedReportForTheClientWhoEnteredTheOrder()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.Director1);
        await GivenDelivered();

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        Then<ShipOrderResponse>(response =>
        {
            Assert.Equal(12, response.Version);
            var recorded = Assert.IsType<OrderShipped>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Shipped1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Ship_CarriesTheOrdersNumber()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given([.. AssignedEvents(), AssignOrderNumberTestData.Assigned1(GrainId), Scheduled1(GrainId), Completed1(GrainId),
            Delivered1(GrainId)]);

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        Then<ShipOrderResponse>(response => Assert.Equal(
            AssignOrderNumberTestData.FirstOrderNumber,
            Assert.IsType<OrderShipped>(SingleEvent(response.Changes)).OrderNumber));
    }

    [Fact]
    public async Task Ship_LetsTheTenantAdminShipAnOrderAssignedToSomeoneElse()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenDelivered();

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        Then<ShipOrderResponse>(response => Assert.IsType<OrderShipped>(SingleEvent(response.Changes)));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.Coordinator1))]
    [InlineData(nameof(RequestedByUsers.VendorUser1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Ship_RejectsAUserWhoIsNotTheAssigneeOrAnAssigner(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenDelivered();

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Ship_RejectsAnOrderWhoseReportIsNotDelivered()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given([.. AssignedEvents(), Scheduled1(GrainId), Completed1(GrainId)]);

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The report must be delivered before the order is shipped.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Ship_RejectsAnOrderThatWasAlreadyShipped()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenDelivered(Shipped1(GrainId));

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The order was already shipped.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Ship_RejectsADocumentThatIsNotTheDeliveredReport()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
        [
            StartOrderDraftTestData.DraftStarted1(GrainId),
            AttachSupportingDocumentTestData.Attached1(GrainId),
            .. DeliveredEvents().Skip(1)
        ]);

        await When(() => Order.ShipOrderAsync(new ShipOrderCommand(AttachSupportingDocumentTestData.Command1.DocumentId)));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.NotFound, error.Code);
            Assert.Equal("The report was not found.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Ship_RejectsADocumentOfAnotherTenant()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenDelivered();

        await When(() => Order.ShipOrderAsync(new ShipOrderCommand("other|8f2c4e6a-1b3d-4f5a-9c7e-0d2b4a6c8e13")));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(ShipOrderCommand.DocumentId), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Ship_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.ShipOrderAsync(ShipCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Ship_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.ShipOrderAsync(ShipCommand1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        nameof(RequestedByUsers.Coordinator1) => RequestedByUsers.Coordinator1,
        nameof(RequestedByUsers.VendorUser1) => RequestedByUsers.VendorUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    private OrderEvent[] AssignedEvents() =>
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        DescriptionOfWorkTestData.Submitted1(GrainId),
        DescriptionOfWorkTestData.Approved(GrainId),
        VendorAssignmentTestData.Assigned1(GrainId)
    ];

    private OrderEvent[] DeliveredEvents() => [.. AssignedEvents(), Scheduled1(GrainId), Completed1(GrainId), Delivered1(GrainId)];

    /// <summary>Sets up an order whose report VendorUser1 delivered, followed by <paramref name="more"/> events.</summary>
    private Task GivenDelivered(params OrderEvent[] more) => Given([.. DeliveredEvents(), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
