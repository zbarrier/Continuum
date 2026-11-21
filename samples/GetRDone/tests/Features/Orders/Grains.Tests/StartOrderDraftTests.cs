using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.ReadModels;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

namespace GetRDone.Orders.Tests;

public sealed class StartOrderDraftTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    private const string TenantId = SeedData.TenantId;
    private const string AnaRuizUserGuid = "7304a1f2-3c5d-4e6f-8a9b-0c1d2e3f4a5b";

    [Fact]
    public async Task StartDraft_RecordsAllDetails()
    {
        var orderId = NewOrderId();
        var order = GetOrder(orderId);
        var before = DateTimeOffset.UtcNow;

        var result = await StartAsync(order, ValidCommand());

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : null);
        Assert.Equal(1, result.Value.Version);
        var saved = Assert.IsType<OrderDraftStarted>(Assert.Single(await order.GetSavedEventsSince(0)));
        Assert.InRange(saved.OccurredAt, before, DateTimeOffset.UtcNow);
        var expected = new OrderDraftStarted
        {
            OrderId = orderId,
            ClientId = "elliott|c8e41a7d",
            ServiceRequestTypeId = "elliott|5a1f0c3e",
            ServiceRequestTypeVersion = 1,
            ServiceRequestTypeName = "Appraisal Commercial",
            FormTypeId = "elliott|e5a1c9d3",
            FormTypeVersion = 1,
            FormTypeName = "Market Value Interior Inspection",
            PropertyStreet = "1200 Main St",
            PropertyCity = "Roanoke",
            PropertyState = "VA",
            PropertyZip = "24011",
            PropertyType = "Commercial - Office",
            IntendedUseId = "elliott|ad86c5b7",
            IntendedUseVersion = 1,
            IntendedUseName = "Refinance",
            LoanNumber = "CB-2026-0412",
            DueDate = new DateOnly(2026, 12, 4),
            InitiatedBy = SeedData.AnaRuizUserId,
            OccurredAt = saved.OccurredAt
        };
        Assert.Equivalent(expected, saved, strict: true);
    }

    [Fact]
    public async Task StartDraft_RejectsAnExistingOrder()
    {
        var order = GetOrder(NewOrderId());
        Assert.True((await StartAsync(order, ValidCommand())).IsSuccess);

        var result = await StartAsync(order, ValidCommand());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.AlreadyExists, result.Error.Code);
        Assert.Single(await order.GetSavedEventsSince(0));
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
        var command = ValidCommand();
        command = property switch
        {
            nameof(StartOrderDraftCommand.ClientId) => command with { ClientId = " " },
            nameof(StartOrderDraftCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = "elliott|" },
            nameof(StartOrderDraftCommand.ServiceRequestTypeVersion) => command with { ServiceRequestTypeVersion = 0 },
            nameof(StartOrderDraftCommand.ServiceRequestTypeName) => command with { ServiceRequestTypeName = "" },
            nameof(StartOrderDraftCommand.FormTypeId) => command with { FormTypeId = "" },
            nameof(StartOrderDraftCommand.FormTypeVersion) => command with { FormTypeVersion = 0 },
            nameof(StartOrderDraftCommand.FormTypeName) => command with { FormTypeName = " " },
            nameof(StartOrderDraftCommand.PropertyStreet) => command with { PropertyStreet = " " },
            nameof(StartOrderDraftCommand.PropertyCity) => command with { PropertyCity = "" },
            nameof(StartOrderDraftCommand.PropertyState) => command with { PropertyState = "ZZ" },
            nameof(StartOrderDraftCommand.PropertyZip) => command with { PropertyZip = "2401" },
            nameof(StartOrderDraftCommand.PropertyType) => command with { PropertyType = "" },
            nameof(StartOrderDraftCommand.IntendedUseId) => command with { IntendedUseId = "elliott|a|b" },
            nameof(StartOrderDraftCommand.IntendedUseVersion) => command with { IntendedUseVersion = 0 },
            nameof(StartOrderDraftCommand.IntendedUseName) => command with { IntendedUseName = "" },
            nameof(StartOrderDraftCommand.LoanNumber) => command with { LoanNumber = "" },
            nameof(StartOrderDraftCommand.DueDate) => command with { DueDate = default },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        var order = GetOrder(NewOrderId());

        var result = await StartAsync(order, command);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(property, Assert.Single(error.Entries).Target);
        Assert.Empty(await order.GetSavedEventsSince(0));
    }

    [Theory]
    [InlineData(nameof(StartOrderDraftCommand.ClientId))]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseId))]
    public async Task StartDraft_RejectsAnIdOfAnotherTenant(string property)
    {
        var command = ValidCommand();
        command = property switch
        {
            nameof(StartOrderDraftCommand.ClientId) => command with { ClientId = "other|c8e41a7d" },
            nameof(StartOrderDraftCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = "other|5a1f0c3e" },
            nameof(StartOrderDraftCommand.FormTypeId) => command with { FormTypeId = "e5a1c9d3" },
            nameof(StartOrderDraftCommand.IntendedUseId) => command with { IntendedUseId = "other|ad86c5b7" },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };

        var result = await StartAsync(GetOrder(NewOrderId()), command);

        var entry = Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries);
        Assert.Equal(property, entry.Target);
        Assert.Equal(ValidationErrorCodes.InvalidFormat, entry.Code);
    }

    [Fact]
    public async Task StartDraft_ReportsEveryInvalidDetail()
    {
        var command = ValidCommand() with { PropertyStreet = "", LoanNumber = "" };

        var result = await StartAsync(GetOrder(NewOrderId()), command);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(
            [nameof(StartOrderDraftCommand.PropertyStreet), nameof(StartOrderDraftCommand.LoanNumber)],
            error.Entries.Select(entry => entry.Target));
    }

    [Fact]
    public async Task StartDraft_RejectsAnOrderIdThatIsNotAGuid()
    {
        var order = fixture.Cluster.GrainFactory.GetGrain<IOrderGrain>(TenantKey.Create(TenantId, "not-a-guid"));

        var result = await StartAsync(order, ValidCommand());

        Assert.Equal("OrderId", Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries).Target);
    }

    [Fact]
    public async Task StartDraft_RejectsAnUnauthenticatedCaller()
    {
        var result = await GetOrder(NewOrderId()).StartOrderDraftAsync(ValidCommand());

        Assert.Equal(nameof(OrderDraftStarted.InitiatedBy), Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries).Target);
    }

    [Fact]
    public async Task StartDraft_RejectsAUserWhoIsNotAUserOfTheClient()
    {
        var order = GetOrder(NewOrderId());

        var otherUser = await StartAsync(order, ValidCommand(), userGuid: Guid.NewGuid().ToString());
        var otherClient = await StartAsync(order, ValidCommand() with { ClientId = "elliott|0b2e4f61" });

        Assert.Equal(ErrorCodes.PermissionDenied, otherUser.Error.Code);
        Assert.Equal(ErrorCodes.PermissionDenied, otherClient.Error.Code);
        Assert.Empty(await order.GetSavedEventsSince(0));
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
        var command = ValidCommand();
        command = part switch
        {
            "service" => command with { ServiceRequestTypeId = "elliott|0d9c1b2a" },
            "serviceVersion" => command with { ServiceRequestTypeVersion = 2 },
            "serviceName" => command with { ServiceRequestTypeName = "Appraisal Residential" },
            "form" => command with { FormTypeId = "elliott|0d9c1b2a" },
            "formVersion" => command with { FormTypeVersion = 2 },
            "formName" => command with { FormTypeName = "Desktop Appraisal" },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var order = GetOrder(NewOrderId());

        var result = await StartAsync(order, command);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        Assert.Empty(await order.GetSavedEventsSince(0));
    }

    [Theory]
    [InlineData("intendedUse")]
    [InlineData("intendedUseVersion")]
    [InlineData("intendedUseName")]
    public async Task StartDraft_RejectsAnIntendedUseThatIsNotCurrent(string part)
    {
        var command = ValidCommand();
        command = part switch
        {
            "intendedUse" => command with { IntendedUseId = "elliott|0d9c1b2a" },
            "intendedUseVersion" => command with { IntendedUseVersion = 2 },
            "intendedUseName" => command with { IntendedUseName = "Purchase" },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var order = GetOrder(NewOrderId());

        var result = await StartAsync(order, command);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        Assert.Empty(await order.GetSavedEventsSince(0));
    }

    [Fact]
    public void ReplayingOrderDraftStarted_MovesTheOrderToDraft()
    {
        var state = new OrderState();
        var started = new OrderDraftStarted
        {
            OrderId = NewOrderId(),
            ClientId = "elliott|c8e41a7d",
            ServiceRequestTypeId = "elliott|5a1f0c3e",
            ServiceRequestTypeVersion = 1,
            ServiceRequestTypeName = "Appraisal Commercial",
            FormTypeId = "elliott|e5a1c9d3",
            FormTypeVersion = 1,
            FormTypeName = "Market Value Interior Inspection",
            PropertyStreet = "1200 Main St",
            PropertyCity = "Roanoke",
            PropertyState = "VA",
            PropertyZip = "24011",
            PropertyType = "Commercial - Office",
            IntendedUseId = "elliott|ad86c5b7",
            IntendedUseVersion = 1,
            IntendedUseName = "Refinance",
            LoanNumber = "CB-2026-0412",
            DueDate = new DateOnly(2026, 12, 4),
            InitiatedBy = SeedData.AnaRuizUserId,
            OccurredAt = DateTimeOffset.Parse("2026-11-17T09:05:00Z")
        };
        Assert.Equal(OrderStatus.None, state.Status);

        state.When(started);

        Assert.Equal(OrderStatus.Draft, state.Status);
        Assert.Equal(SeedData.AnaRuizUserId, state.StartedBy);
        Assert.False(state.StateMachine.CanFire(OrderCommand.StartDraft));
    }

    private static StartOrderDraftCommand ValidCommand() => new(
        "elliott|c8e41a7d",
        "elliott|5a1f0c3e", 1, "Appraisal Commercial",
        "elliott|e5a1c9d3", 1, "Market Value Interior Inspection",
        "1200 Main St", "Roanoke", "VA", "24011", "Commercial - Office",
        "elliott|ad86c5b7", 1, "Refinance",
        "CB-2026-0412", new DateOnly(2026, 12, 4));

    private static string NewOrderId() => TenantKey.Create(TenantId, Guid.NewGuid().ToString());

    private IOrderGrain GetOrder(string orderId) => fixture.Cluster.GrainFactory.GetGrain<IOrderGrain>(orderId);

    private static async Task<Result<StartOrderDraftResponse>> StartAsync(
        IOrderGrain order, StartOrderDraftCommand command, string userGuid = AnaRuizUserGuid)
    {
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userGuid)], "test")));
        try
        {
            return await order.StartOrderDraftAsync(command);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }
}
