using System.Text;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace Continuum.CSharpFunctionalExtensions.AspNetCore.Tests;

public class ResultFileHttpExtensionsTests
{
    [Fact]
    public async Task Bytes_success_writes_file_with_download_name()
    {
        var httpResult = await Task.FromResult(Result.Success("hello"u8.ToArray())).ToFileHttpResult("text/plain", "hello.txt");

        var (context, body) = await ExecuteAsync(httpResult);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        context.Response.ContentType.Should().Be("text/plain");
        context.Response.Headers.ContentDisposition.ToString().Should().Contain("hello.txt");
        body.Should().Be("hello");
    }

    [Fact]
    public async Task Stream_success_writes_file_and_disposes_stream()
    {
        var stream = new TrackingStream("data"u8.ToArray());

        var (context, body) = await ExecuteAsync(Result.Success<Stream>(stream).ToFileHttpResult("application/octet-stream"));

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        body.Should().Be("data");
        stream.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task File_failure_writes_problem_details()
    {
        var (context, _) = await ExecuteAsync(Result.Failure<byte[]>(RequestErrors.NewNotFound()).ToFileHttpResult());

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public void General_overload_uses_built_result_on_success()
    {
        var httpResult = Result.Success(3).ToHttpResult(value => TypedResults.Text($"v{value}"));

        httpResult.Result.Should().BeOfType<ContentHttpResult>().Which.ResponseContent.Should().Be("v3");
    }

    [Fact]
    public async Task Async_overload_does_not_call_builder_on_failure()
    {
        var called = false;

        var httpResult = await Task.FromResult(Result.Failure<int>(RequestErrors.NewNotFound()))
            .ToHttpResultAsync(_ =>
            {
                called = true;
                return Task.FromResult(TypedResults.NoContent());
            });

        called.Should().BeFalse();
        httpResult.Result.Should().BeOfType<ErrorHttpResult>();
    }

    [Fact]
    public async Task Async_overload_streams_built_result_on_success()
    {
        var httpResult = await Result.Success("blob").ToHttpResultAsync(name =>
            Task.FromResult(TypedResults.Stream(new MemoryStream(Encoding.UTF8.GetBytes(name)), "text/plain", $"{name}.txt")));

        var (context, body) = await ExecuteAsync(httpResult);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        body.Should().Be("blob");
    }

    private static async Task<(HttpContext Context, string Body)> ExecuteAsync(Microsoft.AspNetCore.Http.IResult httpResult)
    {
        var services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services, Response = { Body = new MemoryStream() } };
        await httpResult.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return (context, await reader.ReadToEndAsync());
    }

    private sealed class TrackingStream(byte[] buffer) : MemoryStream(buffer)
    {
        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            Disposed = true;
            return base.DisposeAsync();
        }
    }
}
