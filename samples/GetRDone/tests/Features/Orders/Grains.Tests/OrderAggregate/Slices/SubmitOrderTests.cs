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

using static GetRDone.Orders.Grains.Tests.Data.SubmitOrderTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for submitting a draft. Each test uses a new order in the test tenant. The first test checks what
///     applying the slice's event does to the order state and the second checks every value of the event the slice records;
///     the behavior tests follow.
/// </summary>
public sealed class SubmitOrderTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public SubmitOrderTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingOrderSubmitted_MovesTheOrderToSubmitted()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        var attached = AttachSupportingDocumentTestData.Attached1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(attached);

        state.When(Submitted1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Submitted,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string> { [attached.DocumentId] = attached.BlobPath },
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)null
        }, state, strict: true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Submit_RecordsTheSubmissionForTheDraftsClient(int variation)
    {
        var (user, draftStarted, borrowerSaved, submitted) = Variation(variation);
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(user);
        await Given(draftStarted(GrainId, null), borrowerSaved(GrainId, null));

        await When(() => Order.SubmitOrderAsync());

        Then<SubmitOrderResponse>(response =>
        {
            Assert.Equal(3, response.Version);
            var recorded = Assert.IsType<OrderSubmitted>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(submitted(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Submit_SubmitsTheSupportingDocumentsWithTheDraft()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
            AttachSupportingDocumentTestData.Attached1(GrainId));

        await When(() => Order.SubmitOrderAsync());

        Then<SubmitOrderResponse>(response => Assert.Equal(4, response.Version));
    }

    [Fact]
    public async Task Submit_RejectsADraftWithoutABorrower()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), AttachSupportingDocumentTestData.Attached1(GrainId));

        await When(() => Order.SubmitOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsAnOrderThatWasAlreadySubmitted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), SaveBorrowerAndAccessContactTestData.Saved1(GrainId), Submitted1(GrainId));

        await When(() => Order.SubmitOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.SubmitOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsAUserWhoDidNotStartTheOrder()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), SaveBorrowerAndAccessContactTestData.Saved1(GrainId));

        await When(() => Order.SubmitOrderAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SubmitOrderAsync());
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (
        ClaimsPrincipal User,
        Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted,
        Func<string, DateTimeOffset?, BorrowerAndAccessContactSaved> BorrowerSaved,
        Func<string, DateTimeOffset?, OrderSubmitted> Submitted)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted1, SaveBorrowerAndAccessContactTestData.Saved1, Submitted1),
            2 => (RequestedByUsers.ClientUser2, StartOrderDraftTestData.DraftStarted2, SaveBorrowerAndAccessContactTestData.Saved2, Submitted2),
            3 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted3, SaveBorrowerAndAccessContactTestData.Saved3, Submitted3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
