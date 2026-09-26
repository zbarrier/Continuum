using System.Threading.Tasks;
using CSharpFunctionalExtensions;

using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapIfTests_Task_Left : MapIfTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_Task_Left_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            Result<T> returned = await result.MapIf(condition, GetTaskAction());

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_Task_Left_computes_predicate_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            Result<T> returned = await result.MapIf(GetValuePredicate(condition), GetTaskAction());

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task MapIf_Task_Left_T_with_context_executes_func_conditionally_and_passes_context(
            bool isSuccess,
            bool condition
        )
        {
            Task<Result<T>> resultTask = Result
                .SuccessIf(isSuccess, T.Value, ErrorMessage)
                .AsTask();

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
        public async Task MapIf_Task_Left_computes_predicate_T_with_context_executes_func_conditionally_and_passes_context(
            bool isSuccess,
            bool condition
        )
        {
            Task<Result<T>> resultTask = Result
                .SuccessIf(isSuccess, T.Value, ErrorMessage)
                .AsTask();

            Result<T> returned = await resultTask.MapIf(
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return GetValuePredicate(condition)(value);
                },
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
