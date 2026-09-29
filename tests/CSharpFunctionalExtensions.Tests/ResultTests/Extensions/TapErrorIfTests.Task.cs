using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class TapErrorIfTests_Task : TapErrorIfTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_executes_action_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(condition, (Func<Task>)Task_Action);

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_executes_action_String_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(condition, (Func<Error, Task>)Task_Action);

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_T_executes_action_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(condition, (Func<Task>)Task_Action);

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_T_executes_action_T_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(condition, (Func<Error, Task>)Task_Action);

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_executes_action_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(Predicate_Error(condition), (Func<Task>)Task_Action);

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_executes_action_String_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(Predicate_Error(condition), (Func<Error, Task>)Task_Action);

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_T_executes_action_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(Predicate_Error(condition), (Func<Task>)Task_Action);

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task TapErrorIf_Task_T_executes_action_String_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = await result.AsTask().TapErrorIf(Predicate_Error(condition), (Func<Error, Task>)Task_Action);

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }
    }
}
