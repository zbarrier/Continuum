# Continuum.NewId.Orleans

[Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) serialization surrogate for [Continuum.NewId](https://www.nuget.org/packages/Continuum.NewId).

## Install

```
dotnet add package Continuum.NewId.Orleans
```

## Supported types

| Type | Surrogate |
|---|---|
| `NewId` | `NewIdSurrogate` (serialized as a sequential `Guid`) |

The converter is registered with `[RegisterConverter]`, and Orleans discovers it automatically when this assembly is referenced. Grain interfaces and `[GenerateSerializer]` types can then use `NewId` directly:

```csharp
public interface IOrderGrain : IGrainWithGuidKey
{
	Task<NewId> CreateAsync();
}

[GenerateSerializer]
public sealed class OrderDto
{
	[Id(0)] public NewId Id { get; init; }
}
```
