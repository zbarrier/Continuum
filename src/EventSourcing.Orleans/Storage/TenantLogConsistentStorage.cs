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
public sealed class TenantLogConsistentStorage : IMultiStreamLogConsistentStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly ILogConsistentStorage _defaultStorage;
    private readonly Func<string, CancellationToken, Task<ILogConsistentStorage>> _createTenantStorage;
    private readonly TenantStorageFailureLogger _failureLogger;
    private readonly ConcurrentDictionary<string, Lazy<Task<ILogConsistentStorage>>> _tenantStorages = new(StringComparer.Ordinal);

    /// <summary>
    ///     Creates a new <see cref="TenantLogConsistentStorage"/>.
    /// </summary>
    /// <param name="defaultStorage">The storage used by grains without a tenant.</param>
    /// <param name="createTenantStorage">Creates and initializes the storage for a tenant.</param>
    /// <param name="failureLogger">Logs failures to create a tenant's storage.</param>
    public TenantLogConsistentStorage(ILogConsistentStorage defaultStorage, Func<string, CancellationToken, Task<ILogConsistentStorage>> createTenantStorage,
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
    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion, Guid transactionId,
        IReadOnlyList<IReadOnlyList<string>?>? streamTargets = null)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        return await storage.AppendAsync(grainTypeName, grainId, entries, expectedVersion, transactionId, streamTargets).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Guid?> GetTransactionIdAsync(string grainTypeName, GrainId grainId, int version)
    {
        var storage = await GetStorageAsync(grainId).ConfigureAwait(false);
        return await storage.GetTransactionIdAsync(grainTypeName, grainId, version).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">The grains do not all belong to the same tenant.</exception>
    /// <exception cref="NotSupportedException">The tenant's storage does not support multi-stream appends.</exception>
    public async Task<IReadOnlyList<int>> AppendAsync<TLogEntry>(IReadOnlyList<LogStream> streams, IReadOnlyList<LogRecord<TLogEntry>> records, Guid transactionId)
    {
        ArgumentNullException.ThrowIfNull(streams);
        if (streams.Count == 0)
        {
            throw new ArgumentException("At least one stream is required.", nameof(streams));
        }
        // A transaction cannot span tenants, since each tenant may be stored in a different database.
        var tenantId = GetTenantId(streams[0].GrainId);
        for (var index = 1; index < streams.Count; index++)
        {
            if (!string.Equals(GetTenantId(streams[index].GrainId), tenantId, StringComparison.Ordinal))
            {
                throw new ArgumentException("A multi-stream append cannot span tenants.", nameof(streams));
            }
        }
        var storage = await GetStorageAsync(tenantId).ConfigureAwait(false);
        if (storage is not IMultiStreamLogConsistentStorage multiStreamStorage)
        {
            throw new NotSupportedException("The tenant's log-consistent storage does not support multi-stream appends.");
        }
        return await multiStreamStorage.AppendAsync(streams, records, transactionId).ConfigureAwait(false);
    }

    private static string? GetTenantId(GrainId grainId)
    {
        TenantKey.Split(grainId.Key.ToString()!, out var tenantId, out _);
        return tenantId;
    }

    private Task<ILogConsistentStorage> GetStorageAsync(GrainId grainId) => GetStorageAsync(GetTenantId(grainId));

    private Task<ILogConsistentStorage> GetStorageAsync(string? tenantId)
    {
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
