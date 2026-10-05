using Continuum.TypeMapping;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[SnapshotType("persistence-test-counter-state")]
[GenerateSerializer]
public sealed class CounterState
{
    [Id(0)]
    public int Count { get; set; }

    [Id(1)]
    public string? Label { get; set; }
}
