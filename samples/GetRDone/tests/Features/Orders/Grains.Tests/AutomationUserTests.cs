using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

namespace GetRDone.Orders.Grains.Tests;

public sealed class AutomationUserTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    /// <summary>
    ///     A user's ClientOrders can only be read by that user, so reading the automation account's own succeeds only
    ///     when <c>[Authorize]</c> accepts its principal and grains read back the account's tenant user ID.
    /// </summary>
    [Fact]
    public async Task SignIn_MakesGrainCallsAsTheAccountInTheTenant()
    {
        var account = AutomationUser.OrderNumberAssigner;
        account.SignIn(TestData.TenantId);

        var result = await fixture.Cluster.GrainFactory
            .GetClientOrdersGrain(account.GetTenantUserId(TestData.TenantId))
            .GetOrdersAsync(0, 10);

        Assert.True(result.IsSuccess);
    }
}
