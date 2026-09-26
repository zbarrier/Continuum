using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapErrorTests_ValueTask_Right : TestBase
    {
        private const string ContextMessage = "Context-specific error";

        [Fact]
        public async Task MapError_ValueTask_Right_returns_success()
        {
            Result result = Result.Success();
            var invocations = 0;

            Result actual = await result.MapError(error =>
            {
                invocations++;
                return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
            });

            actual.IsSuccess.Should().BeTrue();
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_returns_success_with_context()
        {
            Result result = Result.Success();
            var invocations = 0;

            Result actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_returns_new_failure()
        {
            Result result = Result.Failure(ErrorMessage);
            var invocations = 0;

            Result actual = await result.MapError(error =>
            {
                invocations++;
                return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
            });

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_returns_new_failure_with_context()
        {
            Result result = Result.Failure(ErrorMessage);
            var invocations = 0;

            Result actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_T_returns_success()
        {
            Result<T> result = Result.Success(T.Value);
            var invocations = 0;

            Result<T> actual = await result.MapError(error =>
            {
                invocations++;
                return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
            });

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(T.Value);
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_T_returns_success_with_context()
        {
            Result<T> result = Result.Success(T.Value);
            var invocations = 0;

            Result<T> actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(T.Value);
            invocations.Should().Be(0);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_T_returns_new_failure()
        {
            Result<T> result = Result.Failure<T>(ErrorMessage);
            var invocations = 0;

            Result<T> actual = await result.MapError(error =>
            {
                invocations++;
                return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
            });

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }

        [Fact]
        public async Task MapError_ValueTask_Right_T_returns_new_failure_with_context()
        {
            Result<T> result = Result.Failure<T>(ErrorMessage);
            var invocations = 0;

            Result<T> actual = await result.MapError(
                (error, context) =>
                {
                    context.Should().Be(ContextMessage);
                    invocations++;
                    return Task.FromResult((Error)RequestErrors.NewUnknown("{0} {1}", error.GetFormattedMessage(), error.GetFormattedMessage()));
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.GetFormattedMessage().Should().Be($"{ErrorMessageString} {ErrorMessageString}");
            invocations.Should().Be(1);
        }
    }
}