using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapTryTests_Task_Left : MapTryTestsBase
    {
        #region MapTry for Task<Result> with function returning K
        [Fact]
        public async Task MapTry_execute_func_K_on_task_success_returns_success()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.MapTry(Func_K);

            AssertSuccess(result);
        }

        [Fact]
        public async Task MapTry_execute_func_K_on_task_failure_returns_failure()
        {
            Task<Result> sut = Result.Failure(ErrorMessage).AsTask();

            Result<K> result = await sut.MapTry(Func_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task MapTry_execute_throwing_func_K_on_taks_success_returns_failure_with_exception_message()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.MapTry(Throwing_K);

            AssertFailureFromDefaultHandler(result);
        }

        [Fact]
        public async Task MapTry_execute_throwing_func_K_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Task<Result> sut = Result.Success().AsTask();

            Result<K> result = await sut.MapTry(Throwing_K, ErrorHandler);

            AssertFailureFromHandler(result);
        }
        #endregion

        #region MapTry for Task<Result<T>> with function returning K
        [Fact]
        public async Task MapTry_execute_func_K_on_task_success_T_returns_success()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.MapTry(Func_T_K);

            AssertSuccess(result);
        }

        [Fact]
        public async Task MapTry_execute_func_K_on_task_failure_T_returns_failure()
        {
            Task<Result<T>> sut = Result.Failure<T>(ErrorMessage).AsTask();

            Result<K> result = await sut.MapTry(Func_T_K);

            AssertFailure(result);
        }

        [Fact]
        public async Task MapTry_execute_throwing_func_K_on_taks_success_T_returns_failure_with_exception_message()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.MapTry(Throwing_T_K);

            AssertFailureFromDefaultHandler(result);
        }

        [Fact]
        public async Task MapTry_execute_throwing_func_K_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Task<Result<T>> sut = Result.Success(T.Value).AsTask();

            Result<K> result = await sut.MapTry(Throwing_T_K, ErrorHandler);

            AssertFailureFromHandler(result);
        }
        #endregion
    }
}
