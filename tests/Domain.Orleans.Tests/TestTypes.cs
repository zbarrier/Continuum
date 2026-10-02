using Continuum.TypeMapping;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Continuum.Domain.Orleans.Tests;

[GenerateSerializer]
public abstract record CounterEvent;

[GenerateSerializer]
public sealed record CounterIncremented([property: Id(0)] int By) : CounterEvent;

[GenerateSerializer]
public sealed record CounterReset : CounterEvent;

[GenerateSerializer]
public sealed class CounterState : EventDrivenState<CounterState, CounterEvent>
{
    public CounterState()
    {
        On<CounterIncremented>(e => Count += e.By);
        On<CounterReset>(_ => Count = 0);
    }

    [Id(0)] public int Count { get; set; }
}

public sealed class TestTypeMapper : TypeMapper
{
    public TestTypeMapper() => AddType(typeof(CounterIncremented), "CounterIncremented");
}

public static class TestServices
{
    private static readonly IServiceProvider Provider = new ServiceCollection().AddSerializer().BuildServiceProvider();
    public static DeepCopier<T> Copier<T>() => Provider.GetRequiredService<DeepCopier<T>>();
}
