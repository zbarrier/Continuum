using FluentAssertions;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public abstract class CompensateTestsBase : TestBase
    {
        protected bool funcExecuted;

        protected CompensateTestsBase()
        {
            funcExecuted = false;
        }

        protected Result GetSuccessResult(Error _)
        {
            funcExecuted.Should().BeFalse();

            funcExecuted = true;
            return Result.Success();
        }

        protected Result GetErrorResult(Error error)
        {
            funcExecuted.Should().BeFalse();

            funcExecuted = true;
            return Result.Failure(error);
        }

        protected Result GetSuccessResult(E _)
        {
            funcExecuted.Should().BeFalse();

            funcExecuted = true;
            return Result.Success();
        }

        protected Result GetErrorResult(E error)
        {
            funcExecuted.Should().BeFalse();

            funcExecuted = true;
            return Result.Failure(ErrorMessage);
        }

        protected Result<T> GetSuccessValueResult(Error _)
        {
            funcExecuted.Should().BeFalse();

            funcExecuted = true;
            return Result.Success(T.Value);
        }

        protected Result<T> GetErrorValueResult(Error error)
        {
            funcExecuted.Should().BeFalse();

            funcExecuted = true;
            return Result.Failure<T>(error);
        }

        protected Task<Result> GetSuccessResultTask(Error error) => GetSuccessResult(error).AsTask();

        protected Task<Result> GetErrorResultTask(Error error) => GetErrorResult(error).AsTask();

        protected Task<Result> GetSuccessResultTask(E error) => GetSuccessResult(error).AsTask();

        protected Task<Result> GetErrorResultTask(E error) => GetErrorResult(error).AsTask();

        protected Task<Result<T>> GetSuccessValueResultTask(Error error) => GetSuccessValueResult(error).AsTask();

        protected Task<Result<T>> GetErrorValueResultTask(Error error) => GetErrorValueResult(error).AsTask();

        protected ValueTask<Result> GetSuccessResultValueTask(Error error) => GetSuccessResult(error).AsValueTask();

        protected ValueTask<Result> GetErrorResultValueTask(Error error) => GetErrorResult(error).AsValueTask();

        protected ValueTask<Result> GetSuccessResultValueTask(E error) => GetSuccessResult(error).AsValueTask();

        protected ValueTask<Result> GetErrorResultValueTask(E error) => GetErrorResult(error).AsValueTask();

        protected ValueTask<Result<T>> GetSuccessValueResultValueTask(Error error) => GetSuccessValueResult(error).AsValueTask();

        protected ValueTask<Result<T>> GetErrorValueResultValueTask(Error error) => GetErrorValueResult(error).AsValueTask();
        
        protected void AssertFailure(Result output, bool executed = false)
        {
            funcExecuted.Should().Be(executed);
            output.IsFailure.Should().BeTrue();
            output.Error.Should().Be(ErrorMessage);
        }

        protected void AssertFailure(Result<K> output, bool executed = false)
        {
            funcExecuted.Should().Be(executed);
            output.IsFailure.Should().BeTrue();
            output.Error.Should().Be(ErrorMessage);
        }

        protected void AssertSuccess(Result output, bool executed = true)
        {
            funcExecuted.Should().Be(executed);
            output.IsSuccess.Should().BeTrue();
        }

        protected void AssertSuccess(Result<K> output, bool executed = true)
        {
            funcExecuted.Should().Be(executed);
            output.IsSuccess.Should().BeTrue();
            output.Value.Should().Be(K.Value);
        }
    }
}
