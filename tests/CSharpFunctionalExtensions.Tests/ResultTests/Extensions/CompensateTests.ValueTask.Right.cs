using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CompensateTests_ValueTask_Right : CompensateTestsBase
    {
        [Fact]
        public async Task Compensate_ValueTask_Right_returns_success_and_does_not_execute_func()
        {
            Result input = Result.Success();

            Result output = await input.Compensate(GetErrorResultTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_returns_failure_and_does_not_execute_func()
        {
            Result input = Result.Failure(ErrorMessage);

            Result output = await input.Compensate(GetSuccessResultTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_returns_success_and_execute_func()
        {
            Result input = Result.Failure(ErrorMessage);

            Result output = await input.Compensate(GetErrorResultTask);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_T_returns_success_and_does_not_execute_func()
        {
            Result<T> input = Result.Success(T.Value);

            Result output = await input.Compensate(GetErrorResultTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_T_returns_failure_and_does_not_execute_func()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result output = await input.Compensate(GetSuccessResultTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_T_returns_success_and_execute_func()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result output = await input.Compensate(GetErrorResultTask);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_T_returns_T_success_and_does_not_execute_func()
        {
            Result<T> input = Result.Success(T.Value);

            Result<T> output = await input.Compensate(GetErrorValueResultTask);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_T_returns_T_failure_and_does_not_execute_func()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result<T> output = await input.Compensate(GetSuccessValueResultTask);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Compensate_ValueTask_Right_T_returns_T_success_and_execute_func()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result<T> output = await input.Compensate(GetErrorValueResultTask);

            AssertFailure(output, executed: true);
        }
    }
}
