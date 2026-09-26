using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.Tests.ResultTests;
using FluentAssertions;

namespace Continuum.CSharpFunctionalExtensions.Tests
{
    public abstract class BindTestsBase : TestBase
    {
        private bool _funcExecuted;
        protected T FuncParam;

        protected BindTestsBase()
        {
            _funcExecuted = false;
            FuncParam = null;
        }

        protected bool FuncExecuted => _funcExecuted;

        protected Result Success()
        {
            _funcExecuted = true;
            return Result.Success();
        }

        protected Result Failure()
        {
            _funcExecuted = false;
            return Result.Failure(ErrorMessage);
        }

        protected Result<T> Success_T(T value)
        {
            _funcExecuted = true;
            FuncParam = value;
            return Result.Success(value);
        }

        protected Result<T> Failure_T()
        {
            _funcExecuted = false;
            return Result.Failure<T>(ErrorMessage);
        }

        protected Result<K> Success_K()
        {
            _funcExecuted = true;
            return Result.Success(K.Value);
        }
        protected Result<K> Failure_K()
        {
            _funcExecuted = false;
            return Result.Failure<K>(ErrorMessage);
        }

        protected Result<K> Success_T_Func_K(T value)
        {
            _funcExecuted = true;
            FuncParam = value;
            return Result.Success(K.Value);
        }

        protected Task<Result> Task_Success()
        {
            return Success().AsTask();
        }

        protected Task<Result> Task_Failure()
        {
            return Failure().AsTask();
        }

        protected Task<Result<T>> Task_Success_T(T value)
        {
            return Success_T(value).AsTask();
        }

        protected Task<Result<T>> Task_Failure_T()
        {
            return Failure_T().AsTask();
        }

        protected Task<Result<K>> Task_Success_K()
        {
            return Success_K().AsTask();
        }

        protected Task<Result<K>> Task_Failure_K()
        {
            return Failure_K().AsTask();
        }

        protected Task<Result<K>> Func_T_Task_Success_K(T value)
        {
            return Success_T_Func_K(value).AsTask();
        }

        protected ValueTask<Result> ValueTask_Success()
        {
            return Success().AsValueTask();
        }

        protected ValueTask<Result> ValueTask_Failure()
        {
            return Failure().AsValueTask();
        }

        protected ValueTask<Result<T>> ValueTask_Success_T(T value)
        {
            return Success_T(value).AsValueTask();
        }

        protected ValueTask<Result<T>> ValueTask_Failure_T()
        {
            return Failure_T().AsValueTask();
        }

        protected ValueTask<Result<K>> ValueTask_Success_K()
        {
            return Success_K().AsValueTask();
        }
        protected ValueTask<Result<K>> ValueTask_Failure_K()
        {
            return Failure_K().AsValueTask();
        }

        protected ValueTask<Result<K>> Func_T_ValueTask_Success_K(T value)
        {
            return Success_T_Func_K(value).AsValueTask();
        }

        protected void AssertFailure(Result output)
        {
            _funcExecuted.Should().BeFalse();
            output.IsFailure.Should().BeTrue();
            output.Error.Should().Be(ErrorMessage);
        }

        protected void AssertFailure(Result<K> output)
        {
            _funcExecuted.Should().BeFalse();
            output.IsFailure.Should().BeTrue();
            output.Error.Should().Be(ErrorMessage);
        }

        protected void AssertSuccess(Result output)
        {
            _funcExecuted.Should().BeTrue();
            output.IsSuccess.Should().BeTrue();
        }

        protected void AssertSuccess(Result<K> output)
        {
            _funcExecuted.Should().BeTrue();
            output.IsSuccess.Should().BeTrue();
            output.Value.Should().Be(K.Value);
        }
    }
}