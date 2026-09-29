# Continuum.CSharpFunctionalExtensions.Orleans

[Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) serialization surrogates for [Continuum.CSharpFunctionalExtensions](https://www.nuget.org/packages/Continuum.CSharpFunctionalExtensions).

## Install

```
dotnet add package Continuum.CSharpFunctionalExtensions.Orleans
```

## Supported types

| Type | Surrogate |
|---|---|
| `Result` | `ResultSurrogate` |
| `Result<T>` | `ResultSurrogate<T>` |
| `Maybe<T>` | `MaybeSurrogate<T>` |
| `RequestError` | `RequestErrorSurrogate` (includes `Target` and `RetryAfter`) |
| `ValidationError` | `ValidationErrorSurrogate` / `ValidationErrorEntrySurrogate` |
| `ErrorArgument` | `ErrorArgumentSurrogate` (keeps the argument's original type) |

Converters are registered with `[RegisterConverter]`, and Orleans discovers them automatically when this assembly is referenced. Grain interfaces can then return `Result`, `Result<T>` and `Maybe<T>` directly:

```csharp
public interface IOrderGrain : IGrainWithGuidKey
{
	Task<Result<OrderDto>> GetAsync();
}
```

`PriorityCode` is not serialized because it is derived from the gRPC status code.

## Your own value objects

`ValueObject`, `SimpleValueObject<T>` and `EnumValueObject<...>` are abstract base classes, so this package cannot provide surrogates for them. Adding `[GenerateSerializer]` to your own subclass is not enough: Orleans only serializes base-class members when the base class is also marked `[GenerateSerializer]`, and the core library does not depend on Orleans. Register a surrogate for each value object instead:

```csharp
[GenerateSerializer, Immutable]
public struct EmailAddressSurrogate
{
    [Id(0)] public string Value;
}

[RegisterConverter]
public sealed class EmailAddressSurrogateConverter : IConverter<EmailAddress, EmailAddressSurrogate>
{
    public EmailAddress ConvertFromSurrogate(in EmailAddressSurrogate surrogate) => new(surrogate.Value);

    public EmailAddressSurrogate ConvertToSurrogate(in EmailAddress value) => new() { Value = value.Value };
}
```

For an `EnumValueObject`, send only its `Id` and look the member up again when deserializing, so the predefined instance (for example `Color.Red`) comes back instead of a copy:

```csharp
[GenerateSerializer, Immutable]
public struct ColorSurrogate
{
    [Id(0)] public string Id;
}

[RegisterConverter]
public sealed class ColorSurrogateConverter : IConverter<Color, ColorSurrogate>
{
    public Color ConvertFromSurrogate(in ColorSurrogate surrogate) =>
        Color.FromId(surrogate.Id).GetValueOrThrow();

    public ColorSurrogate ConvertToSurrogate(in Color value) => new() { Id = value.Id };
}
```

## Mapping exceptions to errors

`GlobalGrainErrorHandler.Default` logs an exception and turns it into a `RequestError`: `DeadlineExceeded` for `TimeoutException`, and `Unknown` for anything else.
