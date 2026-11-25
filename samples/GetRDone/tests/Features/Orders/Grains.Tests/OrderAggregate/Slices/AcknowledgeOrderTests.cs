using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.AcknowledgeOrderTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the assigned staff member acknowledging an order. Each test uses a new order in the test
///     tenant. The first test checks what applying the slice's event does to the order state and the second checks every
///     value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class AcknowledgeOrderTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public AcknowledgeOrderTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingOrderAcknowledged_MovesTheOrderToAcknowledged()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));

        state.When(Acknowledged1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Acknowledged,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)RequestedByUsers.Director1.GetTenantUserId(),
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>()
        }, state, strict: true);
    }

    [Fact]
    public async Task Acknowledge_RecordsTheAssigneeAcknowledgingTheOrder()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAssigned();

        await When(() => Order.AcknowledgeOrderAsync());

        Then<AcknowledgeOrderResponse>(response =>
        {
            Assert.Equal(5, response.Version);
            var recorded = Assert.IsType<OrderAcknowledged>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Acknowledged1(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.TenantAdmin1))]
    [InlineData(nameof(RequestedByUsers.Coordinator1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Acknowledge_RejectsAUserTheOrderIsNotAssignedTo(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenAssigned();

        await When(() => Order.AcknowledgeOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Acknowledge_RejectsAnOrderThatIsNotAssignedYet()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
            SubmitOrderTestData.Submitted1(GrainId));

        await When(() => Order.AcknowledgeOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Acknowledge_RejectsAnOrderThatIsAlreadyAcknowledged()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAssigned(Acknowledged1(GrainId));

        await When(() => Order.AcknowledgeOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Acknowledge_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.AcknowledgeOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Acknowledge_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.AcknowledgeOrderAsync());
    }

    [Fact]
    public async Task AssignOrderNumber_StillGivesAnAcknowledgedOrderItsNumber()
    {
        GivenSignedIn(AutomationUser.OrderNumberAssigner.CreatePrincipal(TestData.TenantId));
        await GivenAssigned(Acknowledged1(GrainId));

        await When(() => Order.AssignOrderNumberAsync(AssignOrderNumberTestData.FirstOrderNumber));

        Then<AssignOrderNumberResponse>(response => Assert.IsType<OrderNumberAssigned>(SingleEvent(response.Changes)));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.TenantAdmin1) => RequestedByUsers.TenantAdmin1,
        nameof(RequestedByUsers.Coordinator1) => RequestedByUsers.Coordinator1,
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>Sets up a submitted order assigned to Director1, followed by <paramref name="more"/> events.</summary>
    private Task GivenAssigned(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        .. more
    ]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
