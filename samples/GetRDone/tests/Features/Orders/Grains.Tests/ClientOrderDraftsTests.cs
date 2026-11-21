using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.StartOrderDraftTestData;

namespace GetRDone.Orders.Grains.Tests;

/// <summary>
///     Tests for the ClientOrderDrafts projection. The events are produced on the user's client orders stream the way the
///     KurrentDB stream provider delivers them; each test uses a new user so it starts with no drafts.
/// </summary>
public sealed class ClientOrderDraftsTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    private static long _commitPosition;

    [Fact]
    public async Task Drafts_ListTheUsersDraftsMostRecentlySavedFirst()
    {
        var user = RequestedByUsers.NewClientUser();
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(user);
        await GivenProjected(
            DraftStarted(Command1, order1, user, TestData.At),
            DraftStarted(Command2, order2, user, TestData.At.AddHours(2)),
            DraftStarted(Command3, order3, user, TestData.At.AddHours(1)));

        var page = await WhenProjectedAsync(user, totalCount: 3);

        Assert.Equal(
        [
            new ClientOrderDraft(order2, "715 Commerce Blvd, Greenville, SC 29601", TestData.RestrictedAppraisal.FormTypeName, TestData.At.AddHours(2)),
            new ClientOrderDraft(order3, "9 Laurel Ridge Ct, Black Mountain, NC 28711", TestData.DesktopAppraisal.FormTypeName, TestData.At.AddHours(1)),
            new ClientOrderDraft(order1, "48 Birch Hollow Rd, Asheville, NC 28803", TestData.UniformResidentialAppraisal.FormTypeName, TestData.At)
        ], page.Drafts);
    }

    [Fact]
    public async Task Drafts_ArePaged()
    {
        var user = RequestedByUsers.NewClientUser();
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(user);
        await GivenProjected(
            DraftStarted(Command1, order1, user, TestData.At),
            DraftStarted(Command2, order2, user, TestData.At.AddHours(2)),
            DraftStarted(Command3, order3, user, TestData.At.AddHours(1)));
        await WhenProjectedAsync(user, totalCount: 3);

        var result = await fixture.Cluster.GrainFactory.GetClientOrderDraftsGrain(user.GetTenantUserId()).GetDraftsAsync(1, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(order3, Assert.Single(result.Value.Drafts).OrderId);
        Assert.Equal(3, result.Value.TotalCount);
    }

    [Fact]
    public async Task Drafts_CanOnlyBeReadByTheirUser()
    {
        var user = RequestedByUsers.NewClientUser();
        GivenSignedIn(RequestedByUsers.ClientUser1);

        var result = await fixture.Cluster.GrainFactory.GetClientOrderDraftsGrain(user.GetTenantUserId()).GetDraftsAsync(0, 10);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.PermissionDenied, result.Error.Code);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    public async Task Drafts_RejectAnInvalidPage(int skip, int take)
    {
        var user = RequestedByUsers.NewClientUser();
        GivenSignedIn(user);

        var result = await fixture.Cluster.GrainFactory.GetClientOrderDraftsGrain(user.GetTenantUserId()).GetDraftsAsync(skip, take);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>
    ///     Produces each event on the client orders stream of the user who started the order, as the stream provider does for
    ///     the stream target the order grain names. The event keeps its order stream name and cursor.
    /// </summary>
    private async Task GivenProjected(params OrderDraftStarted[] events)
    {
        var provider = fixture.Cluster.Client.GetStreamProvider(OrderProjections.ProviderName);
        foreach (var evt in events)
        {
            TenantKey.Split(evt.OrderId, out var tenantId, out var key);
            var streamName = new StreamName($"order-{evt.OrderId}", "order", tenantId, key);
            var cursor = new StreamedEventCursor("kurrentdb", "order", evt.OrderId, Interlocked.Increment(ref _commitPosition), 0);
            var stream = provider.GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.ClientUserOrdersNamespace, evt.InitiatedBy));
            await stream.OnNextAsync(new StreamedEvent<object>(
                Guid.NewGuid(), "Orders.OrderDraftStarted.V1", streamName, 0, cursor, evt.OccurredAt.UtcDateTime, evt));
        }
    }

    /// <summary>Waits until the projection has applied <paramref name="totalCount"/> drafts, then returns them all.</summary>
    private async Task<ClientOrderDraftsPage> WhenProjectedAsync(ClaimsPrincipal user, int totalCount)
    {
        var grain = fixture.Cluster.GrainFactory.GetClientOrderDraftsGrain(user.GetTenantUserId());
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var result = await grain.GetDraftsAsync(0, int.MaxValue);
            Assert.True(result.IsSuccess);
            if (result.Value.TotalCount >= totalCount)
            {
                Assert.Equal(totalCount, result.Value.TotalCount);
                return result.Value;
            }
            await Task.Delay(50, timeout.Token);
        }
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
