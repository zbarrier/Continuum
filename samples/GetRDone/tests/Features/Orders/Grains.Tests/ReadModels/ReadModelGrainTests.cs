using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

namespace GetRDone.Orders.Grains.Tests.ReadModels;

/// <summary>Tests for the pre-populated read models, seeded with <see cref="TestData"/> for the test tenant.</summary>
public sealed class ReadModelGrainTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    private const string TenantId = TestData.TenantId;

    [Fact]
    public async Task ReadModels_AreSeededForTheTestTenantOnly()
    {
        var grainFactory = fixture.Cluster.GrainFactory;
        GivenSignedIn(RequestedByUsers.ClientUser1);

        Assert.Equal(TestData.IntendedUses, await grainFactory.GetIntendedUsesGrain(TenantId).GetIntendedUsesAsync());
        Assert.Equal(TestData.DocumentTypes, await grainFactory.GetDocumentTypesGrain(TenantId).GetDocumentTypesAsync());
        Assert.Equivalent(TestData.OrderableServices, await grainFactory.GetOrderableServicesGrain(TenantId).GetOrderableServicesAsync(), strict: true);
        Assert.Equal(TestData.Users, await grainFactory.GetUserDirectoryGrain(TenantId).GetUsersAsync());

        Assert.Empty(await grainFactory.GetIntendedUsesGrain(TestData.OtherTenantId).GetIntendedUsesAsync());
        Assert.Empty(await grainFactory.GetDocumentTypesGrain(TestData.OtherTenantId).GetDocumentTypesAsync());
        Assert.Empty(await grainFactory.GetOrderableServicesGrain(TestData.OtherTenantId).GetOrderableServicesAsync());
        Assert.Empty(await grainFactory.GetUserDirectoryGrain(TestData.OtherTenantId).GetUsersAsync());
    }

    [Theory]
    [InlineData("summit|7a1d0c42", 2, "Refinance", true)]
    [InlineData("summit|3e8b5f19", 1, "Purchase", true)]
    [InlineData("summit|7a1d0c42", 1, "Refinance", false)]
    [InlineData("summit|7a1d0c42", 2, "Purchase", false)]
    [InlineData("summit|0d9c1b2a", 2, "Refinance", false)]
    public async Task IsCurrentIntendedUse_MatchesIdVersionAndName(string id, int version, string name, bool expected)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);

        var result = await fixture.Cluster.GrainFactory.GetIntendedUsesGrain(TenantId).IsCurrentIntendedUseAsync(
            IntendedUseRef.Create(id, version, name, TenantId).Value);

        Assert.Equal(expected, result.IsSuccess);
        if (!expected)
        {
            Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        }
    }

    [Theory]
    [InlineData("summit|4a8e2c61", 2, "Purchase contract", true)]
    [InlineData("summit|c3f7b905", 1, "Survey", true)]
    [InlineData("summit|4a8e2c61", 1, "Purchase contract", false)]
    [InlineData("summit|4a8e2c61", 2, "Survey", false)]
    [InlineData("summit|0d9c1b2a", 1, "Survey", false)]
    public async Task IsCurrentDocumentType_MatchesIdVersionAndName(string id, int version, string name, bool expected)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);

        var result = await fixture.Cluster.GrainFactory.GetDocumentTypesGrain(TenantId).IsCurrentDocumentTypeAsync(
            DocumentTypeRef.Create(id, version, name, TenantId).Value);

        Assert.Equal(expected, result.IsSuccess);
        if (!expected)
        {
            Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        }
    }

    [Theory]
    [InlineData("summit|6c4f2a90", 3, "Residential Appraisal", "summit|d05e9b37", 1, "Uniform Residential Appraisal Report", true)]
    [InlineData("summit|6c4f2a90", 3, "Residential Appraisal", "summit|8e2b6f41", 2, "Desktop Appraisal", true)]
    [InlineData("summit|4b7e1d03", 1, "Commercial Appraisal", "summit|a2c6f8e0", 1, "Narrative Appraisal Report", true)]
    [InlineData("summit|6c4f2a90", 2, "Residential Appraisal", "summit|d05e9b37", 1, "Uniform Residential Appraisal Report", false)]
    [InlineData("summit|6c4f2a90", 3, "Residential Appraisal", "summit|8e2b6f41", 1, "Desktop Appraisal", false)]
    [InlineData("summit|6c4f2a90", 3, "Residential Appraisal", "summit|d05e9b37", 1, "Desktop Appraisal", false)]
    [InlineData("summit|6c4f2a90", 3, "Residential Appraisal", "summit|0d9c1b2a", 1, "Uniform Residential Appraisal Report", false)]
    [InlineData("summit|6c4f2a90", 3, "Residential Appraisal", "summit|a2c6f8e0", 1, "Narrative Appraisal Report", false)]
    [InlineData("summit|4b7e1d03", 1, "Commercial Appraisal", "summit|8e2b6f41", 2, "Desktop Appraisal", false)]
    public async Task IsCurrentOrderableService_MatchesServiceAndForm(
        string serviceId, int serviceVersion, string serviceName, string formId, int formVersion, string formName, bool expected)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);

        var result = await fixture.Cluster.GrainFactory.GetOrderableServicesGrain(TenantId).IsCurrentOrderableServiceAsync(
            ServiceRequestTypeRef.Create(serviceId, serviceVersion, serviceName, TenantId).Value,
            FormTypeRef.Create(formId, formVersion, formName, TenantId).Value);

        Assert.Equal(expected, result.IsSuccess);
        if (!expected)
        {
            Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        }
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.ClientUser1), TestData.LakeviewCreditUnionClientId, true)]
    [InlineData(nameof(RequestedByUsers.ClientUser2), TestData.HarborNationalBankClientId, true)]
    [InlineData(nameof(RequestedByUsers.ClientUser1), TestData.HarborNationalBankClientId, false)]
    [InlineData(nameof(RequestedByUsers.Director1), TestData.LakeviewCreditUnionClientId, false)]
    public async Task IsClientUser_MatchesUserAndClient(string user, string clientId, bool expected)
    {
        var principal = user switch
        {
            nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
            nameof(RequestedByUsers.ClientUser2) => RequestedByUsers.ClientUser2,
            nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
            _ => throw new ArgumentOutOfRangeException(nameof(user))
        };
        GivenSignedIn(principal);

        var result = await fixture.Cluster.GrainFactory.GetUserDirectoryGrain(TenantId).IsClientUserAsync(
            UserId.Create(principal.GetTenantUserId(), TenantId).Value, ClientId.Create(clientId, TenantId).Value);

        Assert.Equal(expected, result.IsSuccess);
    }

    [Fact]
    public async Task IsClientUser_RejectsAnUnknownUser()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);

        var result = await fixture.Cluster.GrainFactory.GetUserDirectoryGrain(TenantId).IsClientUserAsync(
            UserId.Create(TenantKey.Create(TenantId, Guid.NewGuid().ToString()), TenantId).Value,
            ClientId.Create(TestData.LakeviewCreditUnionClientId, TenantId).Value);

        Assert.True(result.IsFailure);
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
