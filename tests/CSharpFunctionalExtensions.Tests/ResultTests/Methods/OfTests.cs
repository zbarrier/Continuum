using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods;

public class OfTests
{
    [Fact]
    public void Of_can_create_ResultT_from_value()
    {
        string value = "value";

        Result<string> result = Result.Of(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Of_can_create_ResultT_from_func()
    {
        string value = "value";
        Func<string> func = () => value;

        Result<string> result = Result.Of(func);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public async Task Of_can_create_ResultT_from_valueTask()
    {
        string value = "value";
        Task<string> valueTask = Task.FromResult(value);

        Result<string> result = await Result.Of(valueTask);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public async Task Of_can_create_ResultT_from_valueTaskFunc()
    {
        string value = "value";
        Task<string> valueTask = Task.FromResult(value);
        Func<Task<string>> valueTaskFunc = () => valueTask;

        Result<string> result = await Result.Of(valueTaskFunc);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }
}
