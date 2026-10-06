using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Orleans.EventSourcing;
using Orleans.EventSourcing.Common;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     A log view adaptor that wraps around a traditional storage adaptor, and uses batching and e-tags
///     to append entries.
///     <para>
///         The log itself is actually saved to storage - the latest view (snapshot) and metadata (the log position, and write flags)
///         and all log entries are stored in the primary.
///     </para>
/// </summary>
/// <typeparam name="TLogView">Type of log view</typeparam>
/// <typeparam name="TLogEntry">Type of log entry</typeparam>
internal class LogViewAdaptor<TLogView, TLogEntry> : PrimaryBasedLogViewAdaptor<TLogView, TLogEntry, SubmissionEntry<TLogEntry>>
    where TLogView : class, new()
    where TLogEntry : class
{
    private readonly IGrainStorage _grainStorage;
    private readonly string _grainTypeName;
    private readonly ILogConsistentStorage _logStorage;
    private readonly DeepCopier _deepCopier;

    private SnapshotWithMetaDataAndETag<TLogView> _globalSnapshot = new();
    private TLogView _confirmedView = new();
    private int _confirmedVersion;
    private int _globalVersion;

    /// <summary>
    ///     Initializes a new instance of LogViewAdaptor class
    /// </summary>
    public LogViewAdaptor(ILogViewAdaptorHost<TLogView, TLogEntry> host, TLogView initialState,
        IGrainStorage grainStorage,
        string grainTypeName,
        ILogConsistencyProtocolServices services,
        ILogConsistentStorage logStorage,
        DeepCopier deepCopier)
            : base(host, initialState, services)
    {
        ArgumentNullException.ThrowIfNull(grainStorage);
        ArgumentException.ThrowIfNullOrEmpty(grainTypeName);
        ArgumentNullException.ThrowIfNull(logStorage);

        _grainStorage = grainStorage;
        _grainTypeName = grainTypeName;
        _logStorage = logStorage;
        _deepCopier = deepCopier;
    }

    /// <inheritdoc />
    protected override void InitializeConfirmedView(TLogView initialState)
    {
        _globalSnapshot = new SnapshotWithMetaDataAndETag<TLogView>(initialState);
        _confirmedView = initialState;
        _confirmedVersion = 0;
        _globalVersion = 0;
    }

    /// <inheritdoc />
    protected override TLogView LastConfirmedView() => _confirmedView;

    /// <inheritdoc />
    protected override int GetConfirmedVersion() => _confirmedVersion;

    /// <inheritdoc />
    /// <remarks>
    ///     Returns the entries that move the log from version <paramref name="fromVersion"/> to version
    ///     <paramref name="toVersion"/>. As versions, <paramref name="fromVersion"/> is exclusive and
    ///     <paramref name="toVersion"/> is inclusive. As zero-based log positions (the entry producing version v is at
    ///     position v - 1), the same range is <paramref name="fromVersion"/> inclusive to <paramref name="toVersion"/>
    ///     exclusive, which is how Orleans defines it.
    ///     For example, (3, 5) returns the entries that produce versions 4 and 5 (positions 3 and 4), and
    ///     (0, n) returns the whole log.
    /// </remarks>
    public override Task<IReadOnlyList<TLogEntry>> RetrieveLogSegment(int fromVersion, int toVersion)
        => _logStorage.ReadAsync<TLogEntry>(_grainTypeName, Services.GrainId, fromVersion, toVersion - fromVersion);

    /// <inheritdoc />
    protected override SubmissionEntry<TLogEntry> MakeSubmissionEntry(TLogEntry entry)
        => new SubmissionEntry<TLogEntry> { Entry = entry };

    private void UpdateConfirmedView(IReadOnlyList<TLogEntry> logEntries)
    {
        foreach (var logEntry in logEntries)
        {
            try
            {
                Host.UpdateView(_confirmedView, logEntry);
            }
            catch (Exception ex)
            {
                Services.CaughtUserCodeException("UpdateView", nameof(UpdateConfirmedView), ex);
            }
        }
        _confirmedVersion += logEntries.Count;
    }

    #region Read & Write

    /// <inheritdoc />
    protected override async Task ReadAsync()
    {
        enter_operation("ReadAsync");

        while (true)
        {
            try
            {
                var snapshot = new SnapshotWithMetaDataAndETag<TLogView>();
                await _grainStorage.ReadStateAsync(_grainTypeName, Services.GrainId, snapshot);
                _globalSnapshot = snapshot;
                Services.Log(LogLevel.Debug, "read success {0}", _globalSnapshot);
                if (_confirmedVersion < _globalSnapshot.State.SnapshotVersion)
                {
                    _confirmedVersion = _globalSnapshot.State.SnapshotVersion;
                    _confirmedView = _deepCopier.Copy(_globalSnapshot.State.Snapshot)!;
                }
                try
                {
                    _globalVersion = await _logStorage.GetLastVersionAsync(_grainTypeName, Services.GrainId);
                    if (_confirmedVersion < _globalVersion)
                    {
                        var logEntries = await RetrieveLogSegment(_confirmedVersion, _globalVersion);
                        Services.Log(LogLevel.Debug, "read success {0}", logEntries);
                        UpdateConfirmedView(logEntries);
                    }
                    LastPrimaryIssue.Resolve(Host, Services);
                    break; // successful
                }
                catch (Exception ex)
                {
                    LastPrimaryIssue.Record(new ReadFromLogStorageFailed { Exception = ex }, Host, Services);
                }
            }
            catch (Exception ex)
            {
                LastPrimaryIssue.Record(new ReadFromSnapshotStorageFailed { Exception = ex }, Host, Services);
            }
            Services.Log(LogLevel.Warning, "read failed {0}", LastPrimaryIssue);
            await LastPrimaryIssue.DelayBeforeRetry();
        }

        exit_operation("ReadAsync");
    }

    /// <inheritdoc />
    protected override async Task<int> WriteAsync()
    {
        enter_operation("WriteAsync");

        var updates = GetCurrentBatchOfUpdates();
        var logsSuccessfullyAppended = false;
        var batchSuccessfullyWritten = false;
        var writeBit = _globalSnapshot.State.FlipBit(Services.MyClusterId);

        try
        {
            var logEntries = updates.Select(x => x.Entry!).ToImmutableList();
            _globalVersion = await _logStorage.AppendAsync(_grainTypeName, Services.GrainId, logEntries, _globalVersion);
            logsSuccessfullyAppended = true;
            Services.Log(LogLevel.Debug, "write success {0}", logEntries);
            UpdateConfirmedView(logEntries);
        }
        catch (Exception ex)
        {
            LastPrimaryIssue.Record(new UpdateLogStorageFailed { Exception = ex }, Host, Services);
        }
        if (logsSuccessfullyAppended)
        {
            try
            {
                _globalSnapshot.State.Snapshot = _deepCopier.Copy(_confirmedView)!;
                _globalSnapshot.State.SnapshotVersion = _confirmedVersion;
                await _grainStorage.WriteStateAsync(_grainTypeName, Services.GrainId, _globalSnapshot);
                batchSuccessfullyWritten = true;
                Services.Log(LogLevel.Debug, "write ({0} updates) success {1}", updates.Length, _globalSnapshot);
                LastPrimaryIssue.Resolve(Host, Services);
            }
            catch (Exception ex)
            {
                LastPrimaryIssue.Record(new UpdateSnapshotStorageFailed { Exception = ex }, Host, Services);
            }
        }
        if (!batchSuccessfullyWritten)
        {
            Services.Log(LogLevel.Warning, "write apparently failed {0}", LastPrimaryIssue);
            while (true) // be stubborn until we can read what is there
            {
                await LastPrimaryIssue.DelayBeforeRetry();
                try
                {
                    var snapshot = new SnapshotWithMetaDataAndETag<TLogView>();
                    await _grainStorage.ReadStateAsync(_grainTypeName, Services.GrainId, snapshot);
                    _globalSnapshot = snapshot;
                    Services.Log(LogLevel.Debug, "read success {0}", _globalSnapshot);
                    if (_confirmedVersion < _globalSnapshot.State.SnapshotVersion)
                    {
                        _confirmedVersion = _globalSnapshot.State.SnapshotVersion;
                        _confirmedView = _deepCopier.Copy(_globalSnapshot.State.Snapshot)!;
                    }
                    try
                    {
                        _globalVersion = await _logStorage.GetLastVersionAsync(_grainTypeName, Services.GrainId);
                        if (_confirmedVersion < _globalVersion)
                        {
                            var logEntries = await RetrieveLogSegment(_confirmedVersion, _globalVersion);
                            Services.Log(LogLevel.Debug, "read success {0}", logEntries);
                            UpdateConfirmedView(logEntries);
                        }
                        LastPrimaryIssue.Resolve(Host, Services);
                        break; // successful
                    }
                    catch (Exception ex)
                    {
                        LastPrimaryIssue.Record(new ReadFromLogStorageFailed { Exception = ex }, Host, Services);
                    }
                }
                catch (Exception ex)
                {
                    LastPrimaryIssue.Record(new ReadFromSnapshotStorageFailed { Exception = ex }, Host, Services);
                }
                Services.Log(LogLevel.Warning, "read failed {0}", LastPrimaryIssue);
            }
            // Check if last apparently failed write was in fact successful
            if (writeBit == _globalSnapshot.State.GetBit(Services.MyClusterId))
            {
                Services.Log(LogLevel.Debug, "last write ({0} updates) was actually a success {1}", updates.Length, _globalSnapshot);
                batchSuccessfullyWritten = true;
            }
        }

        exit_operation("WriteAsync");

        return batchSuccessfullyWritten ? updates.Length : 0;
    }

    #endregion

    // The notification handling (Merge, OnNotificationReceived, ProcessNotifications and UpdateNotificationMessage) that
    // Orleans' own adaptors carry comes from the multi-cluster replication feature, which is not active in Orleans 7+.
    // Nothing sends these notifications, so the queue logic was removed. Cross-region freshness, if needed later,
    // should come from store subscriptions. WriteVector and MyClusterId are still used by WriteAsync to detect
    // whether an apparently failed write actually succeeded.

    #region Operation Failed Classes

    /// <summary>
    ///     Describes a connection issue that occurred when reading from the primary storage.
    /// </summary>
    [GenerateSerializer, Immutable]
    public sealed class ReadFromSnapshotStorageFailed : PrimaryOperationFailed
    {
        /// <inheritdoc />
        public override string ToString()
            => $"read state from snapshot storage failed: caught {Exception.GetType().Name}: {Exception.Message}";
    }

    /// <summary>
    ///     Describes a connection issue that occurred when updating the primary storage.
    /// </summary>
    [GenerateSerializer, Immutable]
    public sealed class UpdateSnapshotStorageFailed : PrimaryOperationFailed
    {
        /// <inheritdoc />
        public override string ToString()
            => $"write state to snapshot storage failed: caught {Exception.GetType().Name}: {Exception.Message}";
    }

    /// <summary>
    ///     Describes a connection issue that occurred when reading from the primary storage.
    /// </summary>
    [GenerateSerializer, Immutable]
    public sealed class ReadFromLogStorageFailed : PrimaryOperationFailed
    {
        /// <inheritdoc />
        public override string ToString()
            => $"read logs from storage failed: caught {Exception.GetType().Name}: {Exception.Message}";
    }

    /// <summary>
    ///     Describes a connection issue that occurred when updating the primary storage.
    /// </summary>
    [GenerateSerializer, Immutable]
    public sealed class UpdateLogStorageFailed : PrimaryOperationFailed
    {
        /// <inheritdoc />
        public override string ToString()
            => $"write logs to storage failed: caught {Exception.GetType().Name}: {Exception.Message}";
    }

    #endregion

    #region Debug

#if DEBUG
    private bool operation_in_progress;
#endif

    [Conditional("DEBUG")]
    private void enter_operation(string name)
    {
#if DEBUG
        Services.Log(LogLevel.Trace, "/-- enter {0}", name);
        Debug.Assert(!operation_in_progress);
        operation_in_progress = true;
#endif
    }

    [Conditional("DEBUG")]
    private void exit_operation(string name)
    {
#if DEBUG
        Services.Log(LogLevel.Trace, "\\-- exit {0}", name);
        Debug.Assert(operation_in_progress);
        operation_in_progress = false;
#endif
    }

    #endregion

}
