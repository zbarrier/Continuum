using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class EnsureTests_Task_Left : TestBase
    {
        [Fact]
        public async Task Ensure_Task_Left_with_errorPredicate_does_not_throw_when_given_result_failure()
        {
            var result = Task.FromResult(Result.Failure<string>(ErrorMessage));
            Func<Task> ensure = () => result.Ensure(
                x => x != "", 
                x => ErrorMessage2);
            
            await ensure.Should().NotThrowAsync<Exception>("passing in a Result.Failure is a valid use case");
        }
        
        [Fact]
        public async Task Ensure_Task_Left_with_errorPredicate_initial_result_has_failure_state()
        {
            var tResult = Task.FromResult(Result.Failure<string>(ErrorMessage));

            var result = await tResult.Ensure(x => x != "", 
                x => ErrorMessage2);
            
            result.IsSuccess.Should().BeFalse("Input Result.Failure should be returned");
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Ensure_Task_Left_with_errorPredicate_predicate_passes()
        {
            var tResult = Task.FromResult(Result.Success("initial ok"));

            var result = await tResult.Ensure(x => x != "", 
                x => ErrorMessage2);
            
            result.IsSuccess.Should().BeTrue("Input Result passes predicate condition");
            result.Value.Should().Be("initial ok");
        }

        [Fact]
        public async Task Ensure_Task_Left_with_errorPredicate_does_not_execute_error_predicate_when_predicate_passes()
        {
            var tResult = Task.FromResult(Result.Success<int?>(null));

            Result<int?> result = await tResult.Ensure(value => !value.HasValue, 
                value => RequestErrors.NewAlreadyExists("should be null but found {0}", value.Value));

            result.Should().Be((await tResult));
        }

        [Fact]
        public async Task Ensure_Task_Left_with_errorPredicate_using_errorPredicate()
        {
            var tResult = Task.FromResult(Result.Success(""));

            var result = await tResult.Ensure(x => x != "", 
                x => ErrorMessage2);
            
            result.IsSuccess.Should().BeFalse("Input Result fails predicate condition");
            result.Error.Should().Be(ErrorMessage2);
        }

        [Fact]
        public async Task Ensure_Task_Left_with_successInput_and_successPredicate()
        {
          var initialResult = Task.FromResult(Result.Success("Initial message"));

          var result = await initialResult.Ensure(() => Result.Success("Success message"));

          result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
          result.Value.Should().Be("Initial message");
        }

        [Fact]
        public async Task Ensure_Task_Left_with_successInput_and_failurePredicate()
        {
          var initialResult = Task.FromResult(Result.Success("Initial Result"));

          var result = await initialResult.Ensure(() => Result.Failure(ErrorMessage2));

          result.IsSuccess.Should().BeFalse("Predicate is failure result");
          result.Error.Should().Be(ErrorMessage2);
        }
        
        [Fact]
        public async Task Ensure_Task_Left_with_failureInput_and_successPredicate()
        {
          var initialResult = Task.FromResult(Result.Failure(ErrorMessage));

          var result = await initialResult.Ensure(() => Result.Success("Success message"));

          result.IsSuccess.Should().BeFalse("Initial result is failure result");
          result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Ensure_Task_Left_with_failureInput_and_failurePredicate()
        {
          var initialResult = Task.FromResult(Result.Failure(ErrorMessage));

          var result = await initialResult.Ensure(() => Result.Failure(ErrorMessage2));

          result.IsSuccess.Should().BeFalse("Initial result is failure result");
          result.Error.Should().Be(ErrorMessage);
        }
        
        [Fact]
        public async Task Ensure_Task_Left_with_successInput_and_parameterisedFailurePredicate()
        {
          var initialResult = Task.FromResult(Result.Success("Initial Success message"));

          var result = await initialResult.Ensure(_ => Result.Failure(ErrorMessage));

          result.IsSuccess.Should().BeFalse("Predicate is failure result");
          result.Error.Should().Be(ErrorMessage);
        }
    
        [Fact]
        public async Task Ensure_Task_Left_with_successInput_and_parameterisedSuccessPredicate()
        {
          var initialResult = Task.FromResult(Result.Success("Initial Success message"));

          var result = await initialResult.Ensure(_ => Result.Success("Success Message"));

          result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");;
          result.Value.Should().Be("Initial Success message");
        }
    
        [Fact]
        public async Task Ensure_Task_Left_with_failureInput_and_parameterisedSuccessPredicate()
        {
          var initialResult = Task.FromResult(Result.Failure<string>(ErrorMessage));

          var result = await initialResult.Ensure(_ => Result.Success("Success Message"));

          result.IsSuccess.Should().BeFalse("Initial result is failure result");;
          result.Error.Should().Be(ErrorMessage);
        }
    
        [Fact]
        public async Task Ensure_Task_Left_with_failureInput_and_parameterisedFailurePredicate()
        {
          var initialResult = Task.FromResult(Result.Failure<string>(ErrorMessage));

          var result = await initialResult.Ensure(_ => Result.Failure(ErrorMessage2));

          result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result");;
          result.Error.Should().Be(ErrorMessage);
        }
    }
}
