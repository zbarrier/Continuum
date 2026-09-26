using FluentAssertions;
using Xunit;
using System;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public sealed class BindTryTests : BindTryTestsBase
    {
        #region BindTry for Result with function returning Result
        [Fact]
        public void BindTry_execute_func_returning_success_on_success_returns_success()
        {
            Result sut = Result.Success();

            Result result = sut.BindTry(Success);

            AssertSuccess(result);
        }
        [Fact]
        public void BindTry_execute_func_returning_success_on_failure_returns_self()
        {
            Result sut = Result.Failure(ErrorMessage);

            Result result = sut.BindTry(Success);

            AssertFailure(result);
        }
        [Fact]
        public void BindTry_execute_func_returning_failure_on_success_returns_failure()
        {
            Result sut = Result.Success();

            Result result = sut.BindTry(Failure);

            AssertFailure(result);
        }

        [Fact]
        public void BindTry_execute_throwing_func_on_success_returns_failure_with_exception_message()
        {
            Result sut = Result.Success();

            Result result = sut.BindTry(Throwing);

            AssertFailure(result);
        }


        [Fact]
        public void BindTry_execute_throwing_func_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result sut = Result.Success();

            Result result = sut.BindTry(Throwing, e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Result with function returning Result<K>
        [Fact]
        public void BindTry_execute_func_K_returning_success_on_success_returns_success()
        {
            Result sut = Result.Success();

            Result<K> result = sut.BindTry(Success_K);

            AssertSuccess(result);
        }
        [Fact]
        public void BindTry_execute_func_K_returning_success_on_failure_returns_self()
        {
            Result sut = Result.Failure(ErrorMessage);

            Result<K> result = sut.BindTry(Success_K);

            AssertFailure(result);
        }
        [Fact]
        public void BindTry_execute_func_K_returning_failure_on_success_returns_failure()
        {
            Result sut = Result.Success();

            Result<K> result = sut.BindTry(Failure_K);

            AssertFailure(result);
        }

        [Fact]
        public void BindTry_execute_throwing_func_K_on_success_returns_failure_with_exception_message()
        {
            Result sut = Result.Success();

            Result<K> result = sut.BindTry(Throwing_K);

            AssertFailure(result);
        }
        [Fact]
        public void BindTry_execute_throwing_func_K_on_success_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result sut = Result.Success();

            Result<K> result = sut.BindTry(Throwing_K, e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Result<T> with function returning Result
        [Fact]
        public void BindTry_execute_func_returning_success_on_success_T_returns_success()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = sut.BindTry(t => Success());

            AssertSuccess(result);
        }
        [Fact]
        public void BindTry_execute_func_returning_success_on_failure_T_returns_self()
        {
            Result<T> sut = Result.Failure<T>(ErrorMessage);

            Result result = sut.BindTry(t => Success());

            AssertFailure(result);
        }
        [Fact]
        public void BindTry_execute_func_returning_failure_on_success_T_returns_failure()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = sut.BindTry(t => Failure());

            AssertFailure(result);
        }

        [Fact]
        public void BindTry_execute_throwing_func_on_success_T_returns_failure_with_exception_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = sut.BindTry(t => Throwing());

            AssertFailure(result);
        }
        [Fact]
        public void BindTry_execute_throwing_func_on_success_T_with_custom_error_handler_returns_failure_with_custom_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result result = sut.BindTry(t => Throwing(), e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion

        #region BindTry for Result<T> with function returning Result<K>
        [Fact]
        public void BindTry_execute_func_T_K_returning_success_on_success_T_returns_success_K()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = sut.BindTry(t => Success_K());

            AssertSuccess(result);            
        }
        [Fact]
        public void BindTry_execute_func_T_K_returning_success_on_failure_T_returns_failure_K()
        {
            Result<T> sut = Result.Failure<T>(ErrorMessage);

            Result<K> result = sut.BindTry(t => Success_K());

            AssertFailure(result);
            result.Should().BeOfType<Result<K>>();
        }
        [Fact]
        public void BindTry_execute_func_K_returning_failure_K_on_success_returns_failure_K()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = sut.BindTry(t => Failure_K());

            AssertFailure(result);
        }

        [Fact]
        public void BindTry_execute_throwing_func_T_K_on_success_T_returns_failure_K_with_exception_message()
        {
            Result<T> sut = Result.Success(T.Value);

            Result<K> result = sut.BindTry(t => Throwing_K());

            AssertFailure(result);
        }
        [Fact]
        public void BindTry_execute_throwing_func_T_K_on_success_T_with_custom_error_handler_returns_failure_K_with_custom_message()
        {
            Result sut = Result.Success();

            Result<K> result = sut.BindTry(() => Throwing_K(), e => ErrorMessage2);

            AssertFailure(result, ErrorMessage2);
        }
        #endregion
    }
}
