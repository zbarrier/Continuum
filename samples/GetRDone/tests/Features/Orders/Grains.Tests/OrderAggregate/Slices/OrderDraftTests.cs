using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using static GetRDone.Orders.Grains.Tests.Data.SaveBorrowerAndAccessContactTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the OrderDraft read model the order builds from its own events. Each test uses a new order
///     in the test tenant.
/// </summary>
public sealed class OrderDraftTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    private static readonly DateTimeOffset SavedAt = TestData.At.AddMinutes(5);

    public OrderDraftTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetOrderDraft_ShowsTheStartedDraft(int variation)
    {
        var (user, command, draftStarted) = Variation(variation);
        GivenSignedIn(user);
        await Given(draftStarted(GrainId, null));

        await WhenQuery(() => Order.GetOrderDraftAsync());

        ThenQuery<OrderDraft>(draft => Assert.Equal(new OrderDraft(
            GrainId,
            command.ServiceRequestTypeId,
            command.ServiceRequestTypeVersion,
            command.ServiceRequestTypeName,
            command.FormTypeId,
            command.FormTypeVersion,
            command.FormTypeName,
            command.PropertyStreet,
            command.PropertyCity,
            command.PropertyState,
            command.PropertyZip,
            command.PropertyType,
            command.IntendedUseId,
            command.IntendedUseVersion,
            command.IntendedUseName,
            command.LoanNumber,
            command.DueDate,
            BorrowerName: null,
            AccessContactName: null,
            AccessContactPhone: null,
            TestData.At), draft));
    }

    [Fact]
    public async Task GetOrderDraft_ShowsEverythingSavedSoFar()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), Saved1(GrainId, SavedAt));

        await WhenQuery(() => Order.GetOrderDraftAsync());

        ThenQuery<OrderDraft>(draft =>
        {
            Assert.Equal("48 Birch Hollow Rd, Asheville, NC 28803", draft.PropertyAddress);
            Assert.Equal(Command1.BorrowerName, draft.BorrowerName);
            Assert.Equal(Command1.AccessContactName, draft.AccessContactName);
            Assert.Equal(Command1.AccessContactPhone, draft.AccessContactPhone);
            Assert.Equal(SavedAt, draft.LastSavedAt);
        });
    }

    [Fact]
    public async Task GetOrderDraft_ShowsTheLatestSave()
    {
        var latest = Command1 with { BorrowerName = "Daniel J. Price", AccessContactName = null, AccessContactPhone = null };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), Saved1(GrainId), Saved(latest, GrainId, RequestedByUsers.ClientUser1, SavedAt));

        await WhenQuery(() => Order.GetOrderDraftAsync());

        ThenQuery<OrderDraft>(draft =>
        {
            Assert.Equal(latest.BorrowerName, draft.BorrowerName);
            Assert.Null(draft.AccessContactName);
            Assert.Null(draft.AccessContactPhone);
            Assert.Equal(SavedAt, draft.LastSavedAt);
        });
    }

    [Fact]
    public async Task GetOrderDraft_ShowsTheLatestRequestAndProperty()
    {
        var latest = SaveRequestAndPropertyTestData.Command1;
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            Saved1(GrainId),
            SaveRequestAndPropertyTestData.Saved1(GrainId, SavedAt));

        await WhenQuery(() => Order.GetOrderDraftAsync());

        ThenQuery<OrderDraft>(draft => Assert.Equal(new OrderDraft(
            GrainId,
            latest.ServiceRequestTypeId,
            latest.ServiceRequestTypeVersion,
            latest.ServiceRequestTypeName,
            latest.FormTypeId,
            latest.FormTypeVersion,
            latest.FormTypeName,
            latest.PropertyStreet,
            latest.PropertyCity,
            latest.PropertyState,
            latest.PropertyZip,
            latest.PropertyType,
            latest.IntendedUseId,
            latest.IntendedUseVersion,
            latest.IntendedUseName,
            latest.LoanNumber,
            latest.DueDate,
            Command1.BorrowerName,
            Command1.AccessContactName,
            Command1.AccessContactPhone,
            SavedAt), draft));
    }

    [Fact]
    public async Task GetOrderDraft_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await WhenQuery(() => Order.GetOrderDraftAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task GetOrderDraft_RejectsAUserWhoDidNotStartTheOrder()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await WhenQuery(() => Order.GetOrderDraftAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task GetOrderDraft_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.GetOrderDraftAsync());
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (ClaimsPrincipal User, StartOrderDraftCommand Command, Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.Command1, StartOrderDraftTestData.DraftStarted1),
            2 => (RequestedByUsers.ClientUser2, StartOrderDraftTestData.Command2, StartOrderDraftTestData.DraftStarted2),
            3 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.Command3, StartOrderDraftTestData.DraftStarted3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };
}
