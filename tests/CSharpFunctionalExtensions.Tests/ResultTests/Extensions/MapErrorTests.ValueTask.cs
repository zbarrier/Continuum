using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapError_ValueTask_Tests : TestBase
    {
        private const string ContextMessage = "Context-specific error";

        [Fact]
        public async Task MapError_ValueTask_returns_success()
        {
            ValueTask<Result> result = Result.Success().AsValueTask();
            var invocations = 0;

            Result actual = await result.MapError(error =>
            {
                invocations++;
                return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
            });

            actual.IsSuccess.Should().BeTrue();
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_returns_success_with_context()
        {
            ValueTask<Result> result = Result.Success().AsValueTask();
            var invocations = 0;

            Result actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_returns_new_failure()
        {
            ValueTask<Result> result = Result.Failure(ErrorMessage).AsValueTask();
            var invocations = 0;

            Result actual = await result.MapError(error =>
            {
                invocations++;
                return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
            });

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }

        [Fact]
        public async Task MapError_ValueTask_returns_new_failure_with_context()
        {
            ValueTask<Result> result = Result.Failure(ErrorMessage).AsValueTask();
            var invocations = 0;

            Result actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }

        [Fact]
        public async Task MapError_ValueTask_T_returns_success()
        {
            ValueTask<Result<T>> result = Result.Success(T.Value).AsValueTask();
            var invocations = 0;

            Result<T> actual = await result.MapError(error =>
            {
                invocations++;
                return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
            });

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(T.Value);
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_T_returns_success_with_context()
        {
            ValueTask<Result<T>> result = Result.Success(T.Value).AsValueTask();
            var invocations = 0;

            Result<T> actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(T.Value);
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_T_returns_new_failure()
        {
            ValueTask<Result<T>> result = Result.Failure<T>(ErrorMessage).AsValueTask();
            var invocations = 0;

            Result<T> actual = await result.MapError(error =>
            {
                invocations++;
                return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
            });

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }

        [Fact]
        public async Task MapError_ValueTask_T_returns_new_failure_with_context()
        {
            ValueTask<Result<T>> result = Result.Failure<T>(ErrorMessage).AsValueTask();
            var invocations = 0;

            Result<T> actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return ((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage())).AsCompletedValueTask();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }
    }
}