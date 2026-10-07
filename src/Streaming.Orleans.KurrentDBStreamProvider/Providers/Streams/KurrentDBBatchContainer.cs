using Continuum;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;

using Newtonsoft.Json;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Providers.Streams.KurrentDB;

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
        Token = new KurrentDBSequenceToken(kurrentDBMessage.Position, kurrentDBMessage.SequenceNumber, 0);
    }

    [JsonProperty]
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
    [JsonProperty]
    [Id(2)]
    private object? Payload { get; }

    /// <summary>
    ///     Why the event could not be materialized, raised when this batch is delivered.
    /// </summary>
    [JsonProperty]
    [Id(3)]
    private string? Fault { get; }

    /// <summary>
    ///     Whether an unresolvable event should be delivered as an empty batch instead of raising <see cref="Fault" />.
    /// </summary>
    [JsonProperty]
    [Id(4)]
    private bool IsSuppressed { get; }

    /// <summary>
    ///     Ges the stream identifier for the stream this batch is part of.
    /// </summary>
    public StreamId StreamId => KurrentDBMessage.StreamId;

    /// <summary>
    ///     Ges the stream sequence token for the start of this batch.
    /// </summary>
    [JsonProperty]
    [Id(1)]
    private KurrentDBSequenceToken Token { get; set; } = new();

    /// <summary>
    ///     Ges the stream sequence token for the start of this batch.
    /// </summary>
    public StreamSequenceToken SequenceToken => Token;

    /// <inheritdoc />
    public ulong CommitPosition => KurrentDBMessage.CommitPosition;

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
    ///     A record holds exactly one event, the same way the event sourcing storage writes a log entry, so a batch
    ///     yields a single streamed event. Events appended together are told apart by their own position in the log
    ///     rather than by an index within a record, and the append they belong to is recorded in the transaction
    ///     values on <see cref="IStreamedEvent{TEvent}.Metadata" />.
    /// </remarks>
    public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
    {
        if (!typeof(T).IsAssignableFrom(typeof(IStreamedEvent<object>)))
        {
            throw new InvalidOperationException($"This stream provider delivers events as {nameof(IStreamedEvent<object>)}, but a consumer subscribed as \"{typeof(T)}\". Subscribe as IStreamedEvent<object> and read the domain event from its Event property.");
        }
        if (IsSuppressed)
        {
            // Delivering nothing lets the agent record progress and move on, which is what the skip behavior asks for.
            return [];
        }
        if (Fault is not null)
        {
            // Raised here, on the delivery path, so the agent retries and faults the subscription rather than
            // advancing the consumer past an event it never received.
            throw new InvalidOperationException(Fault);
        }
        return [Tuple.Create((T)(object)ToStreamedEvent(Payload ?? throw new InvalidOperationException("A resolved KurrentDB record has no event.")), (StreamSequenceToken)new KurrentDBSequenceToken(Token.Position, Token.SequenceNumber, 0))];
    }

    /// <summary>
    ///     Rebuilds the streamed event envelope for the event this record holds.
    /// </summary>
    private StreamedEvent<object> ToStreamedEvent(object @event)
    {
        return new StreamedEvent<object>(
            NewId.FromSequentialGuid(Guid.Parse(KurrentDBMessage.EventId)),
            KurrentDBMessage.EventType,
            KurrentDBMessage.StreamName,
            KurrentDBMessage.StreamKey,
            KurrentDBMessage.Topic,
            StreamEventExtensions.DefaultPartitionId,
            KurrentDBMessage.StreamVersion,
            KurrentDBMessage.StreamVersion,
            KurrentDBMessage.CommitPosition,
            0,
            KurrentDBMessage.Timestamp,
            @event,
            KurrentDBMessage.Metadata);
    }

    /// <summary>
    ///     Gives an opportunity to <see cref="IBatchContainer" /> to set any data in the <see cref="Runtime.RequestContext" /> before this <see cref="IBatchContainer" /> is sent to consumers.
    ///     It can be the data that was set at the time event was generated and enqueued into the persistent provider or any other data.
    /// </summary>
    /// <returns><see langword="true" /> if the <see cref="Runtime.RequestContext" /> was indeed modified, <see langword="false" /> otherwise.</returns>
    /// <remarks>
    ///     Only tracing is restored; see <see cref="KurrentDBBatchContainerV2.ImportRequestContext" />.
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
