using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CompensateTests_Task_Left : CompensateTestsBase
    {
        [Fact]
        public async Task Compensate_Task_Left_returns_success_and_does_not_execute_func()
        {
            Task<Result> input = Result.Success().AsTask();

            Result output = await input.Compensate(GetErrorResult);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_Task_Left_returns_failure_and_does_not_execute_func()
        {
            Task<Result> input = Result.Failure(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetSuccessResult);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_Task_Left_returns_success_and_execute_func()
        {
            Task<Result> input = Result.Failure(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetErrorResult);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_Task_Left_T_returns_success_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Success(T.Value).AsTask();

            Result output = await input.Compensate(GetErrorResult);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_Task_Left_T_returns_failure_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetSuccessResult);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_Task_Left_T_returns_success_and_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result output = await input.Compensate(GetErrorResult);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_Task_Left_T_returns_T_success_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Success(T.Value).AsTask();

            Result<T> output = await input.Compensate(GetErrorValueResult);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_Task_Left_T_returns_T_failure_and_does_not_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result<T> output = await input.Compensate(GetSuccessValueResult);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_Task_Left_T_returns_T_success_and_execute_func()
        {
            Task<Result<T>> input = Result.Failure<T>(ErrorMessage).AsTask();

            Result<T> output = await input.Compensate(GetErrorValueResult);

            AssertFailure(output, executed: true);
        }
    }
}
