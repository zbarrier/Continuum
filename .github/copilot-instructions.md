# Copilot Instructions

## Project Guidelines
- Do not commit or push without explicit approval; leave changes uncommitted for review. Approval to commit/push applies only to the changes that exist when approval is given. Any work done after that (even within the same request, e.g., "commit and push, then write X") must be left uncommitted for review until the user explicitly approves committing it. Stage all changes (git add -A) when leaving work uncommitted for review, since staged changes are easier for the user to review.
- Never commit or push failing tests without prominently flagging the failure and getting explicit approval first; clearly call out any failing test before a commit.
- Follow the Continuum release strategy: no package releases until all Continuum framework projects are added; then a single coordinated 1.0.0 release of all packages.
- All Continuum framework library projects must be Native AOT compatible (set IsAotCompatible=true and keep builds free of AOT/trim warnings). Orleans-dependent runtime features (e.g., Orleans DeepCopier/AddSerializer used by grains and sessions) are not required to run under Native AOT since Orleans does not support AOT yet; libraries must still be IsAotCompatible with no AOT/trim warnings from Continuum's own code.
- Continuum projects reference each other with ProjectReference. Any Continuum project or test project that declares its own type-mapped types (e.g., [DomainEventType]) must reference Continuum.Core.SourceGenerators as an analyzer (OutputItemType="Analyzer" ReferenceOutputAssembly="false"); otherwise those types are never registered with the type mapper. Projects that do not declare type-mapped types do not need this analyzer reference.
- Continuum has not shipped to production; no backward compatibility with previously stored data or prior behavior is needed — don't add legacy fallbacks.

## Framework Usage
- Continuum type map attributes are used almost only in application code that consumes the framework via NuGet, not in other Continuum projects.
- Use the Orleans [Alias] naming convention in Continuum: "Continuum.<Area>.<TypeName>.V<n>" (e.g., Continuum.KurrentDBStorageException.V1). Omit namespace segments already implied by the type name or by Orleans itself. Abstract classes do not get an [Alias], since Orleans only writes the concrete runtime type's alias; interfaces still do (grain interface aliases define the grain interface identity, and grain method aliases identify methods).
- Orleans [Alias] names in Continuum should not include an "Orleans" segment, since being for Orleans is implied (e.g., "Continuum.KurrentDBStorageException.V1", not "Continuum.Orleans.KurrentDBStorageException.V1").
- Continuum namespaces put the "Orleans" segment right after "Continuum" (e.g., Continuum.Orleans.Events, not Continuum.Events.Orleans), matching project naming like Continuum.Orleans.KurrentDB.
- Add a test project only when the library has behavior that genuinely needs testing; Continuum libraries do not each need their own test project.
- Abstractions projects must not know anything about implementation projects. Tests that need implementation types or internals belong in the implementation's own test project (e.g., Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests), never in the Abstractions test project; InternalsVisibleTo should only grant access to the implementation's own test project.
- Continuum event-sourcing storage providers (KurrentDB, CosmosDB) must always use TypeMappedJsonGrainStorageSerializer with requireStoredType: true; the option defaults to false for all other uses (e.g., grain state storage).
- Prefer real class names (e.g., KurrentDBEventMetadataCodec) with normal namespace using statements over using-aliases like `using MetadataCodec = ...`, and prefer using statements over fully qualified names.

## Exception Handling
- In Continuum, business application grains are responsible for catching exceptions, logging exception-specific details, and returning errors in a Result; inner exception details must not leak to the UI. Internal Continuum library code and tests do not use or test the Result-returning pattern; that is for consuming business apps to implement.

## Multi-Tenancy Design
- Continuum multi-tenancy design (planned, not implemented): use Orleans.Multitenant. Grain keys are "{TenantId}|{Key}". Stream names are "{GrainType}-{TenantId}|{Key}" (non-tenant: "{GrainType}-{Key}"), i.e. GrainType + "-" + Orleans grain key. GrainTypes cannot contain '-' or '|'; keys may contain '-' (GUIDs) but never '|'; tenant IDs must be non-empty and contain no '|'. A '|' after the category means tenant-qualified, so one parser handles both.
- Subscriptions stay tenant-unaware (KurrentDB $ce-{GrainType} spans tenants); Orleans.Multitenant filters enforce tenant isolation. The Continuum KurrentDB stream provider subscribes to the $all stream (not $ce-{GrainType} category subscriptions). Orleans.Multitenant tenant isolation filters are left for consuming apps to add themselves; Continuum only needs to follow a naming scheme compatible with Orleans.Multitenant.
- Per-tenant storage providers set ConnectionName via configureTenantOptions so tenants sharing a database share one keyed KurrentDBClient; dedicated tenants use their own connection name.
- Cross-tenant/cross-source reports consume integration events from EventHub/Kafka/Kinesis, never direct from multiple KurrentDB databases.

## Streaming Cursors and Checkpoints
- Continuum streaming cursors and checkpoints use an explicit SourceConnectionName as the first key segment (SourceConnectionName -> Topic -> Partition -> SequenceNumber -> SubSequenceNumber; SourceConnectionName -> SubscriptionName -> Partition -> position), not hidden in the partition. CheckpointConnectionName only selects where checkpoints are stored. Connection names are permanent logical identifiers that must never be renamed.
- In the Continuum KurrentDB stream provider, persistent subscription events are intentionally acked as they are received (not when consumed); the checkpoint lives on the KurrentDB server side. Do not flag ack-on-receive as a bug.
- Events with an active cursor must never be dropped: a cursor with unread events must keep blocking purge and checkpointing indefinitely. Do not suggest staleness timeouts or similar mechanisms that would purge unread events.
- ProjectionGrain must save state before acknowledging each delivered event or batch; do not buffer or throttle projection state writes by default, since a delivered event may be checkpointed and purged. A buffered/throttled projection may be added later only as an explicit opt-in when the user asks for it.
- KurrentDB persistent subscriptions are intended only for consumers that can tolerate dropped and out-of-order events; ack-on-receive stays. Consumers that need ordering and no drops should use catch-up subscriptions. Do not propose ack-on-purge for persistent subscriptions.
- In KurrentDB, every event has a unique $all commit position; events in the same append/transaction never share a commit position. Do not flag issues that assume shared commit positions.

## Testing Guidelines
- Don't add production hooks or seams just to test narrow race windows or code that's likely to change soon. Prefer tests that need no production changes, and rely on code review for small, easy-to-read logic rather than testing every little thing.
- When reviewing code, say plainly when code is fine as is; do not invent marginal suggestions just to have something to report. Only report findings with real value.

## Alias Versioning
- The Orleans [Alias] ".V<n>" suffix is not a hard rule in Continuum. During reviews, flag aliases that may not need ".V1" (types never persisted or sent polymorphically, e.g., surrogates) rather than changing them automatically. Keep ".V1" where versioning is plausible: persisted state, exceptions, types sent polymorphically, and messages like commands where a later V2 alongside V1 should be easy to identify.