namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     The outcome of materializing the event held by a cached KurrentDB record.
/// </summary>
/// <remarks>
///     Resolution happens in the data adapter, but the adapter is called from inside the pooled cache while it is
///     advancing a cursor, and that cursor has already moved past the record by the time the container is built.
///     Throwing there would be reported to the pulling agent as a cursor fault, which it recovers from by rebuilding the
///     cursor from the oldest cached event rather than by stopping. So a failure is carried out of the adapter as data
///     and only raised from <c>GetEvents</c>, on the delivery path, where Orleans retries, records a delivery failure,
///     and faults the subscription without advancing the consumer's progress token.
/// </remarks>
public readonly struct KurrentDBEventMaterialization
{
    private KurrentDBEventMaterialization(object? @event, string? fault, bool suppressed)
    {
        Event = @event;
        Fault = fault;
        IsSuppressed = suppressed;
    }

    /// <summary>
    ///     The materialized event, or <see langword="null" /> when the record is suppressed or faulted.
    /// </summary>
    public object? Event { get; }

    /// <summary>
    ///     The reason the event could not be materialized, raised when the batch is delivered.
    /// </summary>
    public string? Fault { get; }

    /// <summary>
    ///     Whether the record should be delivered as an empty batch instead of raising <see cref="Fault" />.
    /// </summary>
    public bool IsSuppressed { get; }

    /// <summary>
    ///     The event was materialized.
    /// </summary>
    public static KurrentDBEventMaterialization Resolved(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return new KurrentDBEventMaterialization(@event, null, false);
    }

    /// <summary>
    ///     The event could not be materialized and delivery should fail with <paramref name="fault" />.
    /// </summary>
    public static KurrentDBEventMaterialization Faulted(string fault)
    {
        return new KurrentDBEventMaterialization(null, fault, false);
    }

    /// <summary>
    ///     The event could not be materialized and the record should be delivered as an empty batch.
    /// </summary>
    public static KurrentDBEventMaterialization Suppressed()
    {
        return new KurrentDBEventMaterialization(null, null, true);
    }
}
