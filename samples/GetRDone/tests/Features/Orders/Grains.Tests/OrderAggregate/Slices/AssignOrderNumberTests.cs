using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumbers;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;
using Orleans.Streams;

using static GetRDone.Orders.Grains.Tests.Data.AssignOrderNumberTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for giving a submitted order its number. Each test uses a new order in the test tenant. The
///     first test checks what applying the slice's event does to the order state and the second checks every value of the
///     event the slice records; the behavior tests follow, then the tests of the order's reaction to an issued number.
/// </summary>
public sealed class AssignOrderNumberTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public AssignOrderNumberTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    private static ClaimsPrincipal Automation => AutomationUser.OrderNumberAssigner.CreatePrincipal(TestData.TenantId);

    [Fact]
    public void ApplyingOrderNumberAssigned_RecordsTheOrderNumber()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));

        state.When(Assigned1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Submitted,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)FirstOrderNumber,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)null,
            VendorAssignment = (OrderVendorAssignment?)null,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)null
        }, state, strict: true);
    }

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

    /// <summary>
    ///     Checks the reaction the harness bypasses: a number issued to the order and produced on the order's reactor stream,
    ///     as the stream provider does for the stream target the generator names, is assigned by the order itself, signed
    ///     in as the automation. It is published signed out, as the stream provider delivers it with no signed-in user.
    /// </summary>
    [Fact]
    public async Task OrderNumberIssued_GivesTheSubmittedOrderItsNumber()
    {
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await GivenSubmitted();
        RequestContext.Clear();

        await ReactorStream.OnNextAsync(OnGeneratorStream(Issued(GrainId, FirstOrderNumber)));

        var assigned = await WaitForAssignedAsync();
        Assert.InRange(assigned.OccurredAt, before, DateTimeOffset.UtcNow);
        Assert.Equivalent(Assigned1(GrainId, FirstOrderNumber, assigned.OccurredAt), assigned, strict: true);
    }

    /// <summary>
    ///     A number issued to another order is ignored without holding back the order's stream, as the stream provider
    ///     delivers the events of one stream one after another.
    /// </summary>
    [Fact]
    public async Task OrderNumberIssued_IgnoresANumberIssuedToAnotherOrder()
    {
        GivenSignedIn(Automation);
        await GivenSubmitted();
        RequestContext.Clear();
        var otherOrderId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

        await ReactorStream.OnNextAsync(OnGeneratorStream(Issued(otherOrderId, FirstOrderNumber)));
        await ReactorStream.OnNextAsync(OnGeneratorStream(Issued(GrainId, FirstOrderNumber + 1)));

        Assert.Equal(FirstOrderNumber + 1, (await WaitForAssignedAsync()).OrderNumber);
    }

    private static long _position;

    /// <summary>The order's stream on the reactor provider, which the order subscribes to implicitly.</summary>
    private IAsyncStream<IStreamedEvent<object>> ReactorStream => Cluster.Client.GetStreamProvider(OrderReactors.ProviderName)
        .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderReactors.OrderNamespace, GrainId));

    private static OrderNumberIssued Issued(string orderId, long orderNumber) => new()
    {
        OrderId = orderId,
        OrderNumber = orderNumber,
        InitiatedBy = AutomationUser.OrderNumberAssigner.GetTenantUserId(TestData.TenantId),
        OccurredAt = TestData.At
    };

    /// <summary>A number issued by the test tenant's generator, as the stream provider delivers it from the generator's stream.</summary>
    private static StreamedEvent<object> OnGeneratorStream(OrderNumberIssued issued)
    {
        var generatorKey = TenantKey.Create(TestData.TenantId, ReadModelGrainFactoryExtensions.DefaultKey);
        var position = (ulong)Interlocked.Increment(ref _position);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.OrderNumberIssued.V1",
            new StreamName($"orderNumberGenerator-{generatorKey}", "orderNumberGenerator", TestData.TenantId, ReadModelGrainFactoryExtensions.DefaultKey),
            position,
            new StreamedEventCursor("kurrentdb", "orderNumberGenerator", generatorKey, position, 0),
            issued.OccurredAt.UtcDateTime,
            issued);
    }

    /// <summary>Waits until the order has saved its number assignment, then returns it.</summary>
    private async Task<OrderNumberAssigned> WaitForAssignedAsync()
    {
        GivenSignedIn(Automation);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var assigned = (await Order.GetSavedEventsSince(0)).OfType<OrderNumberAssigned>().ToList();
            if (assigned.Count > 0)
            {
                return Assert.Single(assigned);
            }
            await Task.Delay(50, timeout.Token);
        }
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
