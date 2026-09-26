using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTryTests_Task_Left : BindTryTestsBase
    {
        #region BindTry for Task<Result> with function returning Result
        [Fact]
        public async Task BindTry_execute_func_returning_success_on_task_success_returns_success()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result result = await sut.BindTry(Success);

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_success_on_task_failure_returns_failure()
        {
            Task<Result> sut = Result.Failure(ErrorMessage).AsTask();

            Result result = await sut.BindTry(Success);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_failure_on_task_success_returns_failure()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result result = await sut.BindTry(Failure);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_on_taks_success_returns_failure_with_exception_message()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result result = await sut.BindTry(Throwing);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_on_task_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result result = await sut.BindTry(Throwing, e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Task<Result> with function returning Result<K>
        [Fact]
        public async Task BindTry_execute_func_returning_success_K_on_task_success_returns_success()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.BindTry(Success_K);

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_success_K_on_task_failure_returns_failure()
        {
            Task<Result> sut = Result.Failure(ErrorMessage).AsTask();

            Result<K> result = await sut.BindTry(Success_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_failure_K_on_task_success_returns_failure()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.BindTry(Failure_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_K_on_taks_success_returns_failure_with_exception_message()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.BindTry(Throwing_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_K_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.BindTry(Throwing_K, e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Task<Result<T>> with function returning Result
        [Fact]
        public async Task BindTry_execute_func_returning_success_on_task_success_T_returns_success()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result result = await sut.BindTry(t => Success());

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_success_T_on_task_failure_returns_failure()
        {
            Task<Result<T>> sut = Result.Failure<T>(ErrorMessage).AsTask();

            Result result = await sut.BindTry(t => Success());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_failure_on_task_success_T_returns_failure()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result result = await sut.BindTry(t => Failure());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_on_taks_success_T_returns_failure_with_exception_message()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result result = await sut.BindTry(t => Throwing());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result result = await sut.BindTry(t => Throwing(), e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Task<Result<T>> with function returning Result<K>
        [Fact]
        public async Task BindTry_execute_func_returning_success_K_on_task_success_T_returns_success()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.BindTry(t => Success_K());

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_success_K_on_task_failure_T_returns_failure()
        {
            Task<Result<T>> sut = Result.Failure<T>(ErrorMessage).AsTask();

            Result<K> result = await sut.BindTry(t => Success_K());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_func_returning_failure_K_on_task_success_T_returns_failure()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.BindTry(t => Failure_K());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_K_on_taks_success_T_returns_failure_with_exception_message()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.BindTry(t => Throwing_K());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_func_K_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.BindTry(t => Throwing_K(), e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion
    }
}
