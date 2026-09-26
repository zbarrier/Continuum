using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
  public class Ensure_ValueTask_Tests : TestBase
  {
    [Fact]
    public async Task Ensure_ValueTask_with_successInput_and_successPredicate()
    {
      var initialResult = Result.Success("Initial message").AsCompletedValueTask();

      var result = await initialResult.Ensure(() => Result.Success("Success message").AsCompletedValueTask());

      result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
      result.Value.Should().Be("Initial message");
    }

    [Fact]
    public async Task Ensure_ValueTask_with_successInput_and_failurePredicate()
    {
      var initialResult = Result.Success("Initial Result").AsCompletedValueTask();

      var result = await initialResult.Ensure(() => Result.Failure(ErrorMessage).AsCompletedValueTask());

      result.IsSuccess.Should().BeFalse("Predicate is failure result");
      result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_ValueTask_with_failureInput_and_successPredicate()
    {
      var initialResult = Result.Failure(ErrorMessage).AsCompletedValueTask();

      var result = await initialResult.Ensure(() => Result.Success("Success message").AsCompletedValueTask());

      result.IsSuccess.Should().BeFalse("Initial result is failure result");
      result.Error.Should().Be(ErrorMessage);
    }

    [Fact]
    public async Task Ensure_ValueTask_with_failureInput_and_failurePredicate()
    {
      var initialResult = Result.Failure(ErrorMessage).AsCompletedValueTask();

      var result = await initialResult.Ensure(() => Result.Failure(ErrorMessage2).AsCompletedValueTask());

      result.IsSuccess.Should().BeFalse("Initial result is failure result");
      result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_ValueTask_with_successInput_and_parameterisedFailurePredicate()
    {
      var initialResult = Result.Success("Initial Success message").AsCompletedValueTask();

      var result = await initialResult.Ensure(_ => Result.Failure(ErrorMessage2).AsCompletedValueTask());

      result.IsSuccess.Should().BeFalse("Predicate is failure result");
      result.Error.Should().Be(ErrorMessage2);
    }
    
    [Fact]
    public async Task Ensure_ValueTask_with_successInput_and_parameterisedSuccessPredicate()
    {
      var initialResult = Result.Success("Initial Success message").AsCompletedValueTask();

      var result = await initialResult.Ensure(_ => Result.Success("Success Message").AsCompletedValueTask());

      result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");;
      result.Value.Should().Be("Initial Success message");
    }
    
    [Fact]
    public async Task Ensure_ValueTask_with_failureInput_and_parameterisedSuccessPredicate()
    {
      var initialResult = Result.Failure<string>(ErrorMessage).AsCompletedValueTask();

      var result = await initialResult.Ensure(_ => Result.Success("Success Message").AsCompletedValueTask());

      result.IsSuccess.Should().BeFalse("Initial result is failure result");;
      result.Error.Should().Be(ErrorMessage);
    }
    
    [Fact]
    public async Task Ensure_ValueTask_with_failureInput_and_parameterisedFailurePredicate()
    {
      var initialResult = Result.Failure<string>(ErrorMessage).AsCompletedValueTask();

      var result = await initialResult.Ensure(_ => Result.Failure(ErrorMessage2).AsCompletedValueTask());

      result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result");;
      result.Error.Should().Be(ErrorMessage);
    }
  }
}