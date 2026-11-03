using Orleans.EventSourcing;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans.KurrentDB;

/// <summary>
///     A log-consistency provider that appends events to an <see cref="ILogConsistentStorage" /> event log and stores the
///     latest view as a snapshot in any standard grain storage provider.
///     <para>
///         The event log is the source of truth; the snapshot is only a cache. See <see cref="LogViewAdaptor{TLogView,TLogEntry}" />.
///     </para>
/// </summary>
public class LogConsistencyProvider : ILogViewAdaptorFactory
{
    private readonly ILogConsistentStorage _logConsistentStorage;
    private readonly DeepCopier _deepCopier;
    private readonly ITransactionIdGenerator _transactionIdGenerator;

    /// <summary>
    ///     Initializes a new instance of LogConsistencyProvider class
    /// </summary>
    /// <param name="logConsistentStorage"></param>
    /// <param name="deepCopier"></param>
    /// <param name="transactionIdGenerator">Creates the transaction ID stored on every entry of an append.</param>
    public LogConsistencyProvider(ILogConsistentStorage logConsistentStorage, DeepCopier deepCopier, ITransactionIdGenerator transactionIdGenerator)
    {
        ArgumentNullException.ThrowIfNull(logConsistentStorage);
        ArgumentNullException.ThrowIfNull(deepCopier);
        ArgumentNullException.ThrowIfNull(transactionIdGenerator);

        _logConsistentStorage = logConsistentStorage;
        _deepCopier = deepCopier;
        _transactionIdGenerator = transactionIdGenerator;
    }

    /// <inheritdoc />
    public ILogViewAdaptor<TLogView, TLogEntry> MakeLogViewAdaptor<TLogView, TLogEntry>(ILogViewAdaptorHost<TLogView, TLogEntry> hostGrain,
        TLogView initialState, string grainTypeName, IGrainStorage? grainStorage, ILogConsistencyProtocolServices services)
        where TLogView : class, new()
        where TLogEntry : class
    {
        return new LogViewAdaptor<TLogView, TLogEntry>(hostGrain, initialState, grainStorage!, grainTypeName, services, _logConsistentStorage, _deepCopier, _transactionIdGenerator);
    }

    /// <inheritdoc />
    public bool UsesStorageProvider => true;
}
