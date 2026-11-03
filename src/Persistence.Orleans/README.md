# Continuum.Persistence.Orleans

[Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) foundation for Continuum grain state persistence providers, built on [Continuum.Persistence](https://www.nuget.org/packages/Continuum.Persistence). It contains shared types used by grain storage providers.

## TenantGrainStorage

`TenantGrainStorage` is an `IGrainStorage` that routes each grain to the storage for its tenant. Providers use it to support multi-tenancy; applications do not normally use it directly.

- The tenant is read from the grain key, `{TenantId}|{Key}`. Grains without a tenant use the provider's default storage.
- Each tenant's storage is created on first use and reused afterwards. Storages never own their clients, so tenants that share a connection share one client.
- If creating a tenant's storage fails, the caller gets the exception, the failure is logged, and the next call retries.

## Install

```
dotnet add package Continuum.Persistence.Orleans
```

For grain state storage, use a provider such as [Continuum.Persistence.Orleans.KurrentDB](https://www.nuget.org/packages/Continuum.Persistence.Orleans.KurrentDB).
