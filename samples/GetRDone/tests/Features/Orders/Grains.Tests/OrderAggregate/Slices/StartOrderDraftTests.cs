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

using static GetRDone.Orders.Grains.Tests.Data.StartOrderDraftTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for starting an order draft. Each test uses a new order in the test tenant, whose read
///     models are seeded with <see cref="TestData"/>; the drafts come from <see cref="StartOrderDraftTestData"/>.
///     The first test checks what applying the slice's event does to the order state and the second checks every value of
///     the event the slice records; the behavior tests follow.
/// </summary>
public sealed class StartOrderDraftTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public StartOrderDraftTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingOrderDraftStarted_MovesTheOrderToDraftForItsUserAndClient()
    {
        var state = new OrderState();
        var started = DraftStarted1(GrainId);

        state.When(started);

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.Draft,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = false,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)null,
            VendorAssignment = (OrderVendorAssignment?)null,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)null
        }, state, strict: true);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task StartDraft_RecordsTheDraft(int variation)
    {
        var (user, command, draftStarted) = Variation(variation);
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(user);
        await Given();

        await When(() => Order.StartOrderDraftAsync(command));

        Then<StartOrderDraftResponse>(response =>
        {
            Assert.Equal(1, response.Version);
            var started = Assert.IsType<OrderDraftStarted>(SingleEvent(response.Changes));
            Assert.InRange(started.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(draftStarted(GrainId, started.OccurredAt), started, strict: true);
        });
    }

    [Fact]
    public async Task StartDraft_RejectsAnOrderThatWasAlreadyStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(DraftStarted1(GrainId));

        await When(() => Order.StartOrderDraftAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.AlreadyExists, error.Code));
    }

    [Theory]
    [InlineData(nameof(StartOrderDraftCommand.ClientId))]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeVersion))]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeName))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeVersion))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeName))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyStreet))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyCity))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyState))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyZip))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyType))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseId))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseVersion))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseName))]
    [InlineData(nameof(StartOrderDraftCommand.LoanNumber))]
    [InlineData(nameof(StartOrderDraftCommand.DueDate))]
    public async Task StartDraft_RejectsAnInvalidDetail(string property)
    {
        var command = Command1;
        command = property switch
        {
            nameof(StartOrderDraftCommand.ClientId) => command with { ClientId = " " },
            nameof(StartOrderDraftCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = "summit|" },
            nameof(StartOrderDraftCommand.ServiceRequestTypeVersion) => command with { ServiceRequestTypeVersion = 0 },
            nameof(StartOrderDraftCommand.ServiceRequestTypeName) => command with { ServiceRequestTypeName = "" },
            nameof(StartOrderDraftCommand.FormTypeId) => command with { FormTypeId = "" },
            nameof(StartOrderDraftCommand.FormTypeVersion) => command with { FormTypeVersion = 0 },
            nameof(StartOrderDraftCommand.FormTypeName) => command with { FormTypeName = " " },
            nameof(StartOrderDraftCommand.PropertyStreet) => command with { PropertyStreet = " " },
            nameof(StartOrderDraftCommand.PropertyCity) => command with { PropertyCity = "" },
            nameof(StartOrderDraftCommand.PropertyState) => command with { PropertyState = "ZZ" },
            nameof(StartOrderDraftCommand.PropertyZip) => command with { PropertyZip = "2880" },
            nameof(StartOrderDraftCommand.PropertyType) => command with { PropertyType = "" },
            nameof(StartOrderDraftCommand.IntendedUseId) => command with { IntendedUseId = "summit|a|b" },
            nameof(StartOrderDraftCommand.IntendedUseVersion) => command with { IntendedUseVersion = 0 },
            nameof(StartOrderDraftCommand.IntendedUseName) => command with { IntendedUseName = "" },
            nameof(StartOrderDraftCommand.LoanNumber) => command with { LoanNumber = "" },
            nameof(StartOrderDraftCommand.DueDate) => command with { DueDate = default },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Theory]
    [InlineData(nameof(StartOrderDraftCommand.ClientId))]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseId))]
    public async Task StartDraft_RejectsAnIdOfAnotherTenant(string property)
    {
        var command = Command1;
        command = property switch
        {
            nameof(StartOrderDraftCommand.ClientId) => command with { ClientId = "other|9b3e7d21" },
            nameof(StartOrderDraftCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = "other|6c4f2a90" },
            nameof(StartOrderDraftCommand.FormTypeId) => command with { FormTypeId = "d05e9b37" },
            nameof(StartOrderDraftCommand.IntendedUseId) => command with { IntendedUseId = "other|7a1d0c42" },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(command));

        ThenError<ValidationError>(error =>
        {
            var entry = Assert.Single(error.Entries);
            Assert.Equal(property, entry.Target);
            Assert.Equal(ValidationErrorCodes.InvalidFormat, entry.Code);
        });
    }

    [Fact]
    public async Task StartDraft_ReportsEveryInvalidDetail()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(Command1 with { PropertyStreet = "", LoanNumber = "" }));

        ThenError<ValidationError>(error => Assert.Equal(
            [nameof(StartOrderDraftCommand.PropertyStreet), nameof(StartOrderDraftCommand.LoanNumber)],
            error.Entries.Select(entry => entry.Target)));
    }

    [Fact]
    public async Task StartDraft_RejectsAnOrderIdThatIsNotAGuid()
    {
        var order = Cluster.GrainFactory.GetGrain<IOrderGrain>(TenantKey.Create(TestData.TenantId, "not-a-guid"));
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => order.StartOrderDraftAsync(Command1));

        ThenError<ValidationError>(error => Assert.Equal("OrderId", Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task StartDraft_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain (and before the harness could read the
        // order's version), so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.StartOrderDraftAsync(Command1));
    }

    [Fact]
    public async Task StartDraft_RejectsAUserWhoIsNotAClientUser()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task StartDraft_RejectsAClientUserOfAnotherClient()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given();

        await When(() => Order.StartOrderDraftAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Theory]
    [InlineData("service")]
    [InlineData("serviceVersion")]
    [InlineData("serviceName")]
    [InlineData("form")]
    [InlineData("formVersion")]
    [InlineData("formName")]
    public async Task StartDraft_RejectsAServiceAndFormThatAreNotCurrent(string part)
    {
        var desktop = TestData.DesktopAppraisal;
        var command = Command1;
        command = part switch
        {
            "service" => command with { ServiceRequestTypeId = "summit|0d9c1b2a" },
            "serviceVersion" => command with { ServiceRequestTypeVersion = command.ServiceRequestTypeVersion - 1 },
            "serviceName" => command with { ServiceRequestTypeName = TestData.CommercialAppraisal.ServiceRequestTypeName },
            "form" => command with { FormTypeId = "summit|0d9c1b2a" },
            "formVersion" => command with
            {
                FormTypeId = desktop.FormTypeId,
                FormTypeVersion = desktop.FormTypeVersion - 1,
                FormTypeName = desktop.FormTypeName
            },
            "formName" => command with { FormTypeName = desktop.FormTypeName },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(command));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Fact]
    public async Task StartDraft_RejectsAFormOfAnotherService()
    {
        var commercialForm = TestData.NarrativeAppraisal;
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(Command1 with
        {
            FormTypeId = commercialForm.FormTypeId,
            FormTypeVersion = commercialForm.FormTypeVersion,
            FormTypeName = commercialForm.FormTypeName
        }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Theory]
    [InlineData("intendedUse")]
    [InlineData("intendedUseVersion")]
    [InlineData("intendedUseName")]
    public async Task StartDraft_RejectsAnIntendedUseThatIsNotCurrent(string part)
    {
        var command = Command1;
        command = part switch
        {
            "intendedUse" => command with { IntendedUseId = "summit|0d9c1b2a" },
            "intendedUseVersion" => command with { IntendedUseVersion = command.IntendedUseVersion - 1 },
            "intendedUseName" => command with { IntendedUseName = TestData.Purchase.Name },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.StartOrderDraftAsync(command));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (ClaimsPrincipal User, StartOrderDraftCommand Command, Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, Command1, DraftStarted1),
            2 => (RequestedByUsers.ClientUser2, Command2, DraftStarted2),
            3 => (RequestedByUsers.ClientUser1, Command3, DraftStarted3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
