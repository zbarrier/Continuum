using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

namespace GetRDone.Orders.Tests;

public sealed class OrderGrainTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    [Fact]
    public async Task StartDraft_PersistsEventAndRejectsDuplicate()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var offerings = fixture.Cluster.GrainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create("demo", "offerings"));
        var offering = Assert.Single(await offerings.GetOfferingsAsync());
        var order = fixture.Cluster.GrainFactory.GetGrain<IOrderGrain>(TenantKey.Create("demo", orderId.ToString()));
        var command = new StartOrderDraftCommand(offering.ServiceRequestTypeId, offering.ServiceRequestTypeVersion,
            offering.FormTypeId, offering.FormTypeVersion, "1200 Main St", "Roanoke", "VA", "24011", "Office",
            "refinance", "1", "Refinance", "CB-2026-0412", new DateOnly(2026, 12, 4));
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test")));
        try
        {
            var result = await order.StartOrderDraftAsync(command);
            Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : null);
            Assert.Equal(1, result.Value.Version);
            var saved = Assert.IsType<OrderDraftStarted>(Assert.Single(await order.GetSavedEventsSince(0)));
            Assert.Equal(orderId, saved.OrderId);
            Assert.Equal(userId, saved.InitiatedBy);
            Assert.Equal(offering.ServiceRequestTypeName, saved.ServiceRequestTypeName);
            Assert.Equal(offering.FormTypeName, saved.FormTypeName);
            Assert.Equal(command.PropertyStreet, saved.PropertyStreet);

            var duplicate = await order.StartOrderDraftAsync(command);
            Assert.True(duplicate.IsFailure);
            Assert.Equal(ErrorCodes.AlreadyExists, duplicate.Error.Code);
            Assert.Single(await order.GetSavedEventsSince(0));
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }
}
