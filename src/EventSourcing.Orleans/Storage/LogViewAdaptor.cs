using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Orleans.EventSourcing;
using Orleans.EventSourcing.Common;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     A log view adaptor that appends batches of entries to an <see cref="ILogConsistentStorage" /> event log and keeps
///     the latest view as a snapshot in an <see cref="IGrainStorage" />.
///     <para>
///         The event log is the source of truth. The snapshot, with its version and e-tag, is only a cache that
///         speeds up activation; when it is missing or stale, the view is rebuilt from the log.
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
    private readonly ITransactionIdGenerator _transactionIdGenerator;

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
        DeepCopier deepCopier,
        ITransactionIdGenerator transactionIdGenerator)
            : base(host, initialState, services)
    {
        ArgumentNullException.ThrowIfNull(grainStorage);
        ArgumentException.ThrowIfNullOrEmpty(grainTypeName);
        ArgumentNullException.ThrowIfNull(logStorage);
        ArgumentNullException.ThrowIfNull(transactionIdGenerator);

        _grainStorage = grainStorage;
        _grainTypeName = grainTypeName;
        _logStorage = logStorage;
        _deepCopier = deepCopier;
        _transactionIdGenerator = transactionIdGenerator;
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
        var expectedVersion = _globalVersion;
        // A new ID for every attempt: an unknown outcome is always settled within this call, so a retry never needs
        // to recognize an earlier attempt.
        var transactionId = _transactionIdGenerator.NewTransactionId();
        var batchAppended = false;

        try
        {
            var logEntries = updates.Select(x => x.Entry!).ToImmutableList();
            _globalVersion = await _logStorage.AppendAsync(_grainTypeName, Services.GrainId, logEntries, expectedVersion, transactionId);
            // The log is the source of truth and the snapshot is only a cache, so the batch is written once the append
            // succeeds. Reporting a failed snapshot write as a failed batch would make Orleans retry and append the
            // same entries again.
            batchAppended = true;
            Services.Log(LogLevel.Debug, "write success {0}", logEntries);
            UpdateConfirmedView(logEntries);
        }
        catch (Exception ex)
        {
            LastPrimaryIssue.Record(new UpdateLogStorageFailed { Exception = ex }, Host, Services);
        }
        if (batchAppended)
        {
            try
            {
                _globalSnapshot.State.Snapshot = _deepCopier.Copy(_confirmedView)!;
                _globalSnapshot.State.SnapshotVersion = _confirmedVersion;
                await _grainStorage.WriteStateAsync(_grainTypeName, Services.GrainId, _globalSnapshot);
                Services.Log(LogLevel.Debug, "write ({0} updates) success {1}", updates.Length, _globalSnapshot);
                LastPrimaryIssue.Resolve(Host, Services);
            }
            catch (Exception ex)
            {
                LastPrimaryIssue.Record(new UpdateSnapshotStorageFailed { Exception = ex }, Host, Services);
                Services.Log(LogLevel.Warning, "snapshot write failed after a successful append {0}", LastPrimaryIssue);
                await RefreshSnapshotETag();
            }
        }
        else
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
                        // The append may have been committed even though it threw (for example, a timeout). If the
                        // entry after the expected version carries this attempt's transaction ID, the whole batch is in
                        // the log; it was just read into the confirmed view, so it must not be submitted again.
                        if (_globalVersion > expectedVersion &&
                            await _logStorage.GetTransactionIdAsync(_grainTypeName, Services.GrainId, expectedVersion + 1) == transactionId)
                        {
                            Services.Log(LogLevel.Debug, "last write ({0} updates) was actually a success {1}", updates.Length, _globalSnapshot);
                            batchAppended = true;
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
        }

        exit_operation("WriteAsync");

        return batchAppended ? updates.Length : 0;
    }

    #endregion

    /// <summary>
    ///     Re-reads the stored snapshot's ETag after a failed snapshot write, so the next write is not rejected because
    ///     the failed write actually reached storage. The in-memory view is kept, since it already includes the
    ///     appended entries. A failed read is only recorded; the next write will try again.
    /// </summary>
    private async Task RefreshSnapshotETag()
    {
        try
        {
            var stored = new SnapshotWithMetaDataAndETag<TLogView>();
            await _grainStorage.ReadStateAsync(_grainTypeName, Services.GrainId, stored);
            _globalSnapshot.ETag = stored.ETag;
            _globalSnapshot.RecordExists = stored.RecordExists;
        }
        catch (Exception ex)
        {
            LastPrimaryIssue.Record(new ReadFromSnapshotStorageFailed { Exception = ex }, Host, Services);
        }
    }

    // The notification handling (Merge, OnNotificationReceived, ProcessNotifications and UpdateNotificationMessage) that
    // Orleans' own adaptors carry comes from the multi-cluster replication feature, which is not active in Orleans 7+.
    // Nothing sends these notifications, so the queue logic was removed. Cross-region freshness, if needed later,
    // should come from store subscriptions.

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
