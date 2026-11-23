using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using static GetRDone.Orders.Grains.Tests.Data.SaveRequestAndPropertyTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for saving the Request &amp; Property tab of a draft that was already started. Each test uses a
///     new order in the test tenant; the tabs come from <see cref="SaveRequestAndPropertyTestData"/>.
/// </summary>
public sealed class SaveRequestAndPropertyTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public SaveRequestAndPropertyTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Save_RecordsTheRequestAndProperty(int variation)
    {
        var (user, draftStarted, command, saved) = Variation(variation);
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(user);
        await Given(draftStarted(GrainId, null));

        await When(() => Order.SaveRequestAndPropertyAsync(command));

        Then<SaveRequestAndPropertyResponse>(response =>
        {
            Assert.Equal(2, response.Version);
            var recorded = Assert.IsType<RequestAndPropertySaved>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(saved(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Save_ReplacesAnEarlierSave()
    {
        var command = Command1 with { PropertyZip = "28804" };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            Saved1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(command));

        Then<SaveRequestAndPropertyResponse>(response =>
        {
            Assert.Equal(4, response.Version);
            var recorded = Assert.IsType<RequestAndPropertySaved>(SingleEvent(response.Changes));
            Assert.Equivalent(Saved(command, GrainId, RequestedByUsers.ClientUser1, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Theory]
    [InlineData(nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeId))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeVersion))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeName))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.FormTypeId))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.FormTypeVersion))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.FormTypeName))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.PropertyStreet))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.PropertyCity))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.PropertyState))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.PropertyZip))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.PropertyType))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.IntendedUseId))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.IntendedUseVersion))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.IntendedUseName))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.LoanNumber))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.DueDate))]
    public async Task Save_RejectsAnInvalidDetail(string property)
    {
        var command = Command1;
        command = property switch
        {
            nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = "summit|" },
            nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeVersion) => command with { ServiceRequestTypeVersion = 0 },
            nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeName) => command with { ServiceRequestTypeName = "" },
            nameof(SaveRequestAndPropertyCommand.FormTypeId) => command with { FormTypeId = "" },
            nameof(SaveRequestAndPropertyCommand.FormTypeVersion) => command with { FormTypeVersion = 0 },
            nameof(SaveRequestAndPropertyCommand.FormTypeName) => command with { FormTypeName = " " },
            nameof(SaveRequestAndPropertyCommand.PropertyStreet) => command with { PropertyStreet = " " },
            nameof(SaveRequestAndPropertyCommand.PropertyCity) => command with { PropertyCity = "" },
            nameof(SaveRequestAndPropertyCommand.PropertyState) => command with { PropertyState = "ZZ" },
            nameof(SaveRequestAndPropertyCommand.PropertyZip) => command with { PropertyZip = "2880" },
            nameof(SaveRequestAndPropertyCommand.PropertyType) => command with { PropertyType = "" },
            nameof(SaveRequestAndPropertyCommand.IntendedUseId) => command with { IntendedUseId = "summit|a|b" },
            nameof(SaveRequestAndPropertyCommand.IntendedUseVersion) => command with { IntendedUseVersion = 0 },
            nameof(SaveRequestAndPropertyCommand.IntendedUseName) => command with { IntendedUseName = "" },
            nameof(SaveRequestAndPropertyCommand.LoanNumber) => command with { LoanNumber = "" },
            nameof(SaveRequestAndPropertyCommand.DueDate) => command with { DueDate = default },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Theory]
    [InlineData(nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeId))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.FormTypeId))]
    [InlineData(nameof(SaveRequestAndPropertyCommand.IntendedUseId))]
    public async Task Save_RejectsAnIdOfAnotherTenant(string property)
    {
        var command = Command1;
        command = property switch
        {
            nameof(SaveRequestAndPropertyCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = "other|6c4f2a90" },
            nameof(SaveRequestAndPropertyCommand.FormTypeId) => command with { FormTypeId = "d05e9b37" },
            nameof(SaveRequestAndPropertyCommand.IntendedUseId) => command with { IntendedUseId = "other|7a1d0c42" },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(command));

        ThenError<ValidationError>(error =>
        {
            var entry = Assert.Single(error.Entries);
            Assert.Equal(property, entry.Target);
            Assert.Equal(ValidationErrorCodes.InvalidFormat, entry.Code);
        });
    }

    [Fact]
    public async Task Save_ReportsEveryInvalidDetail()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(Command1 with { PropertyStreet = "", LoanNumber = "" }));

        ThenError<ValidationError>(error => Assert.Equal(
            [nameof(SaveRequestAndPropertyCommand.PropertyStreet), nameof(SaveRequestAndPropertyCommand.LoanNumber)],
            error.Entries.Select(entry => entry.Target)));
    }

    [Theory]
    [InlineData("service")]
    [InlineData("formVersion")]
    [InlineData("formOfAnotherService")]
    public async Task Save_RejectsAServiceAndFormThatAreNotCurrent(string part)
    {
        var command = Command1;
        command = part switch
        {
            "service" => command with { ServiceRequestTypeId = "summit|0d9c1b2a" },
            "formVersion" => command with { FormTypeVersion = command.FormTypeVersion + 1 },
            "formOfAnotherService" => command with
            {
                FormTypeId = TestData.NarrativeAppraisal.FormTypeId,
                FormTypeVersion = TestData.NarrativeAppraisal.FormTypeVersion,
                FormTypeName = TestData.NarrativeAppraisal.FormTypeName
            },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(command));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAnIntendedUseThatIsNotCurrent()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(Command1 with { IntendedUseVersion = Command1.IntendedUseVersion - 1 }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.SaveRequestAndPropertyAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAnOrderThatWasSubmitted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
            SubmitOrderTestData.Submitted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAUserWhoDidNotStartTheOrder()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.SaveRequestAndPropertyAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Save_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SaveRequestAndPropertyAsync(Command1));
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (
        ClaimsPrincipal User,
        Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted,
        SaveRequestAndPropertyCommand Command,
        Func<string, DateTimeOffset?, RequestAndPropertySaved> Saved)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted1, Command1, Saved1),
            2 => (RequestedByUsers.ClientUser2, StartOrderDraftTestData.DraftStarted2, Command2, Saved2),
            3 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted3, Command3, Saved3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
