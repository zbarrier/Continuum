namespace GetRDone.Silo.Tests;

/// <summary>A test grain that is called by clients and by <see cref="ITenantCallerGrain"/>.</summary>
public interface ITenantTargetGrain : IGrainWithStringKey
{
    Task<string> GetKeyAsync();
}

public sealed class TenantTargetGrain : Grain, ITenantTargetGrain
{
    public Task<string> GetKeyAsync() => Task.FromResult(this.GetPrimaryKeyString());
}

/// <summary>A test grain that calls a <see cref="ITenantTargetGrain"/>, to test calls from one grain to another.</summary>
public interface ITenantCallerGrain : IGrainWithStringKey
{
    Task<string> CallAsync(string targetKey);
}

public sealed class TenantCallerGrain : Grain, ITenantCallerGrain
{
    public Task<string> CallAsync(string targetKey) => GrainFactory.GetGrain<ITenantTargetGrain>(targetKey).GetKeyAsync();
}
