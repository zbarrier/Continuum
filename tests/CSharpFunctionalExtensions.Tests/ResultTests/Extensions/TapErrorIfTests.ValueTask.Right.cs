using FluentAssertions;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class TapErrorIfTests_ValueTask_Right : TapErrorIfTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_executes_action_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = result.TapErrorIf(condition, ValueTask_Action).Result;

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_executes_action_String_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = result.TapErrorIf(condition, ValueTask_Action_Error).Result;

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_T_executes_action_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = result.TapErrorIf(condition, ValueTask_Action).Result;

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_T_executes_action_T_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = result.TapErrorIf(condition, ValueTask_Action_Error).Result;

            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_executes_action_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = result.TapErrorIf(Predicate_Error(condition), ValueTask_Action).Result;

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_executes_action_String_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            var returned = result.TapErrorIf(Predicate_Error(condition), ValueTask_Action_Error).Result;

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_T_executes_action_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = result.TapErrorIf(Predicate_Error(condition), ValueTask_Action).Result;

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TapErrorIf_ValueTask_Right_T_executes_action_String_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            var returned = result.TapErrorIf(Predicate_Error(condition), ValueTask_Action_Error).Result;

            predicateExecuted.Should().Be(!isSuccess);
            actionExecuted.Should().Be(!isSuccess && condition);
            result.Should().Be(returned);
        }
    }
}
