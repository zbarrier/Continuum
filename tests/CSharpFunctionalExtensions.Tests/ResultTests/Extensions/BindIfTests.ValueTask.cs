using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindIfTests_ValueTask : BindIfTestsBase
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task BindIf_ValueTask_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            ValueTask<Result> resultTask = Result.SuccessIf(isSuccess, ErrorMessage).AsValueTask();

            Result returned = await resultTask.BindIf(condition, GetValueTaskAction(isSuccessAction));

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedResult(isSuccess, condition, isSuccessAction));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task BindIf_ValueTask_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            ValueTask<Result<T>> resultTask = Result.SuccessIf(isSuccess, T.Value, ErrorMessage).AsValueTask();

            Result<T> returned = await resultTask.BindIf(condition, GetValueTaskValueAction(isSuccessAction));

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition, isSuccessAction));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task BindIf_ValueTask_computes_predicate_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            ValueTask<Result> resultTask = Result.SuccessIf(isSuccess, ErrorMessage).AsValueTask();

            Result returned = await resultTask.BindIf(GetPredicate(condition), GetValueTaskAction(isSuccessAction));

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedResult(isSuccess, condition, isSuccessAction));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task BindIf_ValueTask_computes_predicate_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            ValueTask<Result<T>> resultTask = Result.SuccessIf(isSuccess, T.Value, ErrorMessage).AsValueTask();

            Result<T> returned = await resultTask.BindIf(GetValuePredicate(condition), GetValueTaskValueAction(isSuccessAction));

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition, isSuccessAction));
        }
    }
}