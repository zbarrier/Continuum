using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace Continuum.CSharpFunctionalExtensions.AspNetCore.Tests;

public class ResultHttpExtensionsTests
{
    [Fact]
    public async Task Ok_with_success_writes_value()
    {
        var httpResult = Result.Success(42).ToOkHttpResult();

        httpResult.Result.Should().BeOfType<Ok<int>>().Which.Value.Should().Be(42);
        var (status, body) = await ExecuteAsync(httpResult);
        status.Should().Be(StatusCodes.Status200OK);
        body.Should().Be("42");
    }

    [Fact]
    public async Task Ok_with_failure_writes_problem_details_not_the_result()
    {
        var httpResult = Result.Failure<int>(RequestErrors.NewNotFound("Thing {0} not found.", 7)).ToOkHttpResult();

        var (status, body) = await ExecuteAsync(httpResult);

        status.Should().Be(StatusCodes.Status404NotFound);
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("status").GetInt32().Should().Be(404);
        json.RootElement.GetProperty("detail").GetString().Should().Be("Thing 7 not found.");
        json.RootElement.TryGetProperty("code", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("isSuccess", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Failure_with_retry_after_sets_header_rounded_up()
    {
        var httpResult = Result.Failure(RequestErrors.NewUnavailable(TimeSpan.FromSeconds(1.2))).ToNoContentHttpResult();

        var context = CreateContext();
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        context.Response.Headers.RetryAfter.ToString().Should().Be("2");
    }

    [Fact]
    public async Task Validation_failure_writes_errors_grouped_by_target()
    {
        var error = new ValidationError(
        [
            new ValidationErrorEntry(ValidationSeverity.Error, "Name", "required", "Name is required.", null),
            new ValidationErrorEntry(ValidationSeverity.Error, "Name", "length", "Name is too long.", null)
        ]);

        var (status, body) = await ExecuteAsync(Result.Failure(error).ToOkHttpResult());

        status.Should().Be((int)error.HttpStatusCode);
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("errors").GetProperty("Name").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Customize_problem_details_is_applied()
    {
        var httpResult = Result.Failure(RequestErrors.NewNotFound()).ToOkHttpResult(problem => problem.Title = "Custom");

        var (_, body) = await ExecuteAsync(httpResult);

        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("title").GetString().Should().Be("Custom");
    }

    [Fact]
    public async Task Created_with_uri_selector_sets_location()
    {
        var httpResult = await Task.FromResult(Result.Success(5)).ToCreatedHttpResult(id => $"/things/{id}");

        var context = CreateContext();
        await httpResult.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        context.Response.Headers.Location.ToString().Should().Be("/things/5");
    }

    [Fact]
    public async Task Accepted_with_ValueTask_success_returns_202()
    {
        var httpResult = await new ValueTask<Result>(Result.Success()).ToAcceptedHttpResult("/status/1");

        httpResult.Result.Should().BeOfType<Accepted>().Which.Location.Should().Be("/status/1");
    }

    private static DefaultHttpContext CreateContext()
    {
        var services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
        return new DefaultHttpContext { RequestServices = services, Response = { Body = new MemoryStream() } };
    }

    private static async Task<(int Status, string Body)> ExecuteAsync(Microsoft.AspNetCore.Http.IResult httpResult)
    {
        var context = CreateContext();
        await httpResult.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return (context.Response.StatusCode, await reader.ReadToEndAsync());
    }
}
