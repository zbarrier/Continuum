# Copilot Instructions — Get-R-Done Sample

Get-R-Done is a sample application built on the Continuum framework. Continuum source lives in the repository root (`../../src`, referenced via `$(ContinuumSrc)`); the repository-level `.github/copilot-instructions.md` rules also apply.

## Project Guidelines
- Do not commit or push without explicit approval; stage all changes (`git add -A`) and leave them for review.
- The system is designed with Event Modeling (screens/automations → commands → events → read models, organized into slices). The user will supply the event model; implement features slice by slice from it. The `Hello` feature

## Architecture
- Aspire orchestrates everything (`GetRDone.AppHost`): Azurite tables for Orleans clustering, KurrentDB for event storage, Mailpit for local email capture, plus the Identity, Silo, and Web projects.
- `GetRDone.Identity`: Duende IdentityServer with seeded demo users (`client`, `coordinator`, `reviewer`, `vendor`; password `Pass123$`; tenant `demo`).
- `GetRDone.Silo`: Orleans silo host. Orleans Identity **server** setup belongs here (`ManagedCode.Orleans.Identity.Server`, `silo.AddOrleansIdentity()`), which requires `builder.Services.AddAuthorization()`.
- `GetRDone.Web`: Blazor Server (MudBlazor UI; `MainLayout` hosts the MudBlazor providers, app bar, and nav drawer) with cookie + OIDC auth
- Feature-based projects under `src/Features/<Feature>/`: `GetRDone.<Feature>.Abstractions` (grain interfaces, commands, responses, events) and `GetRDone.<Feature>.Grains` (implementations). No generic shared Contracts project.
- Planned: durable in-app notifications via Orleans streams + Blazor Server SignalR, with email through Mailpit.

## Multi-Tenancy
- Uses Orleans.Multitenant. Web calls grains via `ClusterClient.ForTenant(tenantId).GetGrain<T>(key)`; the tenant comes from the `tenant_id` claim.
- Event storage uses `AddMultitenantKurrentDBBasedLogConsistencyProvider` with provider name `EventLog` and connection name `kurrentdb` (see `GetRDoneStorage`). All tenants currently share the default connection.

## Conventions
- Domain events are **classes, not records**. Orleans compares raised events to remove pending ones, and record value equality can match the wrong event. Events have `InitiatedBy` (Guid) and `OccurredAt` (DateTimeOffset), plus `[DomainEventType("GetRDone.<Feature>.<Name>")]`, `[GenerateSerializer]`, and `[Immutable]`.
- Commands and responses may be records.
- `InitiatedBy` comes from the authenticated identity (`OrleansIdentityContext.RequireAuthenticatedUserId()`), never from command input.
- Grains return `Result`/`Result<T>`, catch and log exceptions, and never leak exception details to the UI.

## Required Setup (Known Pitfalls)
- Any project declaring `[DomainEventType]` types **and** the Silo project must reference `Continuum.Core.SourceGenerators` as an analyzer (`OutputItemType="Analyzer" ReferenceOutputAssembly="false"`). Without it in the Silo, events fail at runtime with "A type mapping for the type ... was not found."
- The Silo needs `silo.AddMemoryGrainStorageAsDefault()` so Orleans can activate event-driven grains; event state itself is stored in KurrentDB.
- Orleans version is 10.4.2 (`OrleansVersion`), required by ManagedCode.Orleans.Identity 10.4.6.
- The sample-local `Directory.Build.props` intentionally does not import the repository root packaging rules.

## Local Development
- Run `GetRDone.AppHost` (requires Docker).
- KurrentDB is pinned to port 2113 (`esdb://localhost:2113?tls=false`) so the KurrentDB desktop app can connect. It has no data volume, so data is lost when the AppHost restarts.
- Stop the AppHost before rebuilding; a running Silo locks its output files.

## Status
Hello flow confirmed end to end after pinning Microsoft.Orleans.Streaming to $(OrleansVersion), Web UI moved to MudBlazor.
- Next (infrastructure, independent of the event model):
  - Replace the placeholder `GetRDone.Identity` UI
- Next (domain): the user will create the event model; notifications are designed as part of it. After that, implement the model slice by slice.
