using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

[GenerateSerializer]
public sealed partial class SnackState : EventDrivenState<SnackState, SnackEvent>
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

    public bool IsCreated => CreatedAt != null;

    [Id(2)]
    public DateTimeOffset? LastModifiedAt { get; set; }

    [Id(3)]
    public DateTimeOffset? DeletedAt { get; set; }

    [Id(4)]
    public bool IsDeleted { get; set; }

    [Id(5)]
    public string Name { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Snack with Id:{Id} Name:'{Name}'";
    }
}
