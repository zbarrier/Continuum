using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Extensions;

using Microsoft.Extensions.Logging;

using Stateless.Graph;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<StartOrderDraftResponse>> StartOrderDraftAsync(StartOrderDraftCommand command)
    {
        try
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out var key);
            if (tenantId is null || !Guid.TryParse(key, out var orderId) || orderId == Guid.Empty)
                return Result.Failure<StartOrderDraftResponse>(RequestErrors.NewInvalidArg("A tenant-qualified order GUID is required."));

            var initiatedBy = Guid.Parse(OrleansIdentityContext.RequireAuthenticatedUserId());
            var offerings = await GrainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create(tenantId, "offerings"))
                .GetOfferingsAsync();
            var decision = StartOrderDraft.Decide(State, command, offerings, orderId, initiatedBy, DateTimeOffset.UtcNow);
            if (decision.IsFailure)
                return Result.Failure<StartOrderDraftResponse>(decision.Error);

            await using var transaction = BeginStreamTransaction();
            transaction.Apply(decision.Value);
            var saved = await SaveChanges(transaction);
            return saved.IsFailure
                ? Result.Failure<StartOrderDraftResponse>(saved.Error)
                : Result.Success(new StartOrderDraftResponse(saved.Value.Changes, saved.Value.Version));
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to start order draft for grain {GrainId}", this.GetGrainId());
            return Result.Failure<StartOrderDraftResponse>(RequestErrors.NewUnknown("The order draft could not be started."));
        }
    }
}
