using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderNumbers;
using GetRDone.Orders.Grains.OrderNumbers;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

namespace GetRDone.Orders.Grains.Tests.OrderNumbers;

/// <summary>
///     Given/When/Then tests for issuing order numbers. A tenant has one generator, so each test uses a new tenant to
///     start from a generator that has issued nothing. The first test checks what applying the generator's event does to
///     its state and the second checks every value of the event it records; the behavior tests follow.
/// </summary>
public sealed class OrderNumberGeneratorTests
    : EventDrivenGrainTestHarness<IOrderNumberGeneratorGrain, OrderNumberGeneratorGrain, OrderNumberGeneratorState, OrderNumberGeneratorEvent>,
      IClassFixture<OrdersCluster>
{
    private readonly string _tenantId;

    public OrderNumberGeneratorTests(OrdersCluster fixture) : this(fixture, "t" + Guid.NewGuid().ToString("N")) { }

    private OrderNumberGeneratorTests(OrdersCluster fixture, string tenantId)
        : base(fixture.Cluster, TenantKey.Create(tenantId, "default"))
        => _tenantId = tenantId;

    private IOrderNumberGeneratorGrain Generator => Cluster.GrainFactory.GetOrderNumberGeneratorGrain(_tenantId);

    [Fact]
    public void ApplyingOrderNumberIssued_RecordsTheOrdersNumberAndTheLastNumber()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        var state = new OrderNumberGeneratorState();
        state.When(Issued(TestData.At, order1, 610_696));

        state.When(Issued(TestData.At, order2, 610_697));

        Assert.Equivalent(new
        {
            LastOrderNumber = 610_697L,
            OrderNumbers = new Dictionary<string, long> { [order1] = 610_696, [order2] = 610_697 },
            RecentOrderIds = new[] { order1, order2 }
        }, state, strict: true);
    }

    [Fact]
    public void ApplyingOrderNumberIssued_KeepsOnlyTheMostRecentOrders()
    {
        var orderIds = Enumerable.Range(0, OrderNumberGeneratorGrain.RecentEventCount + 1).Select(_ => NewOrderId()).ToArray();
        var state = new OrderNumberGeneratorState();
        foreach (var (orderId, i) in orderIds.Select((orderId, i) => (orderId, i)))
        {
            state.When(Issued(TestData.At, orderId, 610_696 + i));
        }

        Assert.Equal(610_696 + OrderNumberGeneratorGrain.RecentEventCount, state.LastOrderNumber);
        Assert.Equal(orderIds.Skip(1).Select((orderId, i) => (orderId, Number: 610_697L + i)).ToDictionary(), state.OrderNumbers);
        Assert.Equal(orderIds.Skip(1), state.RecentOrderIds);
    }

    [Fact]
    public async Task Issue_IssuesTheTenantsFirstNumbersInTheOrderAskedFor()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        var before = DateTimeOffset.UtcNow;
        GivenSignedIn(Automation);
        await Given();

        await When(() => Generator.IssueOrderNumbersAsync([order1, order2]));

        Then<IssueOrderNumbersResponse>(response =>
        {
            Assert.Equal(2, response.Version);
            var recorded = Events(response.Changes).Select(Assert.IsType<OrderNumberIssued>).ToArray();
            Assert.Equal(2, recorded.Length);
            Assert.InRange(recorded[0].OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(
                new[] { Issued(recorded[0].OccurredAt, order1, 610_696), Issued(recorded[0].OccurredAt, order2, 610_697) },
                recorded, strict: true);
            Assert.Equal(new Dictionary<string, long> { [order1] = 610_696, [order2] = 610_697 }, response.OrderNumbers);
        });
    }

    [Fact]
    public async Task Issue_ContinuesFromTheLastNumberIssued()
    {
        var order = NewOrderId();
        GivenSignedIn(Automation);
        await Given(Issued(TestData.At, NewOrderId(), 610_696), Issued(TestData.At, NewOrderId(), 610_697));

        await When(() => Generator.IssueOrderNumbersAsync([order]));

        Then<IssueOrderNumbersResponse>(response =>
            Assert.Equal(new Dictionary<string, long> { [order] = 610_698 }, response.OrderNumbers));
    }

    [Fact]
    public async Task Issue_ReturnsTheNumbersAlreadyIssuedWithoutIssuingAgain()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());
        GivenSignedIn(Automation);
        await Given(Issued(TestData.At, order1, 610_696), Issued(TestData.At, order2, 610_697));

        await When(() => Generator.IssueOrderNumbersAsync([order2, order1, order2]));

        Then<IssueOrderNumbersResponse>(response =>
        {
            Assert.Empty(response.Changes);
            Assert.Equal(new Dictionary<string, long> { [order1] = 610_696, [order2] = 610_697 }, response.OrderNumbers);
        });
        await ThenNoChanges();
    }

    [Fact]
    public async Task Issue_IssuesOnlyTheOrdersWithoutANumber()
    {
        var (issuedOrder, newOrder) = (NewOrderId(), NewOrderId());
        GivenSignedIn(Automation);
        await Given(Issued(TestData.At, issuedOrder, 610_696));

        await When(() => Generator.IssueOrderNumbersAsync([issuedOrder, newOrder]));

        Then<IssueOrderNumbersResponse>(response =>
        {
            var recorded = Assert.IsType<OrderNumberIssued>(Assert.Single(Events(response.Changes)));
            Assert.Equal((newOrder, 610_697L), (recorded.OrderId, recorded.OrderNumber));
            Assert.Equal(new Dictionary<string, long> { [issuedOrder] = 610_696, [newOrder] = 610_697 }, response.OrderNumbers);
        });
    }

    [Fact]
    public async Task Issue_RejectsAnOrderOfAnotherTenant()
    {
        GivenSignedIn(Automation);
        await Given();

        await When(() => Generator.IssueOrderNumbersAsync([NewOrderId(), TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())]));

        ThenError<ValidationError>(error => Assert.Equal("OrderIds[1]", Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Issue_RejectsAUserWhoIsNotTheAutomation()
    {
        GivenSignedIn(RequestedByUsers.InTenant(RequestedByUsers.ClientUser1, _tenantId));
        await Given();

        await When(() => Generator.IssueOrderNumbersAsync([NewOrderId()]));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Issue_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Generator.IssueOrderNumbersAsync([NewOrderId()]));
    }

    private ClaimsPrincipal Automation => AutomationUser.OrderNumberAssigner.CreatePrincipal(_tenantId);

    private string NewOrderId() => TenantKey.Create(_tenantId, Guid.NewGuid().ToString());

    private OrderNumberIssued Issued(DateTimeOffset occurredAt, string orderId, long orderNumber) => new()
    {
        OrderId = orderId,
        OrderNumber = orderNumber,
        InitiatedBy = AutomationUser.OrderNumberAssigner.GetTenantUserId(_tenantId),
        OccurredAt = occurredAt
    };

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static IEnumerable<object> Events(IReadOnlyList<IChange> changes) => changes.Select(change => Assert.IsType<Change>(change).Event);
}
