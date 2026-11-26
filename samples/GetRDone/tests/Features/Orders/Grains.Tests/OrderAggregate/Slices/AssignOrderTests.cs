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

using static GetRDone.Orders.Grains.Tests.Data.AssignOrderTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for assigning a submitted order to one of the AMC's staff. Each test uses a new order in the
///     test tenant. The first test checks what applying the slice's event does to the order state and the second checks
///     every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class AssignOrderTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public AssignOrderTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingOrderAssigned_MovesTheOrderToAssignedAndRecordsTheAssignee()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));

        state.When(OrderAssigned1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Assigned,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)RequestedByUsers.Director1.GetTenantUserId(),
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)null
        }, state, strict: true);
    }

    [Fact]
    public async Task Assign_RecordsTheDirectorAssigningTheOrderToThemselves()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted();

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Director1.GetTenantUserId())));

        Then<AssignOrderResponse>(response =>
        {
            Assert.Equal(4, response.Version);
            var recorded = Assert.IsType<OrderAssigned>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(OrderAssigned1(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Assign_LetsTheTenantAdminAssignTheOrderToACoordinator()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenSubmitted();

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Coordinator1.GetTenantUserId())));

        Then<AssignOrderResponse>(response =>
        {
            var recorded = Assert.IsType<OrderAssigned>(SingleEvent(response.Changes));
            Assert.Equivalent(
                OrderAssigned(GrainId, RequestedByUsers.Coordinator1, RequestedByUsers.TenantAdmin1, recorded.OccurredAt),
                recorded, strict: true);
        });
    }

    [Fact]
    public async Task Assign_LetsAnOrderWithItsNumberBeAssigned()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted(AssignOrderNumberTestData.Assigned1(GrainId));

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Director1.GetTenantUserId())));

        Then<AssignOrderResponse>(response => Assert.IsType<OrderAssigned>(SingleEvent(response.Changes)));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.Coordinator1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Assign_RejectsAUserWhoCannotAssignOrders(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenSubmitted();

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Director1.GetTenantUserId())));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAClientUserAsTheAssignee()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted();

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.ClientUser1.GetTenantUserId())));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnAutomationAsTheAssignee()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted();

        await When(() => Order.AssignOrderAsync(
            new AssignOrderCommand(AutomationUser.OrderNumberAssigner.GetTenantUserId(TestData.TenantId))));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnAssigneeWhoIsNotAUserId()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted();

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand("not a user")));

        ThenError<ValidationError>(error => Assert.Equal("AssignedToUserId", Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Assign_RejectsADraft()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), SaveBorrowerAndAccessContactTestData.Saved1(GrainId));

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Director1.GetTenantUserId())));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnOrderThatIsAlreadyAssigned()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenSubmitted(OrderAssigned1(GrainId));

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.TenantAdmin1.GetTenantUserId())));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Director1.GetTenantUserId())));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => Order.AssignOrderAsync(new AssignOrderCommand(RequestedByUsers.Director1.GetTenantUserId())));
    }

    [Fact]
    public async Task AssignOrderNumber_StillGivesAnAssignedOrderItsNumber()
    {
        GivenSignedIn(AutomationUser.OrderNumberAssigner.CreatePrincipal(TestData.TenantId));
        await GivenSubmitted(OrderAssigned1(GrainId));

        await When(() => Order.AssignOrderNumberAsync(AssignOrderNumberTestData.FirstOrderNumber));

        Then<AssignOrderNumberResponse>(response => Assert.IsType<OrderNumberAssigned>(SingleEvent(response.Changes)));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.Coordinator1) => RequestedByUsers.Coordinator1,
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>Sets up a submitted order, followed by <paramref name="more"/> events.</summary>
    private Task GivenSubmitted(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        .. more
    ]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
