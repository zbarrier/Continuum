# Copilot Instructions

## Project Guidelines
- Do not commit or push without explicit approval; leave changes uncommitted for review. Approval to commit/push applies only to the changes that exist when approval is given. Any work done after that (even within the same request, e.g., "commit and push, then write X") must be left uncommitted for review until the user explicitly approves committing it.
- Follow the Continuum release strategy: no package releases until all Continuum framework projects are added; then a single coordinated 1.0.0 release of all packages.
- All Continuum framework library projects must be Native AOT compatible (set IsAotCompatible=true and keep builds free of AOT/trim warnings). Orleans-dependent runtime features (e.g., Orleans DeepCopier/AddSerializer used by grains and sessions) are not required to run under Native AOT since Orleans does not support AOT yet; libraries must still be IsAotCompatible with no AOT/trim warnings from Continuum's own code.
- Continuum projects reference each other with ProjectReference. Any Continuum project or test project that declares its own type-mapped types (e.g., [DomainEventType]) must reference Continuum.Core.SourceGenerators as an analyzer (OutputItemType="Analyzer" ReferenceOutputAssembly="false"); otherwise those types are never registered with the type mapper. Projects that do not declare type-mapped types do not need this analyzer reference.
- Continuum has not shipped to production; no backward compatibility with previously stored data or prior behavior is needed — don't add legacy fallbacks.

## Framework Usage
- Continuum type map attributes are used almost only in application code that consumes the framework via NuGet, not in other Continuum projects.
- Use the Orleans [Alias] naming convention in Continuum: "Continuum.<Area>.<TypeName>.V<n>" (e.g., Continuum.KurrentDBStorageException.V1). Omit namespace segments already implied by the type name or by Orleans itself. Abstract types do not get an [Alias].
- Orleans [Alias] names in Continuum should not include an "Orleans" segment, since being for Orleans is implied (e.g., "Continuum.KurrentDBStorageException.V1", not "Continuum.Orleans.KurrentDBStorageException.V1").
- Add a test project only when the library has behavior that genuinely needs testing; Continuum libraries do not each need their own test project.
- Abstractions projects must not know anything about implementation projects. Tests that need implementation types or internals belong in the implementation's own test project (e.g., Continuum.Streaming.Orleans.KurrentDB.Tests), never in the Abstractions test project; InternalsVisibleTo should only grant access to the implementation's own test project.
- Continuum event-sourcing storage providers (KurrentDB, CosmosDB) must always use TypeMappedJsonGrainStorageSerializer with requireStoredType: true; the option defaults to false for all other uses (e.g., grain state storage).

## Multi-Tenancy Design
- Continuum multi-tenancy design (planned, not implemented): use Orleans.Multitenant. Grain keys are "{TenantId}|{Key}". Stream names are "{GrainType}-{TenantId}|{Key}" (non-tenant: "{GrainType}-{Key}"), i.e. GrainType + "-" + Orleans grain key. GrainTypes cannot contain '-' or '|'; keys may contain '-' (GUIDs) but never '|'; tenant IDs must be non-empty and contain no '|'. A '|' after the category means tenant-qualified, so one parser handles both.
- Subscriptions stay tenant-unaware (KurrentDB $ce-{GrainType} spans tenants); Orleans.Multitenant filters enforce tenant isolation.
- Per-tenant storage providers set ConnectionName via configureTenantOptions so tenants sharing a database share one keyed KurrentDBClient; dedicated tenants use their own connection name.
- Cross-tenant/cross-source reports consume integration events from EventHub/Kafka/Kinesis, never direct from multiple KurrentDB databases.