using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendNewOrderNoticeResponse>> SendNewOrderNoticeAsync(SendNewOrderNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(NewOrderNoticeSent.InitiatedBy)).Deconstruct(out var initiatedBy),
                    UserId.Create(command.RecipientUserId, tenantId, nameof(command.RecipientUserId)).Deconstruct(out var recipientUserId),
                    EmailAddress.Create(command.RecipientEmail, nameof(command.RecipientEmail)).Deconstruct(out var recipientEmail),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendNewOrderNotice(initiatedBy!))
                .Ensure(() => State.OrderNumber is not null,
                    () => RequestErrors.NewFailedPrecondition("The order has no number yet."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The notice is sent once per order; another attempt is an earlier attempt of the automation.
                    if (State.NewOrderNoticeSent)
                    {
                        return;
                    }
                    transaction.Apply(new NewOrderNoticeSent
                    {
                        OrderId = orderId!,
                        OrderNumber = State.OrderNumber!.Value,
                        RecipientUserId = recipientUserId!,
                        RecipientEmail = recipientEmail!,
                        Subject = subject!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new SendNewOrderNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsNewOrderNoticeSentAsync()
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendNewOrderNotice(userId!))
                .Map(() => State.NewOrderNoticeSent));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(NewOrderNoticeSent _) => NewOrderNoticeSent = true;
}
