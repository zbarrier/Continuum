using System.Text.Json;
using System.Text.Json.Serialization;

using Continuum.CSharpFunctionalExtensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// Exercises the ASP.NET Core result extensions under Native AOT. Publish with:
//   dotnet publish tests/CSharpFunctionalExtensions.AspNetCore.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

var services = new ServiceCollection()
    .AddLogging()
    .AddOptions()
    .AddProblemDetails()
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, SmokeJsonContext.Default))
    .BuildServiceProvider();

async Task<(HttpContext Context, string Body)> ExecuteAsync(Microsoft.AspNetCore.Http.IResult httpResult)
{
    var context = new DefaultHttpContext { RequestServices = services, Response = { Body = new MemoryStream() } };
    await httpResult.ExecuteAsync(context);
    context.Response.Body.Position = 0;
    using var reader = new StreamReader(context.Response.Body);
    return (context, await reader.ReadToEndAsync());
}

// Success value serialized with source-generated metadata
var (okContext, okBody) = await ExecuteAsync(Result.Success(new OrderDto("A-1", 3)).ToOkHttpResult());
Check("Ok value", okContext.Response.StatusCode == 200 && okBody.Contains("\"A-1\"", StringComparison.Ordinal));

// Request error -> problem details with code and Retry-After
var (unavailableContext, unavailableBody) = await ExecuteAsync(
    Result.Failure<OrderDto>(RequestErrors.NewUnavailable(TimeSpan.FromSeconds(1.5), "Try later.")).ToOkHttpResult());
using (var json = JsonDocument.Parse(unavailableBody))
{
    Check("Request error problem details",
        unavailableContext.Response.StatusCode == 503
        && unavailableContext.Response.ContentType == "application/problem+json"
        && json.RootElement.GetProperty("detail").GetString() == "Try later."
        && json.RootElement.TryGetProperty("code", out _));
}
Check("Retry-After header", unavailableContext.Response.Headers.RetryAfter.ToString() == "2");

// Validation error -> validation problem details grouped by target
var validation = Result.Combine(Result.NotNullOrEmpty("", "Name"), Result.IsInt32("abc", "Age"));
var (validationContext, validationBody) = await ExecuteAsync(validation.ToNoContentHttpResult());
using (var json = JsonDocument.Parse(validationBody))
{
    var errors = json.RootElement.GetProperty("errors");
    Check("Validation problem details",
        validationContext.Response.StatusCode == (int)validation.Error.HttpStatusCode
        && errors.TryGetProperty("Name", out _)
        && errors.TryGetProperty("Age", out _));
}

// Created with Location
var (createdContext, _) = await ExecuteAsync(Result.Success(new OrderDto("B-2", 1)).ToCreatedHttpResult(order => $"/orders/{order.Number}"));
Check("Created location", createdContext.Response.StatusCode == 201 && createdContext.Response.Headers.Location.ToString() == "/orders/B-2");

// File results
var (fileContext, fileBody) = await ExecuteAsync(Result.Success("hello"u8.ToArray()).ToFileHttpResult("text/plain", "hello.txt"));
Check("File bytes", fileContext.Response.StatusCode == 200 && fileBody == "hello");

var (streamContext, streamBody) = await ExecuteAsync(
    await Result.Success("blob").ToHttpResultAsync(name => Task.FromResult(TypedResults.Stream(new MemoryStream("data"u8.ToArray()), "text/plain", $"{name}.txt"))));
Check("Custom async stream", streamContext.Response.StatusCode == 200 && streamBody == "data");

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

internal sealed record OrderDto(string Number, int Lines);

[JsonSerializable(typeof(OrderDto))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext;
