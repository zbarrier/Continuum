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

using static GetRDone.Orders.Grains.Tests.Data.DocumentRequestTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the AMC asking the client who started an acknowledged order for a document. Each test uses
///     a new order in the test tenant. The first test checks what applying the slice's event does to the order state and
///     the second checks every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class RequestClientDocumentTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public RequestClientDocumentTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingClientDocumentRequested_AddsTheOpenRequest()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));

        state.When(Requested1(GrainId));

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
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>
            {
                [Command1.RequestId] = new(
                    RequestedByUsers.ClientUser1.GetTenantUserId(), Command1.DocumentTypeId, Command1.DocumentTypeVersion,
                    Command1.DocumentTypeName, NoticeSent: false, Provided: false)
            }
        }, state, strict: true);
    }

    [Fact]
    public async Task Request_RecordsTheRequestToTheClientWhoStartedTheOrder()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.RequestClientDocumentAsync(Command1));

        Then<RequestClientDocumentResponse>(response =>
        {
            Assert.Equal(6, response.Version);
            var recorded = Assert.IsType<ClientDocumentRequested>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Requested1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Request_CarriesTheOrdersNumber()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(AssignOrderNumberTestData.Assigned1(GrainId));

        await When(() => Order.RequestClientDocumentAsync(Command1));

        Then<RequestClientDocumentResponse>(response => Assert.Equal(
            AssignOrderNumberTestData.FirstOrderNumber,
            Assert.IsType<ClientDocumentRequested>(SingleEvent(response.Changes)).OrderNumber));
    }

    [Fact]
    public async Task Request_LetsSomeoneWhoMayAssignOrdersAskForAnOrderAssignedToSomeoneElse()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenAcknowledged();

        await When(() => Order.RequestClientDocumentAsync(Command1));

        Then<RequestClientDocumentResponse>(response => Assert.Equal(
            RequestedByUsers.TenantAdmin1.GetTenantUserId(),
            Assert.IsType<ClientDocumentRequested>(SingleEvent(response.Changes)).InitiatedBy));
    }

    [Fact]
    public async Task Request_LetsTheAssigneeAskForMoreThanOneDocument()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(Requested1(GrainId));

        await When(() => Order.RequestClientDocumentAsync(Command1 with
        {
            RequestId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()),
            DocumentTypeId = TestData.PurchaseContract.DocumentTypeId,
            DocumentTypeVersion = TestData.PurchaseContract.DocumentTypeVersion,
            DocumentTypeName = TestData.PurchaseContract.Name
        }));

        Then<RequestClientDocumentResponse>(response => Assert.Equal(7, response.Version));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.Coordinator1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Request_RejectsAUserWhoIsNotTheAssigneeNorMayAssignOrders(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenAcknowledged();

        await When(() => Order.RequestClientDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Request_RejectsAnOrderThatIsNotAcknowledgedYet()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
            SubmitOrderTestData.Submitted1(GrainId),
            AssignOrderTestData.OrderAssigned1(GrainId));

        await When(() => Order.RequestClientDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Request_RejectsARequestThatAlreadyExists()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(Requested1(GrainId));

        await When(() => Order.RequestClientDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.AlreadyExists, error.Code));
    }

    [Fact]
    public async Task Request_RejectsADocumentTypeThatIsNotCurrent()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.RequestClientDocumentAsync(Command1 with { DocumentTypeVersion = 2 }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Theory]
    [InlineData(nameof(RequestClientDocumentCommand.RequestId), "not a GUID")]
    [InlineData(nameof(RequestClientDocumentCommand.RequestId), "another tenant")]
    [InlineData(nameof(RequestClientDocumentCommand.Message), "missing")]
    [InlineData(nameof(RequestClientDocumentCommand.Message), "too long")]
    public async Task Request_RejectsAnInvalidDetail(string property, string problem)
    {
        var command = (property, problem) switch
        {
            (nameof(RequestClientDocumentCommand.RequestId), "not a GUID") => Command1 with { RequestId = "summit|survey" },
            (nameof(RequestClientDocumentCommand.RequestId), _) => Command1 with { RequestId = $"other|{Guid.NewGuid()}" },
            (nameof(RequestClientDocumentCommand.Message), "missing") => Command1 with { Message = " " },
            (nameof(RequestClientDocumentCommand.Message), _) => Command1 with { Message = new string('a', DocumentRequestMessage.MaxLength + 1) },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.RequestClientDocumentAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Request_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.RequestClientDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Request_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.RequestClientDocumentAsync(Command1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.Coordinator1) => RequestedByUsers.Coordinator1,
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
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

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
