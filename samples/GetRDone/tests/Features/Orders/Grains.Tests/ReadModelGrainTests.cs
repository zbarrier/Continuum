using Continuum.CSharpFunctionalExtensions;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;

namespace GetRDone.Orders.Tests;

public sealed class ReadModelGrainTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    private const string TenantId = SeedData.TenantId;

    [Fact]
    public async Task ReadModels_AreSeededForTheSampleTenantOnly()
    {
        var grainFactory = fixture.Cluster.GrainFactory;

        Assert.Equal(SeedData.IntendedUses, await grainFactory.GetIntendedUsesGrain(TenantId).GetIntendedUsesAsync());
        Assert.Equal(SeedData.OrderableServices, await grainFactory.GetOrderableServicesGrain(TenantId).GetOrderableServicesAsync());
        Assert.Equal(SeedData.Users, await grainFactory.GetUserDirectoryGrain(TenantId).GetUsersAsync());

        Assert.Empty(await grainFactory.GetIntendedUsesGrain("other").GetIntendedUsesAsync());
        Assert.Empty(await grainFactory.GetOrderableServicesGrain("other").GetOrderableServicesAsync());
        Assert.Empty(await grainFactory.GetUserDirectoryGrain("other").GetUsersAsync());
    }

    [Theory]
    [InlineData("elliott|ad86c5b7", 1, "Refinance", true)]
    [InlineData("elliott|e81d3f06", 1, "Other", true)]
    [InlineData("elliott|ad86c5b7", 2, "Refinance", false)]
    [InlineData("elliott|ad86c5b7", 1, "Purchase", false)]
    [InlineData("elliott|0d9c1b2a", 1, "Refinance", false)]
    public async Task IsCurrentIntendedUse_MatchesIdVersionAndName(string id, int version, string name, bool expected)
    {
        var result = await fixture.Cluster.GrainFactory.GetIntendedUsesGrain(TenantId).IsCurrentIntendedUseAsync(
            IntendedUseId.Create(id, TenantId).Value, IntendedUseVersion.Create(version).Value, IntendedUseName.Create(name).Value);

        Assert.Equal(expected, result.IsSuccess);
        if (!expected)
        {
            Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        }
    }

    [Theory]
    [InlineData("elliott|5a1f0c3e", 1, "Appraisal Commercial", "elliott|e5a1c9d3", 1, "Market Value Interior Inspection", true)]
    [InlineData("elliott|5a1f0c3e", 2, "Appraisal Commercial", "elliott|e5a1c9d3", 1, "Market Value Interior Inspection", false)]
    [InlineData("elliott|5a1f0c3e", 1, "Appraisal Commercial", "elliott|e5a1c9d3", 1, "Desktop Appraisal", false)]
    [InlineData("elliott|5a1f0c3e", 1, "Appraisal Commercial", "elliott|0d9c1b2a", 1, "Market Value Interior Inspection", false)]
    public async Task IsCurrentOrderableService_MatchesServiceAndForm(
        string serviceId, int serviceVersion, string serviceName, string formId, int formVersion, string formName, bool expected)
    {
        var result = await fixture.Cluster.GrainFactory.GetOrderableServicesGrain(TenantId).IsCurrentOrderableServiceAsync(
            ServiceRequestTypeId.Create(serviceId, TenantId).Value,
            ServiceRequestTypeVersion.Create(serviceVersion).Value,
            ServiceRequestTypeName.Create(serviceName).Value,
            FormTypeId.Create(formId, TenantId).Value,
            FormTypeVersion.Create(formVersion).Value,
            FormTypeName.Create(formName).Value);

        Assert.Equal(expected, result.IsSuccess);
        if (!expected)
        {
            Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        }
    }

    [Theory]
    [InlineData(SeedData.AnaRuizUserId, SeedData.CarterBankAndTrustClientId, true)]
    [InlineData(SeedData.AnaRuizUserId, "elliott|0b2e4f61", false)]
    [InlineData("elliott|3f2a9c1e-0b1d-4c8e-9a51-6d2e7f0a1b2c", SeedData.CarterBankAndTrustClientId, false)]
    public async Task IsClientUser_MatchesUserAndClient(string userId, string clientId, bool expected)
    {
        var result = await fixture.Cluster.GrainFactory.GetUserDirectoryGrain(TenantId).IsClientUserAsync(
            UserId.Create(userId, TenantId).Value, ClientId.Create(clientId, TenantId).Value);

        Assert.Equal(expected, result.IsSuccess);
    }
}
