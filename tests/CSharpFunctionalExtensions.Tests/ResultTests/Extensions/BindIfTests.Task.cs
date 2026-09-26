using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindIfTests_Task : BindIfTestsBase
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
        public async Task BindIf_Task_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Task<Result> resultTask = Result.SuccessIf(isSuccess, ErrorMessage).AsTask();

            Result returned = await resultTask.BindIf(condition, GetTaskAction(isSuccessAction));

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
        public async Task BindIf_Task_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Task<Result<T>> resultTask = Result.SuccessIf(isSuccess, T.Value, ErrorMessage).AsTask();

            Result<T> returned = await resultTask.BindIf(condition, GetTaskValueAction(isSuccessAction));

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
        public async Task BindIf_Task_computes_predicate_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Task<Result> resultTask = Result.SuccessIf(isSuccess, ErrorMessage).AsTask();

            Result returned = await resultTask.BindIf(GetPredicate(condition), GetTaskAction(isSuccessAction));

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
        public async Task BindIf_Task_computes_predicate_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Task<Result<T>> resultTask = Result.SuccessIf(isSuccess, T.Value, ErrorMessage).AsTask();

            Result<T> returned = await resultTask.BindIf(GetValuePredicate(condition), GetTaskValueAction(isSuccessAction));

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition, isSuccessAction));
        }
    }
}