using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendDescriptionOfWorkSubmittedNoticeResponse>> SendDescriptionOfWorkSubmittedNoticeAsync(
        SendDescriptionOfWorkSubmittedNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(DescriptionOfWorkSubmittedNoticeSent.InitiatedBy))
                        .Deconstruct(out var initiatedBy),
                    DescriptionOfWorkId.Create(command.DescriptionOfWorkId, tenantId, nameof(command.DescriptionOfWorkId))
                        .Deconstruct(out var descriptionOfWorkId),
                    DescriptionOfWorkVersion.Create(command.DescriptionOfWorkVersion, nameof(command.DescriptionOfWorkVersion))
                        .Deconstruct(out var version),
                    UserId.Create(command.RecipientUserId, tenantId, nameof(command.RecipientUserId)).Deconstruct(out var recipientUserId),
                    EmailAddress.Create(command.RecipientEmail, nameof(command.RecipientEmail)).Deconstruct(out var recipientEmail),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendDescriptionOfWorkSubmittedNotice(initiatedBy!))
                .Ensure(() => State.DescriptionOfWork is { } current
                        && current.Id == (string)descriptionOfWorkId! && (int)version! <= current.Version,
                    () => RequestErrors.NewNotFound("The description of work version was not found."))
                .Ensure(() => State.DescriptionOfWork!.ApproverUserId == (string)recipientUserId!,
                    () => RequestErrors.NewInvalidArg("The notice must go to the client who approves the description of work."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // Each version's notice is sent once; another attempt is an earlier attempt of the automation. The notice of
                    // a version that was revised before it was recorded is still recorded, since the email went out.
                    if (State.DescriptionOfWork!.NoticeSentVersion >= (int)version!)
                    {
                        return;
                    }
                    transaction.Apply(new DescriptionOfWorkSubmittedNoticeSent
                    {
                        OrderId = orderId!,
                        DescriptionOfWorkId = descriptionOfWorkId!,
                        DescriptionOfWorkVersion = (int)version!,
                        RecipientUserId = recipientUserId!,
                        RecipientEmail = recipientEmail!,
                        Subject = subject!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new SendDescriptionOfWorkSubmittedNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsDescriptionOfWorkSubmittedNoticeDueAsync(string descriptionOfWorkId, int descriptionOfWorkVersion)
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId),
                    DescriptionOfWorkId.Create(descriptionOfWorkId, tenantId, nameof(descriptionOfWorkId)).Deconstruct(out var validId),
                    DescriptionOfWorkVersion.Create(descriptionOfWorkVersion, nameof(descriptionOfWorkVersion)).Deconstruct(out var version))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendDescriptionOfWorkSubmittedNotice(userId!))
                .Ensure(() => State.DescriptionOfWork?.Id == (string)validId!,
                    () => RequestErrors.NewNotFound("The description of work was not found."))
                .Map(() => State.DescriptionOfWork is { Approved: false } current
                    && current.Version == (int)version!
                    && !(current.NoticeSentVersion >= current.Version)));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(DescriptionOfWorkSubmittedNoticeSent evt)
        => DescriptionOfWork = DescriptionOfWork! with { NoticeSentVersion = evt.DescriptionOfWorkVersion };
}
