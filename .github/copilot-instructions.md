# Copilot Instructions

## Project Guidelines
- Do not commit or push without explicit approval; leave changes uncommitted for review. Approval to commit/push applies only to the changes that exist when approval is given. Any work done after that (even within the same request, e.g., "commit and push, then write X") must be left uncommitted for review until the user explicitly approves committing it. Stage all changes (git add -A) when leaving work uncommitted for review, since staged changes are easier for the user to review.
- Never commit or push failing tests without prominently flagging the failure and getting explicit approval first; clearly call out any failing test before a commit.
- Follow the Continuum release strategy: no package releases until all Continuum framework projects are added; then a single coordinated pre-release of all packages (0.1.0).
- All Continuum framework library projects must be Native AOT compatible (set IsAotCompatible=true and keep builds free of AOT/trim warnings). Orleans-dependent runtime features (e.g., Orleans DeepCopier/AddSerializer used by grains and sessions) are not required to run under Native AOT since Orleans does not support AOT yet; libraries must still be IsAotCompatible with no AOT/trim warnings from Continuum's own code.
- Continuum projects reference each other with ProjectReference. Any Continuum project or test project that declares its own type-mapped types (e.g., [DomainEventType]) must reference Continuum.Core.SourceGenerators as an analyzer (OutputItemType="Analyzer" ReferenceOutputAssembly="false"); otherwise those types are never registered with the type mapper. Projects that do not declare type-mapped types do not need this analyzer reference.
- Continuum has not shipped to production; no backward compatibility with previously stored data or prior behavior is needed — don't add legacy fallbacks.
- Continuum is built primarily for the owner's own use (open source but not dumbed down): prefer powerful, flexible features that trust developers to use them wisely and document the costs/risks, rather than imposing restrictive caps or safeguards.

## Framework Usage
- Continuum type map attributes are used almost only in application code that consumes the framework via NuGet, not in other Continuum projects.
- Use the Orleans [Alias] naming convention in Continuum: "Continuum.<Area>.<TypeName>.V<n>" (e.g., Continuum.KurrentDBStorageException.V1). Omit namespace segments already implied by the type name or by Orleans itself. Abstract types do not get an [Alias].
- Orleans [Alias] names in Continuum should not include an "Orleans" segment, since being for Orleans is implied (e.g., "Continuum.KurrentDBStorageException.V1", not "Continuum.Orleans.KurrentDBStorageException.V1").
- Continuum namespaces put the "Orleans" segment right after "Continuum" (e.g., Continuum.Orleans.Events, not Continuum.Events.Orleans), matching project naming like Continuum.Orleans.KurrentDB.
- Add a test project only when the library has behavior that genuinely needs testing; Continuum libraries do not each need their own test project.
- Abstractions projects must not know anything about implementation projects. Tests that need implementation types or internals belong in the implementation's own test project (e.g., Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests), never in the Abstractions test project; InternalsVisibleTo should only grant access to the implementation's own test project.
- Continuum event-sourcing storage providers (KurrentDB, CosmosDB) must always use TypeMappedJsonGrainStorageSerializer with requireStoredType: true; the option defaults to false for all other uses (e.g., grain state storage).
- Prefer real class names (e.g., KurrentDBEventMetadataCodec) with normal namespace using statements over using-aliases like `using MetadataCodec = ...`, and prefer using statements over fully qualified names.

## Exception Handling
- In Continuum, business application grains are responsible for catching exceptions, logging exception-specific details, and returning errors in a Result; inner exception details must not leak to the UI. Internal Continuum library code and tests do not use or test the Result-returning pattern; that is for consuming business apps to implement.

## Multi-Tenancy
- Continuum multi-tenancy (implemented) is compatible with Orleans.Multitenant; Continuum does not reference it (not AOT compatible), and TenantKey reproduces its key format. Grain keys are "{TenantId}|{Key}". Stream names are "{GrainType}-{TenantId}|{Key}" (non-tenant: "{GrainType}-{Key}"), i.e. GrainType + "-" + Orleans grain key. GrainTypes cannot contain '-' or '|'; keys may contain '-' (GUIDs) but never '|'; tenant IDs must be non-empty and contain no '|'. A '|' after the category means tenant-qualified, so one parser handles both.
- Subscriptions stay tenant-unaware (KurrentDB $ce-{GrainType} spans tenants); Orleans.Multitenant filters enforce tenant isolation (apps opt in to multi-tenancy by referencing the Orleans.Multitenant NuGet; Continuum works with it without referencing it). The Continuum KurrentDB stream provider subscribes to the $all stream (not $ce-{GrainType} category subscriptions). KurrentDB catch-up subscriptions in Continuum (Orleans stream provider and self-hosted) must subscribe to either $all (optionally prefix-filtered) or a single stream such as $ce-{Category} or a custom projection stream, never both at the same time. Continuum self-hosted KurrentDB catch-up subscriptions default StartFromNow to false (their main value over the Orleans provider is batching and catching up on history); the Orleans stream provider defaults StartFromNow to true.
- Per-tenant storage providers set ConnectionName via configureTenantOptions so tenants sharing a database share one keyed KurrentDBClient; dedicated tenants use their own connection name.
- Cross-tenant/cross-source reports consume integration events from EventHub/Kafka/Kinesis, never direct from multiple KurrentDB databases.
- Continuum multi-stream transactions cannot span tenants; TenantLogConsistentStorage rejects multi-stream appends whose grains belong to different tenants. Transaction abstractions must be store-agnostic (no KurrentDB-specific types/exceptions exposed to grains).

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

## Versioning for Orleans Aliases
- Continuum Orleans [Alias] version suffixes (e.g., .V1) are only used on types that can be persisted. Orleans surrogate types (e.g., ResultSurrogate, MaybeSurrogate, RequestErrorSurrogate) for message-only types do not get a version suffix, because surrogate aliases aren't meaningfully written to the wire and these types are never persisted.

## Domain Events
- Continuum's optional domain event base types (`IDomainEvent`, `DomainEvent`, `StateMachineDomainEvent<TCommand>`) carry `InitiatedBy` (Guid) and `OccurredAt` (DateTimeOffset). Events are not required to derive from them; they are for apps that use an identity provider (most apps). `InitiatedBy` comes from the validated Orleans.Identity identity in RequestContext (consuming apps reference Orleans.Identity; Continuum does not), never from command input; in-silo automations run as dedicated system users stored in the identity store, and the original user is traced through the correlation ID.

## Helper Overloads
- In Continuum, only add helper overload variations that are actually needed. BindWithStreamTransaction (EventDrivenGrain and EventDrivenStateMachineGrain) has Result and Task<Result> sources with Action, Func<..., Result> and Func<..., Task<Result>> callbacks; do not add ValueTask variants.

## Value Object Creation
- For value objects that wrap a non-string type which is commonly parsed from a string (e.g., Guid), provide Create overloads for both the raw type (with its own validation, e.g., NotEmpty for Guid) and string (parse + validate, e.g., IsGuidAndNotEmpty), each with propertyName and PropertyPath/member variants.

## GetRDone Project Structure
- For GetRDone/Orleans apps: domains live under a Features folder, each with two projects GetRDone.[Domain].Abstractions (grain interfaces, commands, responses, events) and GetRDone.[Domain].Grains (grains and slice logic). Clients reference .Abstractions; the Silo references .Grains. Project names carry the GetRDone prefix; no Contracts project — shared domain concepts go in GetRDone.SharedKernel, added only when needed and limited (via its README) to domain concepts shared by two or more features. Display name is "Get-R-Done".

## KurrentDB Subscriptions
- Continuum KurrentDB subscriptions should fan out per connection: one subscription registered in startup code automatically runs once on each KurrentDB connection/instance (e.g., 1 shared + 2 dedicated = 3 connections), with each connection discovered at startup (likely via DI keyed KurrentDBClient registrations). This is not implemented yet in the self-hosted catch-up subscriptions or stream provider.
- Planned (not implemented): proper OpenTelemetry tracing for the self-hosted catch-up subscriptions. A batch that holds a single transaction continues that transaction's trace; a batch spanning multiple transactions starts its own activity with span links to each transaction's trace context (traceId/spanId from event metadata).
