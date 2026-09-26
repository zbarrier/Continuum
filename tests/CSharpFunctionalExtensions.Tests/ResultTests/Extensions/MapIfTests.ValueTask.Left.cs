using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using CSharpFunctionalExtensions;

using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapIfTests_ValueTask_Left : MapIfTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_ValueTask_Left_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            Result<T> returned = await result.MapIf(condition, GetValueTaskAction());

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_ValueTask_Left_computes_predicate_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            Result<T> returned = await result.MapIf(GetValuePredicate(condition), GetValueTaskAction());

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_ValueTask_Left_T_executes_func_conditionally_and_passes_context(
            bool isSuccess,
            bool condition
        )
        {
            ValueTask<Result<T>> resultTask = Result
                .SuccessIf(isSuccess, T.Value, ErrorMessage)
                .AsValueTask();

            Result<T> returned = await resultTask.MapIf(
                condition,
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return GetAction()(value);
                },
                ContextMessage
            );

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_ValueTask_Left_computes_predicate_T_executes_func_conditionally_and_passes_context(
            bool isSuccess,
            bool condition
        )
        {
            ValueTask<Result<T>> resultTask = Result
                .SuccessIf(isSuccess, T.Value, ErrorMessage)
                .AsValueTask();

            Result<T> returned = await resultTask.MapIf(
                (value, context) => GetValuePredicate(condition)(value),
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return GetAction()(value);
                },
                ContextMessage
            );

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition));
        }
    }
}
