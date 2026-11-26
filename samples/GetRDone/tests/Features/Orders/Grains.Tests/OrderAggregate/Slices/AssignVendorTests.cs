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

using static GetRDone.Orders.Grains.Tests.Data.VendorAssignmentTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for assigning an order whose description of work the client approved to one of the tenant's
///     vendors. Each test uses a new order in the test tenant. The first test checks what applying the slice's event does to
///     the order state and the second checks every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class AssignVendorTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public AssignVendorTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingVendorAssigned_AssignsTheVendor()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(DescriptionOfWorkTestData.Submitted1(GrainId));
        state.When(DescriptionOfWorkTestData.Approved(GrainId));

        state.When(Assigned1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.VendorAssigned,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)RequestedByUsers.Director1.GetTenantUserId(),
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)new OrderDescriptionOfWork(
                DescriptionOfWorkTestData.DescriptionOfWorkId, 1, RequestedByUsers.ClientUser1.GetTenantUserId(),
                Approved: true, NoticeSentVersion: null, ApprovedNoticeSent: false),
            VendorAssignment = (OrderVendorAssignment?)new OrderVendorAssignment(TestData.RidgeAppraisalGroupVendorId, NoticeSent: false),
            InspectionAt = (DateTimeOffset?)null,
            InspectionCompletedOn = (DateOnly?)null,
            Report = (OrderReport?)null,
            OrderShippedNoticeSent = false
        }, state, strict: true);
    }

    [Fact]
    public async Task Assign_RecordsTheVendorWithTheApprovedDescriptionOfWork()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(Command1));

        Then<AssignVendorResponse>(response =>
        {
            Assert.Equal(8, response.Version);
            var recorded = Assert.IsType<VendorAssigned>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Assigned1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Assign_CarriesTheOrdersNumberAndTheApprovedRevision()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(
            AssignOrderNumberTestData.Assigned1(GrainId),
            DescriptionOfWorkTestData.Submitted1(GrainId),
            DescriptionOfWorkTestData.Submitted(DescriptionOfWorkTestData.Command2, GrainId),
            DescriptionOfWorkTestData.Approved(GrainId, version: 2));

        await When(() => Order.AssignVendorAsync(Command1));

        Then<AssignVendorResponse>(response =>
        {
            var recorded = Assert.IsType<VendorAssigned>(SingleEvent(response.Changes));
            Assert.Equal(AssignOrderNumberTestData.FirstOrderNumber, recorded.OrderNumber);
            Assert.Equal(2, recorded.DescriptionOfWorkVersion);
        });
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.TenantAdmin1))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    public async Task Assign_LetsTheAssigneeOrAnOrderAssignerAssignTheVendor(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(Command1));

        Then<AssignVendorResponse>(response => Assert.IsType<VendorAssigned>(SingleEvent(response.Changes)));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.Coordinator1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    [InlineData(nameof(RequestedByUsers.VendorUser1))]
    public async Task Assign_RejectsAUserWhoIsNotTheAssigneeOrAnOrderAssigner(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnOrderWhoseDescriptionOfWorkIsNotApproved()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(DescriptionOfWorkTestData.Submitted1(GrainId));

        await When(() => Order.AssignVendorAsync(Command1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal(
                "The client must approve the description of work before the order is assigned to a vendor.",
                error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Assign_RejectsAnOrderThatIsAlreadyAssignedToAVendor()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved(Assigned1(GrainId));

        await When(() => Order.AssignVendorAsync(Command1 with { VendorId = TestData.CedarPointAppraisalsVendorId }));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The order is already assigned to a vendor.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Assign_RejectsAVendorTheTenantCanNotAssign()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(Command1 with { VendorId = TestData.UnknownVendorId }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsADueDateInThePast()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(
            Command1 with { VendorDueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsADueDateBeforeTheVendorsDueDate()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(Command1 with { DueDate = Command1.VendorDueDate.AddDays(-1) }));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.InvalidArgument, error.Code);
            Assert.Equal("The due date can't be before the vendor's due date.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Assign_AllowsADueDateOnTheVendorsDueDate()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(Command1 with { DueDate = Command1.VendorDueDate }));

        Then<AssignVendorResponse>(response =>
            Assert.Equal(Command1.VendorDueDate, Assert.IsType<VendorAssigned>(SingleEvent(response.Changes)).DueDate));
    }

    [Theory]
    [InlineData(nameof(AssignVendorCommand.VendorId), null)]
    [InlineData(nameof(AssignVendorCommand.VendorFee), "0")]
    [InlineData(nameof(AssignVendorCommand.VendorFee), "1000000")]
    [InlineData(nameof(AssignVendorCommand.VendorFee), "425.005")]
    [InlineData(nameof(AssignVendorCommand.VendorDueDate), null)]
    [InlineData(nameof(AssignVendorCommand.ServiceFee), "0")]
    [InlineData(nameof(AssignVendorCommand.ServiceFee), "1000000")]
    [InlineData(nameof(AssignVendorCommand.ServiceFee), "650.005")]
    [InlineData(nameof(AssignVendorCommand.DueDate), null)]
    public async Task Assign_RejectsAnInvalidDetail(string property, string? fee)
    {
        var command = property switch
        {
            nameof(AssignVendorCommand.VendorId) => Command1 with { VendorId = "other|5e1c9a37" },
            nameof(AssignVendorCommand.VendorFee) => Command1 with { VendorFee = decimal.Parse(fee!, System.Globalization.CultureInfo.InvariantCulture) },
            nameof(AssignVendorCommand.VendorDueDate) => Command1 with { VendorDueDate = default },
            nameof(AssignVendorCommand.ServiceFee) => Command1 with { ServiceFee = decimal.Parse(fee!, System.Globalization.CultureInfo.InvariantCulture) },
            nameof(AssignVendorCommand.DueDate) => Command1 with { DueDate = default },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenApproved();

        await When(() => Order.AssignVendorAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Assign_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.AssignVendorAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Assign_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.AssignVendorAsync(Command1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        nameof(RequestedByUsers.Coordinator1) => RequestedByUsers.Coordinator1,
        nameof(RequestedByUsers.TenantAdmin1) => RequestedByUsers.TenantAdmin1,
        nameof(RequestedByUsers.VendorUser1) => RequestedByUsers.VendorUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>Sets up a submitted order that Director1 was assigned and acknowledged, followed by <paramref name="more"/> events.</summary>
    private Task GivenAcknowledged(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        .. more
    ]);

    /// <summary>Sets up an order whose description of work ClientUser1 approved, followed by <paramref name="more"/> events.</summary>
    private Task GivenApproved(params OrderEvent[] more) => GivenAcknowledged(
        [DescriptionOfWorkTestData.Submitted1(GrainId), DescriptionOfWorkTestData.Approved(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
