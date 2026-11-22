# Continuum.Streaming.Orleans

Microsoft Orleans integration for Continuum streaming.

- `StreamedEvent<T>` / `StreamedEventMetadata` - Orleans-serializable streamed events and metadata. Events are never null; constructing a `StreamedEvent<T>` with a null payload throws.
- `ProjectionGrain<TGrain, TState>` / `ProjectionState<TState>` - single-state projections fed by Orleans stream subscriptions (explicit or implicit) and by catch-up subscriptions through `IProjectionGrain.OnNextBatchAsync`. Stream subscriptions are attached as batch observers, so each batch container (one transaction with the KurrentDB provider) arrives in one call. Both paths share one apply path, `ApplyAsync`, which writes the state once per delivery, with redelivery protection by topic and stream key.
- `HandlerProjectionState<TState>` - projection state that registers a handler per event type with `On<TEvent>` in its constructor instead of overriding `Handle`. Handlers may be synchronous or asynchronous, receive the event and its envelope, and return whether the state changed. Only the exact runtime type is matched; unhandled events are ignored.
- `ProjectionGrain<TGrain, TState, TContext>` / `ProjectionState<TState, TContext>` / `HandlerProjectionState<TState, TContext>` - the same, for states that need dependencies (other grains, services) to apply events. The derived grain builds a `TContext` that wraps them once from its injected services and passes it to the base constructor; the grain hands it to the state with every delivery and handlers receive it as a third argument. The context is not stored on the state, which is replaced whenever it is read or cleared. The context-free classes are these with `NoProjectionContext`. A lookup returns current data, not the data as of the event, so a replay can project a different result; for a repeatable projection, have the event carry the ID and version of anything it looks up.
- `StreamedEventCursor` - Orleans-serializable implementation of `IStreamedEventCursor`.
- `ProjectionWatermarks` - per topic/partition high-water marks used for duplicate detection.
- `StreamedEventSequence` - comparable sequence/sub-sequence position.

Test projection grains with `ProjectionGrainTestHarness` in `Continuum.Streaming.Orleans.TestHarness`, a Given/When/Then harness that also checks the projection is safe under at-least-once delivery (duplicate batches, and replay after the grain reactivates from storage).
