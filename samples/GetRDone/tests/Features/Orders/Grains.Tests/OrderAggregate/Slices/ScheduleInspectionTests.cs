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
///     Given/When/Then tests for the vendor scheduling the inspection of an order assigned to it. Each test uses a new order
///     in the test tenant. The first test checks what applying the slice's event does to the order state and the second
///     checks every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class ScheduleInspectionTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public ScheduleInspectionTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingInspectionScheduled_SchedulesTheInspection()
    {
        var state = new OrderState();
        foreach (var evt in AssignedEvents())
        {
            state.When(evt);
        }

        state.When(Scheduled1(GrainId));

        Assert.Equal(OrderStatus.InspectionScheduled, state.Status);
        Assert.Equal(InspectionTime, state.InspectionAt);
        Assert.Null(state.InspectionCompletedOn);
        Assert.Null(state.Report);
    }

    [Fact]
    public async Task Schedule_RecordsTheInspection()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned();

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1));

        Then<ScheduleInspectionResponse>(response =>
        {
            Assert.Equal(9, response.Version);
            var recorded = Assert.IsType<InspectionScheduled>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Scheduled1(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Schedule_LetsAnotherUserOfTheVendorScheduleTheInspection()
    {
        GivenSignedIn(RequestedByUsers.VendorUser2);
        await GivenAssigned();

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1));

        Then<ScheduleInspectionResponse>(response =>
            Assert.Equal(RequestedByUsers.VendorUser2.GetTenantUserId(),
                Assert.IsType<InspectionScheduled>(SingleEvent(response.Changes)).InitiatedBy));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.OtherVendorUser1))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Schedule_RejectsAUserWhoIsNotOfTheAssignedVendor(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenAssigned();

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Schedule_RejectsAnOrderThatIsNotAssignedToAVendor()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAcknowledged(DescriptionOfWorkTestData.Submitted1(GrainId), DescriptionOfWorkTestData.Approved(GrainId));

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The order is not assigned to a vendor yet.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Schedule_RejectsAnInspectionThatIsAlreadyScheduled()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned(Scheduled1(GrainId));

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The inspection is already scheduled.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Schedule_RejectsAnInspectionInThePast()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned();

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1 with { InspectionAt = DateTimeOffset.UtcNow.AddHours(-1) }));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.InvalidArgument, error.Code);
            Assert.Equal("The inspection can't be scheduled in the past.", error.GetFormattedMessage());
        });
    }

    [Theory]
    [InlineData(nameof(ScheduleInspectionCommand.InspectionAt))]
    [InlineData(nameof(ScheduleInspectionCommand.ContactName))]
    public async Task Schedule_RejectsAnInvalidDetail(string property)
    {
        var command = property switch
        {
            nameof(ScheduleInspectionCommand.InspectionAt) => ScheduleCommand1 with { InspectionAt = default },
            nameof(ScheduleInspectionCommand.ContactName) => ScheduleCommand1 with { ContactName = new string('a', 101) },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned();

        await When(() => Order.ScheduleInspectionAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Schedule_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given();

        await When(() => Order.ScheduleInspectionAsync(ScheduleCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Schedule_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.ScheduleInspectionAsync(ScheduleCommand1));
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

    /// <summary>Sets up a submitted order that Director1 was assigned and acknowledged, followed by <paramref name="more"/> events.</summary>
    private Task GivenAcknowledged(params OrderEvent[] more) => Given([.. AcknowledgedEvents(), .. more]);

    /// <summary>Sets up an order Director1 assigned to Ridge Appraisal Group, followed by <paramref name="more"/> events.</summary>
    private Task GivenAssigned(params OrderEvent[] more) => Given([.. AssignedEvents(), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
