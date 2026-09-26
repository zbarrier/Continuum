using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CompensateTests_Task : CompensateTestsBase
    {
        [Fact]
        public async Task Compensate_Task_returns_success_and_does_not_execute_func()
        {
            Task<Result> input = Result.Success().AsTask();

            Result output = await input.Compensate(GetErrorResultTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_Task_returns_failure_and_does_not_execute_func()
        {
            Task<Result> input = Result.Failure(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetSuccessResultTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_Task_returns_success_and_execute_func()
        {
            Task<Result> input = Result.Failure(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetErrorResultTask);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_Task_T_returns_success_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Success(T.Value).AsTask();

            Result output = await input.Compensate(GetErrorResultTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_Task_T_returns_failure_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetSuccessResultTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_Task_T_returns_success_and_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetErrorResultTask);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_Task_T_returns_T_success_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Success(T.Value).AsTask();

            Result<T> output = await input.Compensate(GetErrorValueResultTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_Task_T_returns_T_failure_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result<T> output = await input.Compensate(GetSuccessValueResultTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_Task_T_returns_T_success_and_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result<T> output = await input.Compensate(GetErrorValueResultTask);

            AssertFailure(output, executed: true);
        }
    }
}
