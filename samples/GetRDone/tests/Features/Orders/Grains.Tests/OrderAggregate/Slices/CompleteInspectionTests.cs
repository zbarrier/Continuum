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

using static GetRDone.Orders.Grains.Tests.Data.InspectionTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the vendor recording that it completed the scheduled inspection. Each test uses a new order
///     in the test tenant. The first test checks what applying the slice's event does to the order state and the second
///     checks every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class CompleteInspectionTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public CompleteInspectionTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingInspectionCompleted_CompletesTheInspection()
    {
        var state = new OrderState();
        foreach (var evt in AssignedEvents())
        {
            state.When(evt);
        }
        state.When(Scheduled1(GrainId));

        state.When(Completed1(GrainId));

        Assert.Equal(OrderStatus.InspectionCompleted, state.Status);
        Assert.Equal(InspectionTime, state.InspectionAt);
        Assert.Equal(CompleteCommand1.CompletedOn, state.InspectionCompletedOn);
        Assert.Null(state.Report);
    }

    [Fact]
    public async Task Complete_RecordsTheCompletedInspection()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenScheduled();

        await When(() => Order.CompleteInspectionAsync(CompleteCommand1));

        Then<CompleteInspectionResponse>(response =>
        {
            Assert.Equal(10, response.Version);
            var recorded = Assert.IsType<InspectionCompleted>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Completed1(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Complete_AllowsAnInspectionCompletedToday()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenScheduled();

        await When(() => Order.CompleteInspectionAsync(new CompleteInspectionCommand(today)));

        Then<CompleteInspectionResponse>(response =>
            Assert.Equal(today, Assert.IsType<InspectionCompleted>(SingleEvent(response.Changes)).CompletedOn));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.OtherVendorUser1))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Complete_RejectsAUserWhoIsNotOfTheAssignedVendor(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenScheduled();

        await When(() => Order.CompleteInspectionAsync(CompleteCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Complete_RejectsAnInspectionThatIsNotScheduled()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned();

        await When(() => Order.CompleteInspectionAsync(CompleteCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The inspection must be scheduled before it is completed.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Complete_RejectsAnInspectionThatIsAlreadyCompleted()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenScheduled(Completed1(GrainId));

        await When(() => Order.CompleteInspectionAsync(CompleteCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The inspection is already completed.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Complete_RejectsAnOrderThatIsNotAssignedToAVendor()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given(AcknowledgedEvents());

        await When(() => Order.CompleteInspectionAsync(CompleteCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The order is not assigned to a vendor yet.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Complete_RejectsAnInspectionCompletedInTheFuture()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenScheduled();

        await When(() => Order.CompleteInspectionAsync(new CompleteInspectionCommand(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2))));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.InvalidArgument, error.Code);
            Assert.Equal("The inspection can't be completed in the future.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Complete_RejectsAMissingDate()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenScheduled();

        await When(() => Order.CompleteInspectionAsync(new CompleteInspectionCommand(default)));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(CompleteInspectionCommand.CompletedOn), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Complete_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given();

        await When(() => Order.CompleteInspectionAsync(CompleteCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Complete_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.CompleteInspectionAsync(CompleteCommand1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        nameof(RequestedByUsers.OtherVendorUser1) => RequestedByUsers.OtherVendorUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    private OrderEvent[] AcknowledgedEvents() =>
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId)
    ];

    private OrderEvent[] AssignedEvents() =>
    [
        .. AcknowledgedEvents(),
        DescriptionOfWorkTestData.Submitted1(GrainId),
        DescriptionOfWorkTestData.Approved(GrainId),
        VendorAssignmentTestData.Assigned1(GrainId)
    ];

    /// <summary>Sets up an order Director1 assigned to Ridge Appraisal Group, followed by <paramref name="more"/> events.</summary>
    private Task GivenAssigned(params OrderEvent[] more) => Given([.. AssignedEvents(), .. more]);

    /// <summary>Sets up an order whose inspection VendorUser1 scheduled, followed by <paramref name="more"/> events.</summary>
    private Task GivenScheduled(params OrderEvent[] more) => GivenAssigned([Scheduled1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
