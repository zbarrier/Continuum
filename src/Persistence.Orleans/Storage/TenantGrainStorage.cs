using System.Collections.Concurrent;

using Continuum.Orleans.Tenancy;

using Orleans.Storage;

namespace Continuum.Persistence.Orleans;

/// <summary>
///     Routes each grain to the grain storage for its tenant.
/// </summary>
/// <remarks>
///     The tenant is read from the grain key, <c>{TenantId}|{Key}</c>. Grains without a tenant, and tenants without
///     specific configuration, use the provider's default settings. Each tenant's storage is created on first use and
///     reused afterwards. Storages never own their clients, so tenants that share a connection share one client.
/// </remarks>
public sealed class TenantGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly IGrainStorage _defaultStorage;
    private readonly Func<string, CancellationToken, Task<IGrainStorage>> _createTenantStorage;
    private readonly TenantStorageFailureLogger _failureLogger;
    private readonly ConcurrentDictionary<string, Lazy<Task<IGrainStorage>>> _tenantStorages = new(StringComparer.Ordinal);

    /// <summary>
    ///     Creates a new <see cref="TenantGrainStorage"/>.
    /// </summary>
    /// <param name="defaultStorage">The storage used by grains without a tenant.</param>
    /// <param name="createTenantStorage">Creates and initializes the storage for a tenant.</param>
    /// <param name="failureLogger">Logs failures to create a tenant's storage.</param>
    public TenantGrainStorage(IGrainStorage defaultStorage, Func<string, CancellationToken, Task<IGrainStorage>> createTenantStorage,
        TenantStorageFailureLogger failureLogger)
    {
        ArgumentNullException.ThrowIfNull(defaultStorage);
        ArgumentNullException.ThrowIfNull(createTenantStorage);
        ArgumentNullException.ThrowIfNull(failureLogger);
        _failureLogger = failureLogger;
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
    public async Task ReadStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        await storage.ReadStateAsync(grainType, grainId, grainState).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        await storage.WriteStateAsync(grainType, grainId, grainState).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        await storage.ClearStateAsync(grainType, grainId, grainState).ConfigureAwait(false);
    }

    private Task<IGrainStorage> GetStorageAsync(GrainId grainId)
    {
        TenantKey.Split(grainId.Key.ToString()!, out var tenantId, out _);
        if (tenantId is null)
        {
            return Task.FromResult(_defaultStorage);
        }
        var lazy = _tenantStorages.GetOrAdd(tenantId, id => new Lazy<Task<IGrainStorage>>(() => CreateTenantStorageAsync(id)));
        return lazy.Value;
    }

    private async Task<IGrainStorage> CreateTenantStorageAsync(string tenantId)
    {
        try
        {
            var storage = await _createTenantStorage(tenantId, CancellationToken.None).ConfigureAwait(false);
            _failureLogger.Succeeded(tenantId);
            return storage;
        }
        catch (Exception ex)
        {
            _failureLogger.Failed(tenantId, ex);
            // Let the next call retry rather than caching the failure for the life of the silo.
            _tenantStorages.TryRemove(tenantId, out _);
            throw;
        }
    }
}
