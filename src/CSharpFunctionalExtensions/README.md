# Continuum.CSharpFunctionalExtensions

Functional extensions for C#: `Result`, `Maybe`, `ValueObject`, and a composable,
localizable validation and error model.

## Install

```shell
dotnet add package Continuum.CSharpFunctionalExtensions
```

## Features

- **Result / Result&lt;T&gt;** – railway-oriented success/failure flow with `Map`, `Bind`, `Tap`, `Ensure`, `Match`, and async variants.
- **Maybe&lt;T&gt;** – explicit optional values.
- **ValueObject** – structural equality base types, including source-generated `EnumValueObject`.
- **Errors** – `RequestError` (HTTP + gRPC status aware, with `Target` and `RetryAfter`) and `ValidationError` with stable machine-readable `Code` values.
- **Validators** – FluentValidation-style checks (`NotEmpty`, `MaxLength`, `LengthBetween`, `CountBetween`, `IsEmail`, `CreditCard`, ...) that return `Result`. US-specific validators ship separately in `Continuum.CSharpFunctionalExtensions.Validators.US`.
- **Localization** – messages are rendered by error code at display time, with built-in translations for 58 languages, custom overrides, and a display-name hook.
- **System.Text.Json** support for results and errors, including Native AOT.
- **Analyzers** – compile-time checks for common mistakes (CFE001–CFE004).

## Quick example

```csharp
Result<string> email = Result.NotEmpty(input, "Email")
	.IsEmail("Email")
	.MaxLength(254, "Email");

if (email.IsFailure)
{
	// Localized rendering
	string message = email.Error.GetFormattedMessage(CultureInfo.GetCultureInfo("fr"));
}
```

## Differences from CSharpFunctionalExtensions

This library started from [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
and keeps its `Result`, `Maybe`, and `ValueObject` programming model. The main differences are below.

### Errors are typed objects, not strings

`Result` and `Result<T>` always carry an `Error`. There is no `Result.Failure(string)` overload and no
`Result<T, E>` variant. Every error has a machine-readable `Code`, an `HttpStatusCode`, and a `GrpcStatusCode`.

```csharp
// Original
Result<Order> order = Result.Failure<Order>("Order not found");

// Continuum
Result<Order> order = Result.Failure<Order>(RequestErrors.NewNotFound());
Result<Order> custom = Result.Failure<Order>(RequestErrors.NewNotFound("Order {0} was not found.", orderId));

order.Error.Code;             // stable code, used as the localization key
order.Error.HttpStatusCode;   // HttpStatusCode.NotFound
order.Error.GrpcStatusCode;   // StatusCode.NotFound
```

`RequestErrors` provides a factory per common gRPC status:

| Factory | HTTP | gRPC | Accepts `RetryAfter` |
|---|---|---|---|
| `NewInvalidArg` | 422 | `InvalidArgument` | |
| `NewNotFound` | 404 | `NotFound` | |
| `NewAlreadyExists` | 409 | `AlreadyExists` | |
| `NewFailedPrecondition` | 400 | `FailedPrecondition` | |
| `NewAborted` | 409 | `Aborted` | ✓ |
| `NewResourceExhausted` | 429 | `ResourceExhausted` | ✓ |
| `NewUnavailable` | 503 | `Unavailable` | ✓ |
| `NewDeadlineExceeded` | 504 | `DeadlineExceeded` | ✓ |
| `NewNotImplemented` | 501 | `Unimplemented` | |
| `NewUnknown` | 500 | `Unknown` | |

For any other pair, use `RequestErrors.New(httpStatusCode, grpcStatusCode, ...)` or the `RequestError` constructor.
HTTP codes are more granular than gRPC codes, so one gRPC code can map to several HTTP codes
(for example `404 Not Found` and `410 Gone` both map to `NotFound`).

### Target and RetryAfter

`RequestError.Target` names the resource, parameter, or field the error is about. `RequestError.RetryAfter` is how
long the caller should wait before retrying; `null` means no hint, not "do not retry".

```csharp
var notFound = RequestErrors.NewNotFound("Order {0} was not found.", orderId).WithTarget("orderId");

var throttled = RequestErrors.NewResourceExhausted(TimeSpan.FromSeconds(30));
if (throttled.RetryAfter is { } delay)
{
	await Task.Delay(delay);
}

var full = new RequestError(
	HttpStatusCode.ServiceUnavailable, StatusCode.Unavailable, "inventory.offline",
	format: null, arguments: null, target: "inventory", retryAfter: TimeSpan.FromSeconds(10));
```

`RetryAfter` is serialized as whole seconds, matching the HTTP `Retry-After` header.

### Validation and combining errors

Validators return `ValidationError`, which holds one or more `ValidationErrorEntry` values. Each entry has a
`Target` (the field that failed), a `Code`, and a `Severity`. `Result.Combine` merges all validation entries into
one `ValidationError`; for other errors it keeps the most severe one, ranked by gRPC status (`PriorityCode`).

```csharp
Result combined = Result.Combine(
	Result.NotNullOrEmpty(name, "Name"),
	Result.IsInt32(age, "Age"));

if (combined.Error is ValidationError validation)
{
	foreach (var entry in validation.Entries)
	{
		Console.WriteLine($"{entry.Target}: {entry.GetFormattedMessage()}");
	}
}
```

### Typed error arguments

Message arguments are `ErrorArgument` values rather than `object`. Common types (strings, numbers, dates, `Guid`,
`TimeSpan`, `BigInteger`, ...) convert implicitly and keep their type and format specifiers (such as `{0:N2}`)
through JSON round-trips.

### Localization by code

Messages are resolved from the error `Code` when displayed, not when the error is created. This means a
serialized error can be rendered in the reader's language.

```csharp
var localizer = new ErrorMessageLocalizer();
localizer.AddTranslation("fr", ValidationErrorCodes.NotEmpty, "'{0}' est requis.");
localizer.DisplayNameResolver = (property, culture) => property == "Name" ? "Nom" : null;

ErrorLocalization.Default = localizer;   // or pass it to GetFormattedMessage
```

Libraries that define their own codes register an English default with `ErrorLocalization.RegisterDefaultTemplate`.

### EnumValueObject uses a source generator

`EnumValueObject` members are discovered at compile time rather than through reflection. Enumeration classes must
be `partial` (CFE001 reports an error otherwise).

```csharp
public sealed partial class Color : EnumValueObject<Color>
{
	public static readonly Color Red = new("red");
	public static readonly Color Blue = new("blue");

	private Color(string id) : base(id) { }
}

Maybe<Color> red = Color.FromId("red");
IReadOnlyCollection<Color> all = Color.All;
```

### Country-specific validators are a separate package

US validators (phone number, zip code, state abbreviation, tax ID) live in
`Continuum.CSharpFunctionalExtensions.Validators.US`, keeping the core package culture-neutral.

## JSON serialization

Results serialize as `IsSuccess`, `Value` (for `Result<T>`), `ErrorType`, and `Error`. `ErrorType` is a string
discriminator (for example `"RequestError"` or `"ValidationError"`); errors include a localized `Message` for
convenience, which is ignored when reading.

For normal (JIT) applications, register everything at once:

```csharp
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddCSharpFunctionalExtensionsConverters();

// Or read directly from an HTTP response using the shared options
Result<Order> order = await response.ReadResultAsync<Order>();
```

### Custom error types

Derive from `ErrorJsonConverter<TError>` and register it once at startup:

```csharp
ErrorJsonTypeRegistry.Register(new PaymentErrorJsonConverter());   // discriminator e.g. "PaymentError"
```

`ErrorJson` provides helpers (`ReadArguments`, `WriteArguments`, property-name handling) for writing converters.

## Native AOT

The core package is marked `IsAotCompatible` and is trim- and AOT-safe, with two exceptions that are annotated
with `[RequiresDynamicCode]`:

- `AddCSharpFunctionalExtensionsConverters()` and `CSharpFunctionalExtensionsJsonSerializerOptions.Options`
  (they build `Result<T>` converters at runtime).
- The `ReadResultAsync` overloads that do not take options or type info.

For Native AOT, list your `Result` types on a source-generated `JsonSerializerContext`. The package's source
generator adds an `AddResultConverters` method to the context that registers exactly those converters:

```csharp
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<Order>))]
[JsonSerializable(typeof(Order))]
internal partial class AppJsonContext : JsonSerializerContext;

var options = AppJsonContext.AddResultConverters(
	new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = AppJsonContext.Default });

var orderInfo = (JsonTypeInfo<Result<Order>>)options.GetTypeInfo(typeof(Result<Order>));

string json = JsonSerializer.Serialize(Result.Success(order), orderInfo);
Result<Order> read = await response.ReadResultAsync(orderInfo);
Result plain = await response.ReadResultAsync(options);
```

To register converters by hand instead, use `options.AddResultConverter()` and `options.AddResultConverter<T>()`.

A library that exposes its own context should make it `public` so the application can call its
`AddResultConverters` and combine resolvers with `JsonTypeInfoResolver.Combine`.

## Analyzers

The package includes these diagnostics:

| Rule | Severity | Description |
|---|---|---|
| CFE001 | Error | An `EnumValueObject` type (or a containing type) is not `partial`, so its members cannot be generated. |
| CFE002 | Warning | A `RequestError` uses an unusual HTTP/gRPC status pair, such as `200 OK` with `NotFound`. |
| CFE003 | Warning | `RetryAfter` is set on a status that is not retryable. Only `Aborted`, `ResourceExhausted`, `Unavailable`, and `DeadlineExceeded` are retryable. |
| CFE004 | Warning | A non-public `JsonSerializerContext` lists `Result` types but its generated `AddResultConverters` is never called, so serialization would fail under Native AOT. |

CFE002 and CFE003 only check constant status codes. Suppress them with `#pragma warning disable` or an
`.editorconfig` entry when an unusual combination is intentional.

## License

Apache License 2.0. Portions are adapted from
[CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) (MIT) and
[FluentValidation](https://github.com/FluentValidation/FluentValidation) (Apache 2.0).
See the `NOTICE` file included in this package for details.
