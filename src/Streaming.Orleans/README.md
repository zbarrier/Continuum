# Continuum.Streaming.Orleans

Microsoft Orleans integration for Continuum streaming.

- `StreamedEvent<T>` / `StreamedEventMetadata` - Orleans-serializable streamed events and metadata. Events are never null; constructing a `StreamedEvent<T>` with a null payload throws.
- `ProjectionGrain<TGrain, TState>` / `ProjectionState<TState>` - single-state projections fed by Orleans stream subscriptions (explicit or implicit) and by catch-up subscriptions through `IProjectionGrain.OnNextBatchAsync`. Both paths share one apply path with redelivery protection by topic and stream key.
- `StreamedEventCursor` - Orleans-serializable implementation of `IStreamedEventCursor`.
- `ProjectionWatermarks` - per topic/partition high-water marks used for duplicate detection.
- `StreamedEventSequence` - comparable sequence/sub-sequence position.
