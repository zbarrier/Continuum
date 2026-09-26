using System.Threading.Tasks;
using Xunit;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapTryTests_ValueTask_Left : MapTryTestsBase
    {
        #region MapTry for ValueTask<Result> with function returning K
        [Fact]
        public async ValueTask MapTry_execute_func_K_on_valuetask_success_returns_success()
        {
            ValueTask<Result> sut = Result.Success().AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Func_K);

            AssertSuccess(result);
        }

        [Fact]
        public async ValueTask MapTry_execute_func_K_on_valuetask_failure_returns_failure()
        {
            ValueTask<Result> sut = Result.Failure(ErrorMessage).AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Func_K);

            AssertFailure(result);
        }

        [Fact]
        public async ValueTask MapTry_execute_throwing_func_K_on_taks_success_returns_failure_with_exception_message()
        {
            ValueTask<Result> sut = Result.Success().AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Throwing_K);

            AssertFailureFromDefaultHandler(result);
        }

        [Fact]
        public async ValueTask MapTry_execute_throwing_func_K_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            ValueTask<Result> sut = Result.Success().AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Throwing_K, ErrorHandler);

            AssertFailureFromHandler(result);
        }
        #endregion

        #region MapTry for ValueTask<Result<T>> with function returning K
        [Fact]
        public async ValueTask MapTry_execute_func_K_on_valuetask_success_T_returns_success()
        {
            ValueTask<Result<T>> sut = Result.Success(T.Value).AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Func_T_K);

            AssertSuccess(result);
        }

        [Fact]
        public async ValueTask MapTry_execute_func_K_on_valuetask_failure_T_returns_failure()
        {
            ValueTask<Result<T>> sut = Result.Failure<T>(ErrorMessage).AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Func_T_K);

            AssertFailure(result);
        }

        [Fact]
        public async ValueTask MapTry_execute_throwing_func_K_on_taks_success_T_returns_failure_with_exception_message()
        {
            ValueTask<Result<T>> sut = Result.Success(T.Value).AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Throwing_T_K);

            AssertFailureFromDefaultHandler(result);
        }

        [Fact]
        public async ValueTask MapTry_execute_throwing_func_K_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            ValueTask<Result<T>> sut = Result.Success(T.Value).AsValueTask();

            Result<K> result = await sut.MapTry(valueTask:Throwing_T_K, ErrorHandler);

            AssertFailureFromHandler(result);
        }
        #endregion
    }
}
