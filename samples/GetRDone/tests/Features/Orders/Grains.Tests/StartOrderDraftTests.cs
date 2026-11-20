using Continuum.CSharpFunctionalExtensions;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.OrderAggregate;

namespace GetRDone.Orders.Tests;

public sealed class StartOrderDraftTests
{
    private static readonly Guid ServiceId = Guid.Parse("d487ac18-f4ae-402c-828e-2b9e8d09516c");
    private static readonly Guid FormId = Guid.Parse("78a59b0d-e8ea-4f85-9860-c206ab297170");
    private static readonly Guid OrderId = Guid.Parse("9b2f3c1e-5d7a-4e8b-a1c4-0d6e8f2a7b31");
    private static readonly Guid UserId = Guid.Parse("d216d7f0-d8db-45f1-b83b-245851e55e50");
    private static readonly DateTimeOffset OccurredAt = DateTimeOffset.Parse("2026-11-17T09:05:00Z");
    private static readonly OrderableService Offering = new(ServiceId, 1, "Appraisal Commercial", FormId, 1, "Market Value Interior Inspection");

    private static StartOrderDraftCommand ValidCommand() => new(
        ServiceId, 1, FormId, 1, "1200 Main St", "Roanoke", "VA", "24011", "Commercial - Office",
        "refinance", "1", "Refinance", "CB-2026-0412", new DateOnly(2026, 12, 4));

    [Fact]
    public void StartDraft_RecordsAllDetailsAndReplaysIntoDraftState()
    {
        var state = new OrderState();
        var result = StartOrderDraft.Decide(state, ValidCommand(), [Offering], OrderId, UserId, OccurredAt);

        Assert.True(result.IsSuccess);
        var expected = new OrderDraftStarted(OrderId, ServiceId, 1, "Appraisal Commercial",
            FormId, 1, "Market Value Interior Inspection", "1200 Main St", "Roanoke", "VA", "24011",
            "Commercial - Office", "refinance", "1", "Refinance", "CB-2026-0412", new DateOnly(2026, 12, 4), 
            UserId, OccurredAt);
        Assert.Equal(expected, result.Value);
        Assert.Null(state.Draft);
        state.When(result.Value);
        Assert.Equal(expected, state.Draft);
    }

    [Theory]
    [InlineData("street")]
    [InlineData("city")]
    [InlineData("state")]
    [InlineData("zip")]
    public void StartDraft_RejectsMissingAddress(string part)
    {
        var command = ValidCommand();
        command = part switch
        {
            "street" => command with { PropertyStreet = " " },
            "city" => command with { PropertyCity = "" },
            "state" => command with { PropertyState = "" },
            "zip" => command with { PropertyZip = "" },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var state = new OrderState();
        var result = StartOrderDraft.Decide(state, command, [Offering], OrderId, UserId, OccurredAt);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
        Assert.Null(state.Draft);
    }

    [Theory]
    [InlineData("propertyType")]
    [InlineData("intendedUseId")]
    [InlineData("intendedUseVersion")]
    [InlineData("intendedUseName")]
    [InlineData("loanNumber")]
    [InlineData("dueDate")]
    public void StartDraft_RejectsMissingRequestDetails(string part)
    {
        var command = ValidCommand();
        command = part switch
        {
            "propertyType" => command with { PropertyType = "" },
            "intendedUseId" => command with { IntendedUseId = "" },
            "intendedUseVersion" => command with { IntendedUseVersion = "" },
            "intendedUseName" => command with { IntendedUseName = "" },
            "loanNumber" => command with { LoanNumber = "" },
            "dueDate" => command with { DueDate = default },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var result = StartOrderDraft.Decide(new OrderState(), command, [Offering], OrderId, UserId, OccurredAt);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
    }

    [Theory]
    [InlineData("service")]
    [InlineData("serviceVersion")]
    [InlineData("form")]
    [InlineData("formVersion")]
    public void StartDraft_RejectsUnknownOfferingOrVersion(string part)
    {
        var command = ValidCommand();
        command = part switch
        {
            "service" => command with { ServiceRequestTypeId = Guid.NewGuid() },
            "serviceVersion" => command with { ServiceRequestTypeVersion = 2 },
            "form" => command with { FormTypeId = Guid.NewGuid() },
            "formVersion" => command with { FormTypeVersion = 2 },
            _ => throw new ArgumentOutOfRangeException(nameof(part))
        };
        var result = StartOrderDraft.Decide(new OrderState(), command, [Offering], OrderId, UserId, OccurredAt);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.InvalidArgument, result.Error.Code);
    }

    [Fact]
    public void StartDraft_RejectsAnExistingOrder()
    {
        var state = new OrderState();
        var first = StartOrderDraft.Decide(state, ValidCommand(), [Offering], OrderId, UserId, OccurredAt);
        state.When(first.Value);

        var result = StartOrderDraft.Decide(state, ValidCommand(), [Offering], OrderId, UserId, OccurredAt);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.AlreadyExists, result.Error.Code);
        Assert.Equal(first.Value, state.Draft);
    }
}
