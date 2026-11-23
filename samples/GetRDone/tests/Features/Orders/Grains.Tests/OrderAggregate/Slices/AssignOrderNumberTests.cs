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

using static GetRDone.Orders.Grains.Tests.Data.AssignOrderNumberTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>Given/When/Then tests for giving a submitted order its number. Each test uses a new order in the test tenant.</summary>
public sealed class AssignOrderNumberTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public AssignOrderNumberTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.OrderNumberAssigner.CreatePrincipal(TestData.TenantId);

    [Fact]
    public async Task Assign_GivesTheSubmittedOrderItsNumberForItsClient()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.AssignOrderNumberAsync(FirstOrderNumber));

        Then<AssignOrderNumberResponse>(response =>
        {
            Assert.Equal(4, response.Version);
            var recorded = Assert.IsType<OrderNumberAssigned>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Assigned1(GrainId, FirstOrderNumber, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Assign_SucceedsWithoutChangesWhenTheOrderAlreadyHasItsNumber()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted(Assigned1(GrainId));

        await When(() => Order.AssignOrderNumberAsync(FirstOrderNumber));

        Then<AssignOrderNumberResponse>(response => Assert.Empty(response.Changes));
        await ThenNoChanges();
    }

    [Fact]
    public async Task Assign_RejectsADraft()
    {
        GivenSignedIn(Automation);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), SaveBorrowerAndAccessContactTestData.Saved1(GrainId));

        await When(() => Order.AssignOrderNumberAsync(FirstOrderNumber));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Order.AssignOrderNumberAsync(FirstOrderNumber));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnInvalidNumber()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted();

        await When(() => Order.AssignOrderNumberAsync(0));

        ThenError<ValidationError>(error => Assert.Equal("OrderNumber", Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Assign_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted();

        await When(() => Order.AssignOrderNumberAsync(FirstOrderNumber));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.AssignOrderNumberAsync(FirstOrderNumber));
    }

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
