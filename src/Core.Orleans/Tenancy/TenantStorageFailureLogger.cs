using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Continuum.Orleans.Tenancy;

/// <summary>
///     Logs failures to create a tenant's storage.
/// </summary>
/// <remarks>
///     Every failure is logged as an error while a tenant keeps failing for less than <see cref="FullLoggingPeriod"/>,
///     so a misconfigured tenant is immediately visible. After that, failures for the tenant are logged at most once
///     every <see cref="ThrottledLoggingInterval"/>, with the number of failures suppressed in between. A successful
///     creation resets the tenant.
/// </remarks>
public sealed class TenantStorageFailureLogger
{
    /// <summary>
    ///     How long every failure for a tenant is logged before logging is throttled.
    /// </summary>
    public static readonly TimeSpan FullLoggingPeriod = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     The minimum time between logged failures for a tenant once logging is throttled.
    /// </summary>
    public static readonly TimeSpan ThrottledLoggingInterval = TimeSpan.FromMinutes(1);

    private readonly ILogger _logger;
    private readonly string _providerName;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, FailureState> _failures = new(StringComparer.Ordinal);

    /// <summary>
    ///     Creates a new <see cref="TenantStorageFailureLogger"/>.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="providerName">The storage provider name included in each log entry.</param>
    /// <param name="timeProvider">The time provider; defaults to <see cref="TimeProvider.System"/>.</param>
    public TenantStorageFailureLogger(ILogger logger, string providerName, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        _logger = logger;
        _providerName = providerName;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    ///     Records that creating the storage for <paramref name="tenantId"/> failed.
    /// </summary>
    public void Failed(string tenantId, Exception exception)
    {
        var now = _timeProvider.GetUtcNow();
        var state = _failures.GetOrAdd(tenantId, _ => new FailureState(now));
        int suppressed;
        lock (state)
        {
            state.Count++;
            var throttled = now - state.FirstFailure >= FullLoggingPeriod;
            if (throttled && state.LastLogged is { } lastLogged && now - lastLogged < ThrottledLoggingInterval)
            {
                state.Suppressed++;
                return;
            }
            suppressed = state.Suppressed;
            state.Suppressed = 0;
            state.LastLogged = now;
        }
        _logger.LogError(exception,
            "Failed to create storage for tenant {TenantId} in provider {ProviderName}. The tenant has been failing since {FirstFailure} ({FailureCount} failures, {SuppressedCount} not logged since the last entry). The next call for the tenant will retry.",
            tenantId, _providerName, state.FirstFailure, state.Count, suppressed);
    }

    /// <summary>
    ///     Records that the storage for <paramref name="tenantId"/> was created, ending any failure period.
    /// </summary>
    public void Succeeded(string tenantId)
    {
        if (_failures.TryRemove(tenantId, out var state))
        {
            _logger.LogInformation(
                "Created storage for tenant {TenantId} in provider {ProviderName} after {FailureCount} failures since {FirstFailure}.",
                tenantId, _providerName, state.Count, state.FirstFailure);
        }
    }

    private sealed class FailureState(DateTimeOffset firstFailure)
    {
        public DateTimeOffset FirstFailure { get; } = firstFailure;

        public DateTimeOffset? LastLogged { get; set; }

        public int Count { get; set; }

        public int Suppressed { get; set; }
    }
}
