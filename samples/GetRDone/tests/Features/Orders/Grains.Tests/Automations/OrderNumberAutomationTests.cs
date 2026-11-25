using System.Security.Claims;

using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumbers;
using GetRDone.Orders.Automations;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Microsoft.Extensions.Logging.Abstractions;

namespace GetRDone.Orders.Grains.Tests.Automations;

/// <summary>
///     Tests for the two automations that give submitted orders their numbers, called as their subscriptions call them
///     with the events of a batch. The automations sign themselves in; the tests sign in only to set up and check grains.
/// </summary>
public sealed class OrderNumberAutomationTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    private IGrainFactory GrainFactory => fixture.Cluster.GrainFactory;

    [Fact]
    public async Task Issuer_HasEachTenantsGeneratorIssueNumbersToItsSubmittedOrders()
    {
        var (tenant1, tenant2) = (NewTenantId(), NewTenantId());
        var (order1, order2, order3) = (NewOrderId(tenant1), NewOrderId(tenant2), NewOrderId(tenant1));
        RequestContext.Clear();

        await Issuer.IssueAsync(
        [
            SubmitOrderTestData.Submitted1(order1),
            SubmitOrderTestData.Submitted2(order2),
            SubmitOrderTestData.Submitted3(order3),
            SubmitOrderTestData.Submitted1(order1)
        ]);

        Assert.Equal(new Dictionary<string, long> { [order1] = 610_696, [order3] = 610_697 }, await IssuedNumbersAsync(tenant1, order1, order3));
        Assert.Equal(new Dictionary<string, long> { [order2] = 610_696 }, await IssuedNumbersAsync(tenant2, order2));
    }

    [Fact]
    public async Task Issuer_IssuesNothingAgainWhenABatchIsRetried()
    {
        var tenant = NewTenantId();
        var (order1, order2) = (NewOrderId(tenant), NewOrderId(tenant));
        await Issuer.IssueAsync([SubmitOrderTestData.Submitted1(order1)]);

        await Issuer.IssueAsync([SubmitOrderTestData.Submitted1(order1), SubmitOrderTestData.Submitted1(order2)]);

        Assert.Equal(new Dictionary<string, long> { [order1] = 610_696, [order2] = 610_697 }, await IssuedNumbersAsync(tenant, order1, order2));
    }

    [Fact]
    public async Task Issuer_SkipsOnlyAnOrderThatCanNeverBeIssuedANumber()
    {
        var tenant = NewTenantId();
        var (order1, corruptOrder, order2) = (NewOrderId(tenant), TenantKey.Create(tenant, "not-a-guid"), NewOrderId(tenant));

        await Issuer.IssueAsync(
        [
            SubmitOrderTestData.Submitted1(order1),
            SubmitOrderTestData.Submitted1(corruptOrder),
            SubmitOrderTestData.Submitted1(order2)
        ]);

        Assert.Equal(new Dictionary<string, long> { [order1] = 610_696, [order2] = 610_697 }, await IssuedNumbersAsync(tenant, order1, order2));
    }

    [Fact]
    public async Task Assigner_GivesEachSubmittedOrderItsNumber()
    {
        var (order1, order2) = (NewOrderId(TestData.TenantId), NewOrderId(TestData.TenantId));
        await GivenSubmittedAsync(order1, order2);

        await Assigner.AssignAsync([Issued(order1, 610_696), Issued(order2, 610_697)]);

        Assert.Equal(610_696, await AssignedNumberAsync(order1));
        Assert.Equal(610_697, await AssignedNumberAsync(order2));
    }

    [Fact]
    public async Task Assigner_SkipsAnOrderThatCanNotBeGivenItsNumber()
    {
        var (submittedOrder, draftOrder) = (NewOrderId(TestData.TenantId), NewOrderId(TestData.TenantId));
        await GivenSubmittedAsync(submittedOrder);
        await GivenAsync(draftOrder, StartOrderDraftTestData.DraftStarted1(draftOrder));

        await Assigner.AssignAsync([Issued(draftOrder, 610_696), Issued(submittedOrder, 610_697)]);

        Assert.Equal(610_697, await AssignedNumberAsync(submittedOrder));
        Assert.Null(await AssignedNumberAsync(draftOrder));
    }

    [Fact]
    public async Task Assigner_AssignsNothingAgainWhenABatchIsRetried()
    {
        var order = NewOrderId(TestData.TenantId);
        await GivenSubmittedAsync(order);
        var assigner = Assigner;
        await assigner.AssignAsync([Issued(order, 610_696)]);

        await assigner.AssignAsync([Issued(order, 610_696)]);

        SignIn(RequestedByUsers.ClientUser1);
        Assert.Single((await GrainFactory.GetGrain<IOrderGrain>(order).GetSavedEventsSince(0)).OfType<OrderNumberAssigned>());
    }

    private OrderNumberIssuer Issuer => new(GrainFactory, NullLogger<OrderNumberIssuer>.Instance);

    private OrderNumberAssigner Assigner => new(GrainFactory, NullLogger<OrderNumberAssigner>.Instance);

    private static string NewTenantId() => "t" + Guid.NewGuid().ToString("N");

    private static string NewOrderId(string tenantId) => TenantKey.Create(tenantId, Guid.NewGuid().ToString());

    private static OrderNumberIssued Issued(string orderId, long orderNumber) => new()
    {
        OrderId = orderId,
        OrderNumber = orderNumber,
        InitiatedBy = AutomationUser.OrderNumberAssigner.GetTenantUserId(TestData.TenantId),
        OccurredAt = TestData.At
    };

    /// <summary>Sets up submitted orders in the test tenant.</summary>
    private async Task GivenSubmittedAsync(params string[] orderIds)
    {
        foreach (var orderId in orderIds)
        {
            await GivenAsync(orderId,
                StartOrderDraftTestData.DraftStarted1(orderId),
                SaveBorrowerAndAccessContactTestData.Saved1(orderId),
                SubmitOrderTestData.Submitted1(orderId));
        }
    }

    private async Task GivenAsync(string orderId, params object[] events)
    {
        SignIn(RequestedByUsers.ClientUser1);
        await GrainFactory.GetGrain<IOrderGrain>(orderId).LoadGivenEvents(events);
        RequestContext.Clear();
    }

    /// <summary>Asks the tenant's generator for the numbers of <paramref name="orderIds"/>, which issues none that weren't issued.</summary>
    private async Task<IReadOnlyDictionary<string, long>> IssuedNumbersAsync(string tenantId, params string[] orderIds)
    {
        AutomationUser.OrderNumberAssigner.SignIn(tenantId);
        var result = await GrainFactory.GetOrderNumberGeneratorGrain(tenantId).IssueOrderNumbersAsync(orderIds);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Changes);
        return result.Value.OrderNumbers;
    }

    /// <summary>The number the order's saved events gave it, or <see langword="null"/>.</summary>
    private async Task<long?> AssignedNumberAsync(string orderId)
    {
        SignIn(RequestedByUsers.ClientUser1);
        var events = await GrainFactory.GetGrain<IOrderGrain>(orderId).GetSavedEventsSince(0);
        return events.OfType<OrderNumberAssigned>().SingleOrDefault()?.OrderNumber;
    }

    private static void SignIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
