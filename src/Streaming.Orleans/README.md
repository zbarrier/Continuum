# Continuum.Streaming.Orleans

Microsoft Orleans integration for Continuum streaming.

- `StreamedEvent<T>` / `StreamedEventMetadata` - Orleans-serializable streamed events and metadata. Events are never null; constructing a `StreamedEvent<T>` with a null payload throws.
- `StreamSubscriberGrain<TGrain>` - base grain for explicit and implicit stream subscriptions.
- `StreamProjectionGrain` / `StreamProjectionState<TState>` - single-event projections with redelivery protection by topic and stream key.
- `ProjectionGrain` / `ProjectionState<TState, TEventArgs>` - batch projections with redelivery protection by topic and partition.
- `StreamSequence` - comparable sequence/sub-sequence position used for duplicate detection.
