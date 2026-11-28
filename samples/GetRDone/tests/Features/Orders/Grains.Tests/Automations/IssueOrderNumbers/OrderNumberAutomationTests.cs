using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderNumberGeneratorAggregate;
using GetRDone.Orders.Automations;
using GetRDone.Orders.Automations.IssueOrderNumbers;
using GetRDone.Orders.Grains.Tests.Data;

using Microsoft.Extensions.Logging.Abstractions;

namespace GetRDone.Orders.Grains.Tests.Automations.IssueOrderNumbers;

/// <summary>
///     Tests for the automation that has the generators issue submitted orders their numbers, called as its subscription
///     calls it with the events of a batch. The automation signs itself in; the tests sign in only to check the generators.
///     Each order then assigns itself its number (see <c>AssignOrderNumberTests</c>).
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

    private OrderNumberIssuer Issuer => new(GrainFactory, NullLogger<OrderNumberIssuer>.Instance);

    private static string NewTenantId() => "t" + Guid.NewGuid().ToString("N");

    private static string NewOrderId(string tenantId) => TenantKey.Create(tenantId, Guid.NewGuid().ToString());

    /// <summary>Asks the tenant's generator for the numbers of <paramref name="orderIds"/>, which issues none that weren't issued.</summary>
    private async Task<IReadOnlyDictionary<string, long>> IssuedNumbersAsync(string tenantId, params string[] orderIds)
    {
        AutomationUser.OrderNumberAssigner.SignIn(tenantId);
        var result = await GrainFactory.GetOrderNumberGeneratorGrain(tenantId).IssueOrderNumbersAsync(orderIds);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Changes);
        return result.Value.OrderNumbers;
    }
}
