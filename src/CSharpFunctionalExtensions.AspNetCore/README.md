# Continuum.CSharpFunctionalExtensions.AspNetCore

ASP.NET Core integration for [Continuum.CSharpFunctionalExtensions](https://www.nuget.org/packages/Continuum.CSharpFunctionalExtensions). Turns `Result` and `Result<T>` into typed `IResult` responses for minimal APIs and controllers. Failures become [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) problem details.

## Install

```
dotnet add package Continuum.CSharpFunctionalExtensions.AspNetCore
```

## Usage

Every method returns `Results<TSuccess, ErrorHttpResult>`, so OpenAPI picks up the success type without extra attributes. Each works on `Result`/`Result<T>` and on `Task<...>`/`ValueTask<...>` of them.

```csharp
app.MapGet("/orders/{id}", (Guid id, IGrainFactory grains) =>
	grains.GetGrain<IOrderGrain>(id).GetAsync().ToOkHttpResult());

app.MapPost("/orders", (CreateOrder command, IOrderService orders) =>
	orders.CreateAsync(command).ToCreatedHttpResult(order => $"/orders/{order.Id}"));
```

Controllers return the same results:

```csharp
[HttpDelete("{id}")]
public Task<Results<NoContent, ErrorHttpResult>> Delete(Guid id) =>
	_grains.GetGrain<IOrderGrain>(id).CancelAsync().ToNoContentHttpResult();
```

| Method | Success response |
|---|---|
| `ToOkHttpResult` | `200 OK`, with the value for `Result<T>` |
| `ToNoContentHttpResult` | `204 No Content` (a value is discarded) |
| `ToCreatedHttpResult` | `201 Created` with `Location` (fixed URI or built from the value) |
| `ToCreatedAtRouteHttpResult` | `201 Created` with `Location` from a named route |
| `ToAcceptedHttpResult` | `202 Accepted` with optional `Location` |
| `ToAcceptedAtRouteHttpResult` | `202 Accepted` with `Location` from a named route |
| `ToFileHttpResult` | File from `Result<byte[]>` or `Result<Stream>` (the stream is disposed after writing) |
| `ToHttpResult` / `ToHttpResultAsync` | Any `IResult` built from the value |
| `Error.ToHttpResult` | Problem details for an error |

## Errors

| Error | Response |
|---|---|
| `RequestError` | `ProblemDetails` with the error's status, localized `detail`, `code`, and `target` when set. `RetryAfter` sets the `Retry-After` header (whole seconds, rounded up). |
| `ValidationError` | `HttpValidationProblemDetails` with localized messages in `errors`, grouped by target, plus their codes. |
| Any other `Error` | Generic `500` with no internal details. |

Every response includes `traceId`. Messages are localized with the registered `IErrorMessageLocalizer` and `CultureInfo.CurrentUICulture` (for example, set by `UseRequestLocalization`).

### Configuration

```csharp
builder.Services.Configure<ResultProblemDetailsOptions>(options =>
{
	options.Mappings[StatusCodes.Status404NotFound] = new ProblemDetailsMapping("Not Found", "https://example.com/problems/not-found");
	options.UnknownErrorDetail = "Something went wrong.";
});
```

Change one response with the optional `customizeProblemDetails` argument:

```csharp
orders.GetAsync(id).ToOkHttpResult(problem => problem.Extensions["orderId"] = id);
```

## File downloads through a grain

Orleans can't serialize a `Stream`, and sending large files through a grain loads silo memory. Let the grain authorize and return file details, then stream from storage in the endpoint:

```csharp
app.MapGet("/orders/{orderId}/files/{fileId}", (Guid orderId, Guid fileId, IGrainFactory grains, BlobContainerClient container) =>
	grains.GetGrain<IOrderGrain>(orderId)
		.AuthorizeFileDownload(fileId) // Task<Result<OrderFileDownload>>
		.ToHttpResultAsync(async file =>
		{
			var stream = await container.GetBlobClient(file.BlobName).OpenReadAsync();
			return TypedResults.Stream(stream, file.ContentType, file.FileName, file.LastModified, enableRangeProcessing: true);
		}));
```

The builder isn't called on failure, so nothing is opened when access is denied. Small generated files (such as a PDF rendered by the grain) can be returned as `Result<byte[]>` and passed to `ToFileHttpResult`.

## Native AOT

so problem details have source-generated JSON metadata.
