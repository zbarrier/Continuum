using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SubmitDescriptionOfWorkResponse>> SubmitDescriptionOfWorkAsync(SubmitDescriptionOfWorkCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(DescriptionOfWorkSubmitted.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DescriptionOfWorkId.Create(command.DescriptionOfWorkId, tenantId, nameof(command.DescriptionOfWorkId))
                        .Deconstruct(out var descriptionOfWorkId),
                    DescriptionOfWorkVersion.Create(command.DescriptionOfWorkVersion, nameof(command.DescriptionOfWorkVersion))
                        .Deconstruct(out var version),
                    DescriptionOfWorkHtml.Create(command.DescriptionOfWorkHtml, nameof(command.DescriptionOfWorkHtml)).Deconstruct(out var html))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSubmitDescriptionOfWork(this.GetCurrentUser(), initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.SubmitDescriptionOfWork),
                    () => State.Status == OrderStatus.DescriptionOfWorkApproved
                        ? RequestErrors.NewFailedPrecondition("The description of work was already approved.")
                        : RequestErrors.NewFailedPrecondition("The order must be acknowledged before its description of work can be submitted."))
                // Someone else submitted a description of work, or revised it, since the caller loaded it.
                .Ensure(() => State.DescriptionOfWork is { } current
                        ? current.Id == (string)descriptionOfWorkId! && current.Version + 1 == (int)version!
                        : (int)version! == DescriptionOfWorkVersion.MinValue,
                    () => RequestErrors.NewAborted("The description of work was changed by someone else. Reload the order and try again."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new DescriptionOfWorkSubmitted
                {
                    OrderId = orderId!,
                    DescriptionOfWorkId = descriptionOfWorkId!,
                    DescriptionOfWorkVersion = (int)version!,
                    DescriptionOfWorkHtml = html!,
                    ApproverUserId = State.StartedBy!,
                    OrderNumber = State.OrderNumber,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new SubmitDescriptionOfWorkResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(DescriptionOfWorkSubmitted evt)
    {
        Status = OrderStatus.DescriptionOfWorkAwaitingApproval;
        DescriptionOfWork = DescriptionOfWork is { } current
            ? current with { Version = evt.DescriptionOfWorkVersion }
            : new OrderDescriptionOfWork(
                evt.DescriptionOfWorkId, evt.DescriptionOfWorkVersion, evt.ApproverUserId,
                Approved: false, NoticeSentVersion: null, ApprovedNoticeSent: false);
    }
}
