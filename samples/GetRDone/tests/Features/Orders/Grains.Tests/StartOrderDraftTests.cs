using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.OrderAggregate;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

namespace GetRDone.Orders.Tests;

public sealed class StartOrderDraftTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    private const string TenantId = "demo";
    private static readonly Guid UserId = Guid.Parse("d216d7f0-d8db-45f1-b83b-245851e55e50");
    private static readonly Guid IntendedUseId = Guid.Parse("5c0f5b7e-6a1d-4f3e-9b2a-8d4c7e1f2a60");

    [Fact]
    public async Task StartDraft_RecordsAllDetails()
    {
        var offering = await GetOfferingAsync();
        var orderId = Guid.NewGuid();
        var order = GetOrder(orderId);
        var before = DateTimeOffset.UtcNow;

        var result = await StartAsync(order, ValidCommand(offering));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : null);
        Assert.Equal(1, result.Value.Version);
        var saved = Assert.IsType<OrderDraftStarted>(Assert.Single(await order.GetSavedEventsSince(0)));
        Assert.InRange(saved.OccurredAt, before, DateTimeOffset.UtcNow);
        var expected = new OrderDraftStarted
        {
            OrderId = orderId,
            ServiceRequestTypeId = offering.ServiceRequestTypeId,
            ServiceRequestTypeVersion = offering.ServiceRequestTypeVersion,
            ServiceRequestTypeName = offering.ServiceRequestTypeName,
            FormTypeId = offering.FormTypeId,
            FormTypeVersion = offering.FormTypeVersion,
            FormTypeName = offering.FormTypeName,
            PropertyStreet = "1200 Main St",
            PropertyCity = "Roanoke",
            PropertyState = "VA",
            PropertyZip = "24011",
            PropertyType = "Commercial - Office",
            IntendedUseId = IntendedUseId,
            IntendedUseVersion = 1,
            IntendedUseName = "Refinance",
            LoanNumber = "CB-2026-0412",
            DueDate = new DateOnly(2026, 12, 4),
            InitiatedBy = UserId,
            OccurredAt = saved.OccurredAt
        };
        Assert.Equivalent(expected, saved, strict: true);
    }

    [Fact]
    public async Task StartDraft_RejectsAnExistingOrder()
    {
        var offering = await GetOfferingAsync();
        var order = GetOrder(Guid.NewGuid());
        Assert.True((await StartAsync(order, ValidCommand(offering))).IsSuccess);

        var result = await StartAsync(order, ValidCommand(offering));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.AlreadyExists, result.Error.Code);
        Assert.Single(await order.GetSavedEventsSince(0));
    }

    [Theory]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.ServiceRequestTypeVersion))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeId))]
    [InlineData(nameof(StartOrderDraftCommand.FormTypeVersion))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyStreet))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyCity))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyState))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyZip))]
    [InlineData(nameof(StartOrderDraftCommand.PropertyType))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseId))]
    [InlineData(nameof(StartOrderDraftCommand.IntendedUseVersion))]
    [InlineData(nameof(StartOrderDraftCommand.LoanNumber))]
    [InlineData(nameof(StartOrderDraftCommand.DueDate))]
    public async Task StartDraft_RejectsAnInvalidDetail(string property)
    {
        var command = ValidCommand(await GetOfferingAsync());
        command = property switch
        {
            nameof(StartOrderDraftCommand.ServiceRequestTypeId) => command with { ServiceRequestTypeId = Guid.Empty },
            nameof(StartOrderDraftCommand.ServiceRequestTypeVersion) => command with { ServiceRequestTypeVersion = 0 },
            nameof(StartOrderDraftCommand.FormTypeId) => command with { FormTypeId = Guid.Empty },
            nameof(StartOrderDraftCommand.FormTypeVersion) => command with { FormTypeVersion = 0 },
            nameof(StartOrderDraftCommand.PropertyStreet) => command with { PropertyStreet = " " },
            nameof(StartOrderDraftCommand.PropertyCity) => command with { PropertyCity = "" },
            nameof(StartOrderDraftCommand.PropertyState) => command with { PropertyState = "ZZ" },
            nameof(StartOrderDraftCommand.PropertyZip) => command with { PropertyZip = "2401" },
            nameof(StartOrderDraftCommand.PropertyType) => command with { PropertyType = "" },
            nameof(StartOrderDraftCommand.IntendedUseId) => command with { IntendedUseId = Guid.Empty },
            nameof(StartOrderDraftCommand.IntendedUseVersion) => command with { IntendedUseVersion = 0 },
            nameof(StartOrderDraftCommand.LoanNumber) => command with { LoanNumber = "" },
            nameof(StartOrderDraftCommand.DueDate) => command with { DueDate = default },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        var order = GetOrder(Guid.NewGuid());

        var result = await StartAsync(order, command);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(property, Assert.Single(error.Entries).Target);
        Assert.Empty(await order.GetSavedEventsSince(0));
    }

    [Fact]
    public async Task StartDraft_ReportsEveryInvalidDetail()
    {
        var command = ValidCommand(await GetOfferingAsync()) with { PropertyStreet = "", LoanNumber = "" };

        var result = await StartAsync(GetOrder(Guid.NewGuid()), command);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(
            [nameof(StartOrderDraftCommand.PropertyStreet), nameof(StartOrderDraftCommand.LoanNumber)],
            error.Entries.Select(entry => entry.Target));
    }

    [Theory]
    [InlineData("service")]
    [InlineData("serviceVersion")]
    [InlineData("form")]
    [InlineData("formVersion")]
    public async Task StartDraft_RejectsUnknownOfferingOrVersion(string part)
    {
        var command = ValidCommand(await GetOfferingAsync());
        command = part switch
        {
            "service" => command with { ServiceRequestTypeId = Guid.NewGuid() },
            "serviceVersion" => command with { ServiceRequestTypeVersion = command.ServiceRequestTypeVersion + 1 },
            "form" => command with { FormTypeId = Guid.NewGuid() },
            "formVersion" => command with { FormTypeVersion = command.FormTypeVersion + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var order = GetOrder(Guid.NewGuid());

        var result = await StartAsync(order, command);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        Assert.Empty(await order.GetSavedEventsSince(0));
    }

    [Theory]
    [InlineData("intendedUse")]
    [InlineData("intendedUseVersion")]
    public async Task StartDraft_RejectsUnknownIntendedUseOrVersion(string part)
    {
        var command = ValidCommand(await GetOfferingAsync());
        command = part switch
        {
            "intendedUse" => command with { IntendedUseId = Guid.NewGuid() },
            "intendedUseVersion" => command with { IntendedUseVersion = command.IntendedUseVersion + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var order = GetOrder(Guid.NewGuid());

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
            OrderId = Guid.NewGuid(),
            ServiceRequestTypeId = Guid.NewGuid(),
            ServiceRequestTypeVersion = 1,
            ServiceRequestTypeName = "Appraisal Commercial",
            FormTypeId = Guid.NewGuid(),
            FormTypeVersion = 1,
            FormTypeName = "Market Value Interior Inspection",
            PropertyStreet = "1200 Main St",
            PropertyCity = "Roanoke",
            PropertyState = "VA",
            PropertyZip = "24011",
            PropertyType = "Commercial - Office",
            IntendedUseId = IntendedUseId,
            IntendedUseVersion = 1,
            IntendedUseName = "Refinance",
            LoanNumber = "CB-2026-0412",
            DueDate = new DateOnly(2026, 12, 4),
            InitiatedBy = UserId,
            OccurredAt = DateTimeOffset.Parse("2026-11-17T09:05:00Z")
        };
        Assert.Equal(OrderStatus.None, state.Status);

        state.When(started);

        Assert.Equal(OrderStatus.Draft, state.Status);
        Assert.Same(started, state.Draft);
        Assert.False(state.StateMachine.CanFire(OrderCommand.StartDraft));
    }

    private static StartOrderDraftCommand ValidCommand(OrderableService offering) => new(
        offering.ServiceRequestTypeId, offering.ServiceRequestTypeVersion, offering.FormTypeId, offering.FormTypeVersion,
        "1200 Main St", "Roanoke", "VA", "24011", "Commercial - Office",
        IntendedUseId, 1, "CB-2026-0412", new DateOnly(2026, 12, 4));

    private async Task<OrderableService> GetOfferingAsync() => Assert.Single(await fixture.Cluster.GrainFactory
        .GetGrain<IOrderableServicesGrain>(TenantKey.Create(TenantId, "offerings")).GetOfferingsAsync());

    private IOrderGrain GetOrder(Guid orderId) =>
        fixture.Cluster.GrainFactory.GetGrain<IOrderGrain>(TenantKey.Create(TenantId, orderId.ToString()));

    private static async Task<Result<StartOrderDraftResponse>> StartAsync(IOrderGrain order, StartOrderDraftCommand command)
    {
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, UserId.ToString())], "test")));
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
