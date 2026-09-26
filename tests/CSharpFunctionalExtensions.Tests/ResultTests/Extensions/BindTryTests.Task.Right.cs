using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTryTests_Task_Right : BindTryTestsBase
    {
        #region BindTry for Result with function returning Task<Result>
        [Fact]
        public async Task BindTry_execute_task_func_returning_success_on_success_returns_success()
        {
            Result sut = Result.Success();

            Result result = await sut.BindTry(Task_Success);

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_success_on_failure_returns_failure()
        {
            Result sut = Result.Failure(ErrorMessage);

            Result result = await sut.BindTry(Task_Success);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_failure_on_success_returns_failure()
        {
            Result sut = Result.Success();

            Result result = await sut.BindTry(Task_Failure);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_on_success_returns_failure_with_exception_message()
        {
            Result sut = Result.Success();

            Result result = await sut.BindTry(Task_Throwing);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result sut = Result.Success();

            Result result = await sut.BindTry(Task_Throwing, e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Result with function returning Task<Result<K>>
        [Fact]
        public async Task BindTry_execute_task_func_returning_success_K_on_success_returns_success()
        {
            Result sut = Result.Success();

            Result<K> result = await sut.BindTry(Task_Success_K);

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_success_K_on_failure_returns_failure()
        {
            Result sut = Result.Failure(ErrorMessage);

            Result<K> result = await sut.BindTry(Task_Success_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_failure_K_on_success_returns_failure()
        {
            Result sut = Result.Success();

            Result<K> result = await sut.BindTry(Task_Failure_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_K_on_success_returns_failure_with_exception_message()
        {
            Result sut = Result.Success();

            Result<K> result = await sut.BindTry(Task_Throwing_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_K_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result sut = Result.Success();

            Result<K> result = await sut.BindTry(Task_Throwing_K, e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Result<T> with function returning Task<Result>
        [Fact]
        public async Task BindTry_execute_task_func_returning_success_T_on_success_returns_success()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = await sut.BindTry(t => Task_Success());

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_success_T_on_failure_returns_failure()
        {
            Result<T> sut = Result.Failure<T>(ErrorMessage);

            Result result = await sut.BindTry(t => Task_Success());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_failure_T_on_success_returns_failure()
        {
            Result<T> sut = Result.Failure<T>(ErrorMessage);

            Result result = await sut.BindTry(t => Task_Failure());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_on_success_T_returns_failure_with_exception_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = await sut.BindTry(t => Task_Throwing());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = await sut.BindTry(t => Task_Throwing(), e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Result<T> with function returning Task<Result<K>>
        [Fact]
        public async Task BindTry_execute_task_func_returning_success_K_on_success_T_returns_success()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = await sut.BindTry(t => Task_Success_K());

            AssertSuccess(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_success_K_on_failure_T_returns_failure()
        {
            Result<T> sut = Result.Failure<T>(ErrorMessage);

            Result<K> result = await sut.BindTry(t => Task_Success_K());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_task_func_returning_failure_K_on_success_T_returns_failure()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = await sut.BindTry(t => Task_Failure_K());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_K_on_success_T_returns_failure_with_exception_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = await sut.BindTry(t => Task_Throwing_K());

            AssertFailure(result);
        }

        [Fact]
        public async Task BindTry_execute_throwing_task_func_K_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = await sut.BindTry(t => Task_Throwing_K(), e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion
    }
}
