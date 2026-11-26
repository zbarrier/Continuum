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

using static GetRDone.Orders.Grains.Tests.Data.RemoveSupportingDocumentTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for removing a supporting document from a draft. Each test uses a new order in the test tenant;
///     the documents come from <see cref="AttachSupportingDocumentTestData"/>. The first test checks what applying the
///     slice's event does to the order state and the second checks every value of the event the slice records; the behavior
///     tests follow.
/// </summary>
public sealed class RemoveSupportingDocumentTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public RemoveSupportingDocumentTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingSupportingDocumentRemoved_RemovesOnlyThatDocument()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        var kept = AttachSupportingDocumentTestData.Attached(AttachSupportingDocumentTestData.Command3, GrainId, RequestedByUsers.ClientUser1);
        state.When(started);
        state.When(AttachSupportingDocumentTestData.Attached1(GrainId));
        state.When(kept);

        state.When(Removed1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Draft,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = false,
            SupportingDocumentBlobPaths = new Dictionary<string, string> { [kept.DocumentId] = kept.BlobPath },
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)null,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)null
        }, state, strict: true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Remove_RecordsTheRemovalWithTheFilePath(int variation)
    {
        var (user, draftStarted, attached, command, removed) = Variation(variation);
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(user);
        await Given(draftStarted(GrainId, null), attached(GrainId, null));

        await When(() => Order.RemoveSupportingDocumentAsync(command));

        Then<RemoveSupportingDocumentResponse>(response =>
        {
            Assert.Equal(3, response.Version);
            var recorded = Assert.IsType<SupportingDocumentRemoved>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(removed(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Remove_KeepsTheOtherDocuments()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            AttachSupportingDocumentTestData.Attached1(GrainId),
            AttachSupportingDocumentTestData.Attached(AttachSupportingDocumentTestData.Command3, GrainId, RequestedByUsers.ClientUser1),
            Removed1(GrainId));

        await When(() => Order.RemoveSupportingDocumentAsync(Command3));

        Then<RemoveSupportingDocumentResponse>(response =>
        {
            Assert.Equal(5, response.Version);
            var recorded = Assert.IsType<SupportingDocumentRemoved>(SingleEvent(response.Changes));
            Assert.Equivalent(Removed(Command3, GrainId, RequestedByUsers.ClientUser1, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Remove_RejectsADocumentThatIsNotAttached()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), AttachSupportingDocumentTestData.Attached1(GrainId));

        await When(() => Order.RemoveSupportingDocumentAsync(Command3));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Remove_RejectsADocumentThatWasAlreadyRemoved()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), AttachSupportingDocumentTestData.Attached1(GrainId), Removed1(GrainId));

        await When(() => Order.RemoveSupportingDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Remove_AllowsTheDocumentToBeAttachedAgain()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), AttachSupportingDocumentTestData.Attached1(GrainId), Removed1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(AttachSupportingDocumentTestData.Command1));

        Then<AttachSupportingDocumentResponse>(response => Assert.Equal(4, response.Version));
    }

    [Theory]
    [InlineData("")]
    [InlineData("summit|purchase-contract")]
    [InlineData("other|0f4b8d2e-6a1c-4e3f-9b7d-2c5e8a1f3d60")]
    public async Task Remove_RejectsAnInvalidDocumentId(string documentId)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), AttachSupportingDocumentTestData.Attached1(GrainId));

        await When(() => Order.RemoveSupportingDocumentAsync(new RemoveSupportingDocumentCommand(documentId)));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(RemoveSupportingDocumentCommand.DocumentId), Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Remove_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.RemoveSupportingDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Remove_RejectsAnOrderThatWasSubmitted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
            AttachSupportingDocumentTestData.Attached1(GrainId),
            SubmitOrderTestData.Submitted1(GrainId));

        await When(() => Order.RemoveSupportingDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Remove_RejectsAUserWhoDidNotStartTheOrder()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), AttachSupportingDocumentTestData.Attached1(GrainId));

        await When(() => Order.RemoveSupportingDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Remove_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.RemoveSupportingDocumentAsync(Command1));
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (
        ClaimsPrincipal User,
        Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted,
        Func<string, DateTimeOffset?, SupportingDocumentAttached> Attached,
        RemoveSupportingDocumentCommand Command,
        Func<string, DateTimeOffset?, SupportingDocumentRemoved> Removed)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted1, AttachSupportingDocumentTestData.Attached1, Command1, Removed1),
            2 => (RequestedByUsers.ClientUser2, StartOrderDraftTestData.DraftStarted2, AttachSupportingDocumentTestData.Attached2, Command2, Removed2),
            3 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted3, AttachSupportingDocumentTestData.Attached3, Command3, Removed3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
