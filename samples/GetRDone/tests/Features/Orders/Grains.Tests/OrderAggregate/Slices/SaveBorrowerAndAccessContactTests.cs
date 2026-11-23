using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using static GetRDone.Orders.Grains.Tests.Data.SaveBorrowerAndAccessContactTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for saving the Borrower &amp; Access contact tab of a draft. Each test uses a new order in the
///     test tenant; the tabs come from <see cref="SaveBorrowerAndAccessContactTestData"/>.
/// </summary>
public sealed class SaveBorrowerAndAccessContactTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public SaveBorrowerAndAccessContactTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Save_RecordsTheBorrowerAndAccessContact(int variation)
    {
        var (user, draftStarted, command, saved) = Variation(variation);
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(user);
        await Given(draftStarted(GrainId, null));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(command));

        Then<SaveBorrowerAndAccessContactResponse>(response =>
        {
            Assert.Equal(2, response.Version);
            var recorded = Assert.IsType<BorrowerAndAccessContactSaved>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(saved(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Save_ReplacesAnEarlierSave()
    {
        var command = Command1 with { BorrowerName = "Daniel J. Price", AccessContactName = null, AccessContactPhone = null };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), Saved1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(command));

        Then<SaveBorrowerAndAccessContactResponse>(response =>
        {
            Assert.Equal(3, response.Version);
            var recorded = Assert.IsType<BorrowerAndAccessContactSaved>(SingleEvent(response.Changes));
            Assert.Equivalent(Saved(command, GrainId, RequestedByUsers.ClientUser1, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Theory]
    [InlineData("(828) 555-0142")]
    [InlineData("828.555.0142")]
    [InlineData("828-555-0142")]
    public async Task Save_RecordsTheAccessContactPhoneAsDigits(string phone)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(Command1 with { AccessContactPhone = phone }));

        Then<SaveBorrowerAndAccessContactResponse>(response => Assert.Equal(
            "8285550142", Assert.IsType<BorrowerAndAccessContactSaved>(SingleEvent(response.Changes)).AccessContactPhone));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Save_RecordsAnEmptyAccessContactAsNone(string? empty)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(
            Command1 with { AccessContactName = empty, AccessContactPhone = empty }));

        Then<SaveBorrowerAndAccessContactResponse>(response =>
        {
            var recorded = Assert.IsType<BorrowerAndAccessContactSaved>(SingleEvent(response.Changes));
            Assert.Null(recorded.AccessContactName);
            Assert.Null(recorded.AccessContactPhone);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Save_RejectsAMissingBorrower(string? borrowerName)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(Command1 with { BorrowerName = borrowerName! }));

        ThenError<ValidationError>(error =>
            Assert.Equal(nameof(SaveBorrowerAndAccessContactCommand.BorrowerName), Assert.Single(error.Entries).Target));
    }

    [Theory]
    [InlineData("Marcus Doyle", null, nameof(SaveBorrowerAndAccessContactCommand.AccessContactPhone))]
    [InlineData("Marcus Doyle", " ", nameof(SaveBorrowerAndAccessContactCommand.AccessContactPhone))]
    [InlineData(null, "8285550142", nameof(SaveBorrowerAndAccessContactCommand.AccessContactName))]
    [InlineData("", "8285550142", nameof(SaveBorrowerAndAccessContactCommand.AccessContactName))]
    public async Task Save_RequiresTheAccessContactNameAndPhoneTogether(string? name, string? phone, string missing)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(
            Command1 with { AccessContactName = name, AccessContactPhone = phone }));

        ThenError<ValidationError>(error => Assert.Equal(missing, Assert.Single(error.Entries).Target));
    }

    [Theory]
    [InlineData(nameof(SaveBorrowerAndAccessContactCommand.BorrowerName))]
    [InlineData(nameof(SaveBorrowerAndAccessContactCommand.AccessContactName))]
    [InlineData(nameof(SaveBorrowerAndAccessContactCommand.AccessContactPhone))]
    public async Task Save_RejectsAnInvalidDetail(string property)
    {
        var command = Command1;
        command = property switch
        {
            nameof(SaveBorrowerAndAccessContactCommand.BorrowerName) => command with { BorrowerName = new string('a', BorrowerName.MaxLength + 1) },
            nameof(SaveBorrowerAndAccessContactCommand.AccessContactName) => command with { AccessContactName = new string('a', AccessContactName.MaxLength + 1) },
            nameof(SaveBorrowerAndAccessContactCommand.AccessContactPhone) => command with { AccessContactPhone = "555-0142" },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Save_ReportsEveryInvalidDetail()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(Command1 with { BorrowerName = "", AccessContactPhone = "123" }));

        ThenError<ValidationError>(error => Assert.Equal(
            [nameof(SaveBorrowerAndAccessContactCommand.BorrowerName), nameof(SaveBorrowerAndAccessContactCommand.AccessContactPhone)],
            error.Entries.Select(entry => entry.Target)));
    }

    [Fact]
    public async Task Save_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.SaveBorrowerAndAccessContactAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAnOrderThatWasSubmitted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), Saved1(GrainId), SubmitOrderTestData.Submitted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(Command3));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAUserWhoDidNotStartTheOrder()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveBorrowerAndAccessContactAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SaveBorrowerAndAccessContactAsync(Command1));
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (
        ClaimsPrincipal User,
        Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted,
        SaveBorrowerAndAccessContactCommand Command,
        Func<string, DateTimeOffset?, BorrowerAndAccessContactSaved> Saved)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted1, Command1, Saved1),
            2 => (RequestedByUsers.ClientUser2, StartOrderDraftTestData.DraftStarted2, Command2, Saved2),
            3 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted3, Command3, Saved3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
