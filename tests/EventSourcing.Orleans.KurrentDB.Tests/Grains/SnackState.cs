using Continuum.Domain.Orleans;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Grains;

[GenerateSerializer]
public sealed class SnackState : EventDrivenState<SnackState, SnackEvent>
{
    public SnackState()
    {
        On<SnackInitializedEvent>(Handle);
        On<SnackRemovedEvent>(Handle);
        On<SnackNameChangedEvent>(Handle);
    }

    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public DateTimeOffset? CreatedAt { get; set; }

    [Id(2)]
    public string? CreatedBy { get; set; }

    public bool IsCreated => CreatedAt != null;

    [Id(3)]
    public DateTimeOffset? LastModifiedAt { get; set; }

    [Id(4)]
    public string? LastModifiedBy { get; set; }

    [Id(5)]
    public DateTimeOffset? DeletedAt { get; set; }

    [Id(6)]
    public string? DeletedBy { get; set; }

    [Id(7)]
    public bool IsDeleted { get; set; }

    [Id(8)]
    public string Name { get; set; } = string.Empty;

    [Id(9)]
    public string? PictureUrl { get; set; }

    public override string ToString()
    {
        return $"Snack with Id:{Id} Name:'{Name}'";
    }

    #region Apply

    public void Handle(SnackInitializedEvent evt)
    {
        Id = evt.Id;
        Name = evt.Name;
        CreatedAt = evt.OperatedAt;
        CreatedBy = evt.OperatedBy;
    }

    public void Handle(SnackRemovedEvent evt)
    {
        DeletedAt = evt.OperatedAt;
        DeletedBy = evt.OperatedBy;
        IsDeleted = true;
    }

    public void Handle(SnackNameChangedEvent evt)
    {
        Name = evt.Name;
        LastModifiedAt = evt.OperatedAt;
        LastModifiedBy = evt.OperatedBy;
    }

    #endregion

}
