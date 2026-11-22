using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.TestHarness;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.StartOrderDraftTestData;

namespace GetRDone.Orders.Grains.Tests.ReadModels;

/// <summary>
///     Tests for the ClientOrderDrafts projection. Each test projects the drafts of a new client user, so it starts with
///     none.
/// </summary>
public sealed class ClientOrderDraftsTests
    : ProjectionGrainTestHarness<IClientOrderDraftsGrain, ClientOrderDraftsGrain, ClientOrderDraftsState>,
      IClassFixture<OrdersCluster>
{
    private readonly ClaimsPrincipal _user;

    public ClientOrderDraftsTests(OrdersCluster fixture) : this(fixture, RequestedByUsers.NewClientUser()) { }

    private ClientOrderDraftsTests(OrdersCluster fixture, ClaimsPrincipal user) : base(fixture.Cluster, user.GetTenantUserId())
        => _user = user;

    [Fact]
    public async Task DraftStarted_AddsTheDraftByOrderId()
    {
        var (order1, order2) = (NewOrderId(), NewOrderId());

        await Given(
            DraftStarted(Command1, order1, _user, TestData.At),
            DraftStarted(Command2, order2, _user, TestData.At.AddHours(1)));

        Then(state => Assert.Equal(
            new Dictionary<string, ClientOrderDraft>
            {
                [order1] = new(order1, "48 Birch Hollow Rd, Asheville, NC 28803", TestData.UniformResidentialAppraisal.FormTypeName, TestData.At),
                [order2] = new(order2, "715 Commerce Blvd, Greenville, SC 29601", TestData.RestrictedAppraisal.FormTypeName, TestData.At.AddHours(1))
            },
            state.Drafts));
    }

    [Fact]
    public async Task BorrowerAndAccessContactSaved_UpdatesTheDraftsLastSavedTime()
    {
        var orderId = NewOrderId();

        await Given(
            DraftStarted(Command1, orderId, _user, TestData.At),
            SaveBorrowerAndAccessContactTestData.Saved(
                SaveBorrowerAndAccessContactTestData.Command1, orderId, _user, TestData.At.AddMinutes(5)));

        Then(state => Assert.Equal(TestData.At.AddMinutes(5), state.Drafts[orderId].LastSavedAt));
    }

    [Fact]
    public async Task Drafts_ListTheUsersDraftsMostRecentlySavedFirst()
    {
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(_user);
        await Given(
            DraftStarted(Command1, order1, _user, TestData.At),
            DraftStarted(Command2, order2, _user, TestData.At.AddHours(2)),
            DraftStarted(Command3, order3, _user, TestData.At.AddHours(1)));

        await When(() => Grain.GetDraftsAsync(0, 10));

        Then<ClientOrderDraftsPage>(page =>
        {
            Assert.Equal(
            [
                new ClientOrderDraft(order2, "715 Commerce Blvd, Greenville, SC 29601", TestData.RestrictedAppraisal.FormTypeName, TestData.At.AddHours(2)),
                new ClientOrderDraft(order3, "9 Laurel Ridge Ct, Black Mountain, NC 28711", TestData.DesktopAppraisal.FormTypeName, TestData.At.AddHours(1)),
                new ClientOrderDraft(order1, "48 Birch Hollow Rd, Asheville, NC 28803", TestData.UniformResidentialAppraisal.FormTypeName, TestData.At)
            ], page.Drafts);
            Assert.Equal(3, page.TotalCount);
        });
    }

    [Fact]
    public async Task Drafts_ArePaged()
    {
        var (order1, order2, order3) = (NewOrderId(), NewOrderId(), NewOrderId());
        GivenSignedIn(_user);
        await Given(
            DraftStarted(Command1, order1, _user, TestData.At),
            DraftStarted(Command2, order2, _user, TestData.At.AddHours(2)),
            DraftStarted(Command3, order3, _user, TestData.At.AddHours(1)));

        await When(() => Grain.GetDraftsAsync(1, 1));

        Then<ClientOrderDraftsPage>(page =>
        {
            Assert.Equal(order3, Assert.Single(page.Drafts).OrderId);
            Assert.Equal(3, page.TotalCount);
        });
    }

    [Fact]
    public async Task Drafts_CanOnlyBeReadByTheirUser()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(DraftStarted(Command1, NewOrderId(), _user, TestData.At));

        await When(() => Grain.GetDraftsAsync(0, 10));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    public async Task Drafts_RejectAnInvalidPage(int skip, int take)
    {
        GivenSignedIn(_user);

        await When(() => Grain.GetDraftsAsync(skip, take));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    /// <summary>
    ///     Checks the stream wiring the harness bypasses: an event produced on the user's client orders stream, as the
    ///     stream provider does for the stream target the order grain names, reaches the user's projection through its
    ///     implicit subscription.
    /// </summary>
    [Fact]
    public async Task Drafts_AreProjectedFromTheUsersClientOrdersStream()
    {
        var orderId = NewOrderId();
        GivenSignedIn(_user);
        var evt = DraftStarted(Command1, orderId, _user, TestData.At);

        TenantKey.Split(orderId, out var tenantId, out var key);
        var stream = Cluster.Client.GetStreamProvider(OrderProjections.ProviderName)
            .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.ClientUserOrdersNamespace, _user.GetTenantUserId()));
        await stream.OnNextAsync(new StreamedEvent<object>(
            Guid.NewGuid(),
            "Orders.OrderDraftStarted.V1",
            new StreamName($"order-{orderId}", "order", tenantId, key),
            0,
            new StreamedEventCursor("kurrentdb", "order", orderId, NextPosition(), 0),
            evt.OccurredAt.UtcDateTime,
            evt));

        var page = await WaitForDraftsAsync(totalCount: 1);
        Assert.Equal(orderId, Assert.Single(page.Drafts).OrderId);
    }

    private static string NewOrderId() => TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString());

    /// <summary>Waits until the projection has applied <paramref name="totalCount"/> drafts, then returns them all.</summary>
    private async Task<ClientOrderDraftsPage> WaitForDraftsAsync(int totalCount)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var result = await Grain.GetDraftsAsync(0, int.MaxValue);
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
