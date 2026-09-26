using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CompensateTests : CompensateTestsBase
    {
        [Fact]
        public void Compensate_returns_success_and_does_not_execute_func()
        {
            Result input = Result.Success();

            Result output = input.Compensate(GetErrorResult);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public void Compensate_returns_failure_and_execute_func_returns_success()
        {
            Result input = Result.Failure(ErrorMessage);

            Result output = input.Compensate(GetSuccessResult);

            AssertSuccess(output, executed: true);
        }

        [Fact]
        public void Compensate_returns_failure_and_execute_func_returns_failure()
        {
            Result input = Result.Failure(ErrorMessage);

            Result output = input.Compensate(GetErrorResult);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public void Compensate_T_returns_success_and_does_not_execute_func()
        {
            Result<T> input = Result.Success(T.Value);

            Result output = input.Compensate(GetErrorResult);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public void Compensate_T_returns_failure_and_execute_func_returns_success()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result output = input.Compensate(GetSuccessResult);

            AssertSuccess(output, executed: true);
        }

        [Fact]
        public void Compensate_T_returns_failure_and_execute_func_returns_failure()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result output = input.Compensate(GetErrorResult);

            AssertFailure(output, executed: true);
        }

        [Fact]
        public void Compensate_T_returns_T_success_and_does_not_execute_func()
        {
            Result<T> input = Result.Success(T.Value);

            Result<T> output = input.Compensate(GetErrorValueResult);

            AssertSuccess(output, executed: false);
        }

        [Fact]
        public void Compensate_T_returns_T_failure_and_execute_func_returns_success()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result<T> output = input.Compensate(GetSuccessValueResult);

            AssertSuccess(output, executed: true);
        }

        [Fact]
        public void Compensate_T_returns_T_failure_and_execute_func_returns_failure()
        {
            Result<T> input = Result.Failure<T>(ErrorMessage);

            Result<T> output = input.Compensate(GetErrorValueResult);

            AssertFailure(output, executed: true);
        }
    }
}
