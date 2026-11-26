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

using static GetRDone.Orders.Grains.Tests.Data.DescriptionOfWorkTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the client who started the order approving the description of work that waits for their
///     approval. Each test uses a new order in the test tenant. The first test checks what applying the slice's event does
///     to the order state and the second checks every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class ApproveDescriptionOfWorkTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public ApproveDescriptionOfWorkTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingDescriptionOfWorkApproved_ApprovesTheDescriptionOfWork()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(Submitted1(GrainId));

        state.When(Approved(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.DescriptionOfWorkApproved,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)RequestedByUsers.Director1.GetTenantUserId(),
            VendorAssignment = (OrderVendorAssignment?)null,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)new OrderDescriptionOfWork(
                DescriptionOfWorkTestData.DescriptionOfWorkId, 1, RequestedByUsers.ClientUser1.GetTenantUserId(),
                Approved: true, NoticeSentVersion: null, ApprovedNoticeSent: false)
        }, state, strict: true);
    }

    [Fact]
    public async Task Approve_RecordsTheApprovalForTheStaffMemberTheOrderIsAssignedTo()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted();

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        Then<ApproveDescriptionOfWorkResponse>(response =>
        {
            Assert.Equal(7, response.Version);
            var recorded = Assert.IsType<DescriptionOfWorkApproved>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Approved(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Approve_CarriesTheOrdersNumber()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted(AssignOrderNumberTestData.Assigned1(GrainId));

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        Then<ApproveDescriptionOfWorkResponse>(response => Assert.Equal(
            AssignOrderNumberTestData.FirstOrderNumber,
            Assert.IsType<DescriptionOfWorkApproved>(SingleEvent(response.Changes)).OrderNumber));
    }

    [Fact]
    public async Task Approve_ApprovesARevision()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted(Submitted(Command2, GrainId));

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1 with { DescriptionOfWorkVersion = 2 }));

        Then<ApproveDescriptionOfWorkResponse>(response => Assert.Equal(
            2, Assert.IsType<DescriptionOfWorkApproved>(SingleEvent(response.Changes)).DescriptionOfWorkVersion));
    }

    [Fact]
    public async Task Approve_RejectsAVersionThatWasRevised()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted(Submitted(Command2, GrainId));

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Approve_RejectsADescriptionOfWorkThatIsAlreadyApproved()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted(Approved(GrainId));

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The description of work was already approved.", error.GetFormattedMessage());
        });
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.ClientUser2))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    [InlineData(nameof(RequestedByUsers.TenantAdmin1))]
    public async Task Approve_RejectsAUserWhoDidNotStartTheOrder(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenSubmitted();

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Approve_RejectsAnOrderWithoutADescriptionOfWork()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged();

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Approve_RejectsAnotherDescriptionOfWork()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted();

        await When(() => Order.ApproveDescriptionOfWorkAsync(
            ApproveCommand1 with { DescriptionOfWorkId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Theory]
    [InlineData(nameof(ApproveDescriptionOfWorkCommand.DescriptionOfWorkId))]
    [InlineData(nameof(ApproveDescriptionOfWorkCommand.DescriptionOfWorkVersion))]
    public async Task Approve_RejectsAnInvalidDetail(string property)
    {
        var command = property switch
        {
            nameof(ApproveDescriptionOfWorkCommand.DescriptionOfWorkId) => ApproveCommand1 with { DescriptionOfWorkId = "summit|dow" },
            _ => ApproveCommand1 with { DescriptionOfWorkVersion = 0 }
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted();

        await When(() => Order.ApproveDescriptionOfWorkAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Approve_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Approve_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.ApproveDescriptionOfWorkAsync(ApproveCommand1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser2) => RequestedByUsers.ClientUser2,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        nameof(RequestedByUsers.TenantAdmin1) => RequestedByUsers.TenantAdmin1,
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

    /// <summary>Sets up an acknowledged order with the description of work <see cref="DescriptionOfWorkTestData.Command1"/>, followed by <paramref name="more"/> events.</summary>
    private Task GivenSubmitted(params OrderEvent[] more) => GivenAcknowledged([Submitted1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
