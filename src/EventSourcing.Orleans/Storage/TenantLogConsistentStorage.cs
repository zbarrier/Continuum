using System.Collections.Concurrent;

using Continuum.Orleans.Tenancy;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Routes each grain to the log-consistent storage for its tenant.
/// </summary>
/// <remarks>
///     The tenant is read from the grain key, <c>{TenantId}|{Key}</c>. Grains without a tenant, and tenants without
///     specific configuration, use the provider's default settings. Each tenant's storage is created on first use and
///     reused afterwards. Storages never own their clients, so tenants that share a connection share one client.
/// </remarks>
public sealed class TenantLogConsistentStorage : ILogConsistentStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly ILogConsistentStorage _defaultStorage;
    private readonly Func<string, CancellationToken, Task<ILogConsistentStorage>> _createTenantStorage;
    private readonly ConcurrentDictionary<string, Lazy<Task<ILogConsistentStorage>>> _tenantStorages = new(StringComparer.Ordinal);

    /// <summary>
    ///     Creates a new <see cref="TenantLogConsistentStorage"/>.
    /// </summary>
    /// <param name="defaultStorage">The storage used by grains without a tenant.</param>
    /// <param name="createTenantStorage">Creates and initializes the storage for a tenant.</param>
    public TenantLogConsistentStorage(ILogConsistentStorage defaultStorage, Func<string, CancellationToken, Task<ILogConsistentStorage>> createTenantStorage)
    {
        ArgumentNullException.ThrowIfNull(defaultStorage);
        ArgumentNullException.ThrowIfNull(createTenantStorage);
        _defaultStorage = defaultStorage;
        _createTenantStorage = createTenantStorage;
    }

    /// <inheritdoc />
    public void Participate(ISiloLifecycle lifecycle)
    {
        if (_defaultStorage is ILifecycleParticipant<ISiloLifecycle> participant)
        {
            participant.Participate(lifecycle);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int maxCount)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        return await storage.ReadAsync<TLogEntry>(grainTypeName, grainId, fromVersion, maxCount).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        return await storage.GetLastVersionAsync(grainTypeName, grainId).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        return await storage.AppendAsync(grainTypeName, grainId, entries, expectedVersion).ConfigureAwait(false);
    }

    private Task<ILogConsistentStorage> GetStorageAsync(GrainId grainId)
    {
        TenantKey.Split(grainId.Key.ToString()!, out var tenantId, out _);
        if (tenantId is null)
        {
            return Task.FromResult(_defaultStorage);
        }
        var lazy = _tenantStorages.GetOrAdd(tenantId, id => new Lazy<Task<ILogConsistentStorage>>(() => CreateTenantStorageAsync(id)));
        return lazy.Value;
    }

    private async Task<ILogConsistentStorage> CreateTenantStorageAsync(string tenantId)
    {
        try
        {
            return await _createTenantStorage(tenantId, CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Let the next call retry rather than caching the failure for the life of the silo.
            _tenantStorages.TryRemove(tenantId, out _);
            throw;
        }
    }
}
