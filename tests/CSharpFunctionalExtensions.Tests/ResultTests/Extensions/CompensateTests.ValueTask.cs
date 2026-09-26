using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CompensateTests_ValueTask : CompensateTestsBase
    {
        [Fact]
        public async Task Compensate_ValueTask_returns_success_and_does_not_execute_func()
        {
            ValueTask<Result> input = Result.Success().AsValueTask();

            Result output = await input.Compensate(GetErrorResultValueTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_ValueTask_returns_failure_and_does_not_execute_func()
        {
            ValueTask<Result> input = Result.Failure(ErrorMessage).AsValueTask();

            Result output = await input.Compensate(GetSuccessResultValueTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_ValueTask_returns_success_and_execute_func()
        {
            ValueTask<Result> input = Result.Failure(ErrorMessage).AsValueTask();

            Result output = await input.Compensate(GetErrorResultValueTask);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_ValueTask_T_returns_success_and_does_not_execute_func()
        {
            ValueTask<Result<T>> input = Result.Success(T.Value).AsValueTask();

            Result output = await input.Compensate(GetErrorResultValueTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_ValueTask_T_returns_failure_and_does_not_execute_func()
        {
            ValueTask<Result<T>> input = Result.Failure<T>(ErrorMessage).AsValueTask();

            Result output = await input.Compensate(GetSuccessResultValueTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_ValueTask_T_returns_success_and_execute_func()
        {
            ValueTask<Result<T>> input = Result.Failure<T>(ErrorMessage).AsValueTask();

            Result output = await input.Compensate(GetErrorResultValueTask);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_ValueTask_T_returns_T_success_and_does_not_execute_func()
        {
            ValueTask<Result<T>> input = Result.Success(T.Value).AsValueTask();

            Result<T> output = await input.Compensate(GetErrorValueResultValueTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_ValueTask_T_returns_T_failure_and_does_not_execute_func()
        {
            ValueTask<Result<T>> input = Result.Failure<T>(ErrorMessage).AsValueTask();

            Result<T> output = await input.Compensate(GetSuccessValueResultValueTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_ValueTask_T_returns_T_success_and_execute_func()
        {
            ValueTask<Result<T>> input = Result.Failure<T>(ErrorMessage).AsValueTask();

            Result<T> output = await input.Compensate(GetErrorValueResultValueTask);

            AssertFailure(output, executed: true);
        }
    }
}
