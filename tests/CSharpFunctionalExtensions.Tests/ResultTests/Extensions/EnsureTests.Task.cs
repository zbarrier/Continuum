using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
  public class Ensure_Task_Tests : TestBase
  {
    [Fact]
    public async Task Ensure_Task_with_successInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial message"));

        var result = await initialResult.Ensure(() => Task.FromResult(Result.Success("Success message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
        result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_Task_with_successInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Result"));

        var result = await initialResult.Ensure(() => Task.FromResult(Result.Failure(ErrorMessage)));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_Task_with_failureInput_and_successPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure(ErrorMessage));

        var result = await initialResult.Ensure(() => Task.FromResult(Result.Success("Success message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(ErrorMessage);
    }

    [Fact]
    public async Task Ensure_Task_with_failureInput_and_failurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure(ErrorMessage));

        var result = await initialResult.Ensure(() => Task.FromResult(Result.Failure(ErrorMessage2)));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");
        result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_Task_with_successInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Success message"));

        var result = await initialResult.Ensure(_ => Task.FromResult(Result.Failure(ErrorMessage)));

        result.IsSuccess.Should().BeFalse("Predicate is failure result");
        result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_Task_with_successInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Success("Initial Success message"));

        var result = await initialResult.Ensure(_ => Task.FromResult(Result.Success("Success Message")));

        result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");;
        result.Value.Should().Be("Initial Success message");
    }
    
    [Fact]
    public async Task Ensure_Task_with_failureInput_and_parameterisedSuccessPredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(ErrorMessage));

        var result = await initialResult.Ensure(_ => Task.FromResult(Result.Success("Success Message")));

        result.IsSuccess.Should().BeFalse("Initial result is failure result");;
        result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_Task_with_failureInput_and_parameterisedFailurePredicate()
    {
        var initialResult = Task.FromResult(Result.Failure<string>(ErrorMessage));

        var result = await initialResult.Ensure(_ => Task.FromResult(Result.Failure(ErrorMessage2)));

        result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result");;
        result.Error.Should().Be(ErrorMessage);
    }
  }
}