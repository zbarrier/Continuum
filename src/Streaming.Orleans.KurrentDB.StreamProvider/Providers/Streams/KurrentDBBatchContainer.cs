using Continuum;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;


using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Each queue message is allowed to be a heterogeneous, ordered set of events.
///     <see cref="IBatchContainer" /> contains these events and allows users to query the batch for a specific type of event.
/// </summary>
[Serializable]
[GenerateSerializer]
public class KurrentDBBatchContainer : IBatchContainer, IKurrentDBCommitPositionBatch
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBBatchContainer" /> class.
    /// </summary>
    [GeneratedActivatorConstructor]
    public KurrentDBBatchContainer()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBBatchContainer" /> class.
    ///     Batch container that delivers events from cached KurrentDB data associated with an orleans stream
    /// </summary>
    /// <param name="kurrentDBMessage"></param>
    /// <param name="materialization">The outcome of materializing the event, produced by the data adapter.</param>
    public KurrentDBBatchContainer(KurrentDBMessage kurrentDBMessage, KurrentDBEventMaterialization materialization)
    {
        ArgumentNullException.ThrowIfNull(kurrentDBMessage, nameof(kurrentDBMessage));
        KurrentDBMessage = kurrentDBMessage;
        Payload = materialization.Event;
        Fault = materialization.Fault;
        IsSuppressed = materialization.IsSuppressed;
        Token = new EventSequenceToken(kurrentDBMessage.SequenceNumber, 0);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBBatchContainer" /> class holding every event of one
    ///     transaction.
    /// </summary>
    /// <param name="kurrentDBMessages">The events of the transaction, in log order.</param>
    /// <param name="materializations">The outcome of materializing each event.</param>
    /// <remarks>
    ///     The container is positioned at the last event, so a consumer's token, the cache cursor and the checkpoint all
    ///     move past the transaction only once every event of it has been delivered.
    /// </remarks>
    public KurrentDBBatchContainer(IReadOnlyList<KurrentDBMessage> kurrentDBMessages, IReadOnlyList<KurrentDBEventMaterialization> materializations)
    {
        ArgumentNullException.ThrowIfNull(kurrentDBMessages, nameof(kurrentDBMessages));
        ArgumentNullException.ThrowIfNull(materializations, nameof(materializations));
        if (kurrentDBMessages.Count == 0 || kurrentDBMessages.Count != materializations.Count)
        {
            throw new ArgumentException("A batch must hold at least one event and one materialization per event.", nameof(materializations));
        }
        KurrentDBMessage = kurrentDBMessages[^1];
        Payload = materializations[^1].Event;
        Fault = materializations[^1].Fault;
        IsSuppressed = materializations[^1].IsSuppressed;
        Token = new EventSequenceToken(KurrentDBMessage.SequenceNumber, 0);
        if (kurrentDBMessages.Count > 1)
        {
            Preceding = new KurrentDBBatchContainer[kurrentDBMessages.Count - 1];
            for (var i = 0; i < Preceding.Length; i++)
            {
                Preceding[i] = new KurrentDBBatchContainer(kurrentDBMessages[i], materializations[i]);
            }
        }
    }

    /// <summary>
    ///     The earlier events of the transaction, each held as a single event record, or <see langword="null" /> when
    ///     the transaction has one event.
    /// </summary>
    [Id(5)]
    private KurrentDBBatchContainer[]? Preceding { get; }

    [Id(0)]
    private KurrentDBMessage KurrentDBMessage { get; } = null!;

    /// <summary>
    ///     The event this record holds.
    /// </summary>
    /// <remarks>
    ///     Materialized by the data adapter, which owns the type mapper and serializer, rather than resolved here. A
    ///     container can be serialized and rebuilt through its activator when it crosses a call boundary, and the
    ///     adapter's options are registered per provider name, so they cannot be recovered at that point.
    /// </remarks>
    [Id(2)]
    private object? Payload { get; }

    /// <summary>
    ///     Why the event could not be materialized, raised when this batch is delivered.
    /// </summary>
    [Id(3)]
    private string? Fault { get; }

    /// <summary>
    ///     Whether an unresolvable event should be delivered as an empty batch instead of raising <see cref="Fault" />.
    /// </summary>
    [Id(4)]
    private bool IsSuppressed { get; }

    /// <summary>
    ///     Ges the stream identifier for the stream this batch is part of.
    /// </summary>
    public StreamId StreamId => KurrentDBMessage.StreamId;

    /// <summary>
    ///     Ges the stream sequence token for the start of this batch.
    /// </summary>
    [Id(1)]
    private EventSequenceToken Token { get; set; } = new();

    /// <summary>
    ///     Ges the stream sequence token for the start of this batch.
    /// </summary>
    public StreamSequenceToken SequenceToken => Token;

    /// <inheritdoc />
    public ulong CommitPosition => KurrentDBMessage.CommitPosition;

    // Built on first delivery and reused, so the tenant qualified partition is not rebuilt for every consumer.
    [NonSerialized]
    private StreamedEventCursor? _cursor;

    #region IBatchContainer Implementation

    /// <summary>
    ///     Gets the events in this batch, as the streamed event envelope every consumer of this provider receives.
    /// </summary>
    /// <typeparam name="T">Must be satisfiable by <see cref="IStreamedEvent{TEvent}" /> of <see cref="object" />.</typeparam>
    /// <remarks>
    ///     Consumers always receive <see cref="IStreamedEvent{TEvent}" /> rather than the bare domain event, so the
    ///     information the event carried about itself, its position, tracing and metadata, survives delivery instead of
    ///     being flattened away. The envelope is rebuilt here rather than cached, because most of what it holds is
    ///     either already in the cached message or constant for the provider, and reconstructing costs one allocation
    ///     per delivery instead of a larger footprint for the whole time the event sits in the cache.
    ///
    ///     A record holds one transaction: the events appended together, in log order. A single-event append yields
    ///     a single streamed event. Each event keeps its own commit position, and the transaction it belongs to is
    ///     recorded in the transaction values on <see cref="IStreamedEvent{TEvent}.Metadata" />.
    /// </remarks>
    public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
    {
        if (!typeof(T).IsAssignableFrom(typeof(IStreamedEvent<object>)))
        {
            throw new InvalidOperationException($"This stream provider delivers events as {nameof(IStreamedEvent<object>)}, but a consumer subscribed as \"{typeof(T)}\". Subscribe as IStreamedEvent<object> and read the domain event from its Event property.");
        }
        // A transaction is delivered whole or not at all: every event is checked for a fault before any of it is handed
        // out, so a consumer never receives part of a transaction. Suppressed events are left out on their own.
        var records = Preceding is { Length: > 0 } preceding ? [.. preceding, this] : new[] { this };
        foreach (var record in records)
        {
            if (!record.IsSuppressed && record.Fault is { } fault)
            {
                // Raised here, on the delivery path, so the agent retries and faults the subscription rather than
                // advancing the consumer past an event it never received.
                throw new InvalidOperationException(fault);
            }
        }
        // Each event is tokened with its own commit position, which is unique, so tokens rise through the batch and no
        // index is needed. A consumer only delivers an item newer than the last one it delivered, so a token shared by
        // the whole batch would drop every event after the first. The batch itself is positioned at the last event.
        var events = new List<Tuple<T, StreamSequenceToken>>(records.Length);
        foreach (var record in records)
        {
            if (record.IsSuppressed)
            {
                continue;
            }
            var payload = record.Payload ?? throw new InvalidOperationException("A resolved KurrentDB record has no event.");
            events.Add(Tuple.Create((T)(object)record.ToStreamedEvent(payload), (StreamSequenceToken)new EventSequenceToken(record.KurrentDBMessage.SequenceNumber, 0)));
        }
        return events;
    }

    /// <summary>
    ///     Rebuilds the streamed event envelope for one event in the batch.
    /// </summary>
    /// <summary>
    ///     Rebuilds the streamed event envelope for the event this record holds.
    /// </summary>
    private StreamedEvent<object> ToStreamedEvent(object @event)
    {
        var streamName = KurrentDBMessage.StreamName;
        return new StreamedEvent<object>(
            KurrentDBMessage.EventId,
            KurrentDBMessage.EventType,
            streamName,
            KurrentDBMessage.StreamVersion,
            _cursor ??= KurrentDBStreamedEventConversions.ToCursor(KurrentDBMessage.SourceConnectionName, streamName, KurrentDBMessage.CommitPosition),
            KurrentDBMessage.Timestamp,
            @event,
            KurrentDBMessage.Metadata);
    }

    /// <summary>
    ///     Gives an opportunity to <see cref="IBatchContainer" /> to set any data in the <see cref="global::Orleans.Runtime.RequestContext" /> before this <see cref="IBatchContainer" /> is sent to consumers.
    ///     It can be the data that was set at the time event was generated and enqueued into the persistent provider or any other data.
    /// </summary>
    /// <returns><see langword="true" /> if the <see cref="global::Orleans.Runtime.RequestContext" /> was indeed modified, <see langword="false" /> otherwise.</returns>
    /// <remarks>
    ///     Only tracing is restored, rebuilt as a W3C traceparent from the ids the event carried. The rest of the
    ///     producer's request context is ambient call-chain state that belonged to the grain call that produced the
    ///     event, not information about the event, so replaying it onto an unrelated consumer call would be wrong.
    /// </remarks>
    public bool ImportRequestContext()
    {
        if (KurrentDBMessage.Metadata.TraceId is not { Length: > 0 } traceId || KurrentDBMessage.Metadata.SpanId is not { Length: > 0 } spanId)
        {
            return false;
        }
        RequestContext.Set("traceparent", $"00-{traceId}-{spanId}-01");
        return true;
    }

    #endregion
}
