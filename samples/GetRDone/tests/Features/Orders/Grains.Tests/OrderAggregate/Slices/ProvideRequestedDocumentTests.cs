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
///     Given/When/Then tests for the client providing a document the AMC requested. Each test uses a new order in the test
///     tenant. The first test checks what applying the slice's event does to the order state and the second checks every
///     value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class ProvideRequestedDocumentTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public ProvideRequestedDocumentTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingRequestedDocumentProvided_AddsTheDocumentAndClosesTheRequest()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        var attached = AttachSupportingDocumentTestData.Attached1(GrainId);
        state.When(started);
        state.When(attached);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(Requested1(GrainId));
        state.When(NoticeSent1(GrainId));
        var provided = Provided1(GrainId);

        state.When(provided);

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Acknowledged,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>
            {
                [attached.DocumentId] = attached.BlobPath,
                [provided.DocumentId] = provided.BlobPath
            },
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)RequestedByUsers.Director1.GetTenantUserId(),
            VendorAssignment = (OrderVendorAssignment?)null,
            InspectionAt = (DateTimeOffset?)null,
            InspectionCompletedOn = (DateOnly?)null,
            Report = (OrderReport?)null,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>
            {
                [Command1.RequestId] = new(
                    RequestedByUsers.ClientUser1.GetTenantUserId(), Command1.DocumentTypeId, Command1.DocumentTypeVersion,
                    Command1.DocumentTypeName, NoticeSent: true, Provided: true)
            },
            DescriptionOfWork = (OrderDescriptionOfWork?)null
        }, state, strict: true);
    }

    [Fact]
    public async Task Provide_RecordsTheDocumentAsTheRequestedType()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenRequested();

        await When(() => Order.ProvideRequestedDocumentAsync(ProvideCommand1));

        Then<ProvideRequestedDocumentResponse>(response =>
        {
            Assert.Equal(9, response.Version);
            var recorded = Assert.IsType<RequestedDocumentProvided>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Provided1(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Provide_RecordsTheFilePathUnderTheTenantOrderAndDocument()
    {
        TenantKey.Split(GrainId, out _, out var orderGuid);
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenRequested();

        await When(() => Order.ProvideRequestedDocumentAsync(ProvideCommand1));

        Then<ProvideRequestedDocumentResponse>(response => Assert.Equal(
            $"summit/{orderGuid}/3d9f6b21-8c4e-4a57-b0d3-5e2a7c9f1b64",
            Assert.IsType<RequestedDocumentProvided>(SingleEvent(response.Changes)).BlobPath));
    }

    [Fact]
    public async Task Provide_DoesNotNeedTheNoticeToBeSentFirst()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged(Requested1(GrainId));

        await When(() => Order.ProvideRequestedDocumentAsync(ProvideCommand1));

        Then<ProvideRequestedDocumentResponse>(response => Assert.IsType<RequestedDocumentProvided>(SingleEvent(response.Changes)));
    }

    [Fact]
    public async Task Provide_RejectsARequestThatWasAlreadyProvided()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenRequested(Provided1(GrainId));

        await When(() => Order.ProvideRequestedDocumentAsync(
            ProvideCommand1 with { DocumentId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Provide_RejectsADocumentThatIsAlreadyAttached()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenRequested();

        await When(() => Order.ProvideRequestedDocumentAsync(
            ProvideCommand1 with { DocumentId = AttachSupportingDocumentTestData.Command1.DocumentId }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.AlreadyExists, error.Code));
    }

    [Fact]
    public async Task Provide_RejectsARequestThatDoesNotExist()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenRequested();

        await When(() => Order.ProvideRequestedDocumentAsync(
            ProvideCommand1 with { RequestId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.ClientUser2))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    public async Task Provide_RejectsAUserTheDocumentsWereNotRequestedFrom(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenRequested();

        await When(() => Order.ProvideRequestedDocumentAsync(ProvideCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Theory]
    [InlineData(nameof(ProvideRequestedDocumentCommand.DocumentId), "not a GUID")]
    [InlineData(nameof(ProvideRequestedDocumentCommand.FileName), "missing")]
    [InlineData(nameof(ProvideRequestedDocumentCommand.ContentType), "missing")]
    [InlineData(nameof(ProvideRequestedDocumentCommand.FileSize), "empty file")]
    [InlineData(nameof(ProvideRequestedDocumentCommand.FileSize), "over 100 MB")]
    public async Task Provide_RejectsAnInvalidDetail(string property, string problem)
    {
        var command = (property, problem) switch
        {
            (nameof(ProvideRequestedDocumentCommand.DocumentId), _) => ProvideCommand1 with { DocumentId = "summit|survey" },
            (nameof(ProvideRequestedDocumentCommand.FileName), _) => ProvideCommand1 with { FileName = "" },
            (nameof(ProvideRequestedDocumentCommand.ContentType), _) => ProvideCommand1 with { ContentType = "" },
            (nameof(ProvideRequestedDocumentCommand.FileSize), "empty file") => ProvideCommand1 with { FileSize = 0 },
            (nameof(ProvideRequestedDocumentCommand.FileSize), _) => ProvideCommand1 with { FileSize = FileSize.MaxValue + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenRequested();

        await When(() => Order.ProvideRequestedDocumentAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Provide_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.ProvideRequestedDocumentAsync(ProvideCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Provide_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.ProvideRequestedDocumentAsync(ProvideCommand1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser2) => RequestedByUsers.ClientUser2,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>
    ///     Sets up an order with a supporting document that Director1 was assigned and acknowledged, followed by
    ///     <paramref name="more"/> events.
    /// </summary>
    private Task GivenAcknowledged(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        AttachSupportingDocumentTestData.Attached1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        .. more
    ]);

    /// <summary>Sets up an acknowledged order with the request <see cref="Command1"/>, whose notice was sent, followed by <paramref name="more"/> events.</summary>
    private Task GivenRequested(params OrderEvent[] more) => GivenAcknowledged([Requested1(GrainId), NoticeSent1(GrainId), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
