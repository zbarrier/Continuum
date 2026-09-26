using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
    public class ToResultTests_ValueTask : MaybeTestBase
    {
        [Fact]
        public async Task ToResult_ValueTask_returns_failure_if_has_no_value()
        {
            Maybe<T> maybe = null;

            var result = await maybe.AsValueTask().ToResult(ErrorMessage);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task ToResult_ValueTask_returns_success_if_has_value()
        {
            var maybe = Maybe<T>.From(T.Value);

            var result = await maybe.AsValueTask().ToResult(ErrorMessage);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(T.Value);
        }

        [Fact]
        public async Task ToResult_ValueTask_returns_custom_failure_if_has_no_value()
        {
            Maybe<T> maybe = null;

            var result = await maybe.AsValueTask().ToResult(E.Value);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(E.Value);
        }

        [Fact]
        public async Task ToResult_ValueTask_custom_failure_returns_success_if_has_value()
        {
            var maybe = Maybe<T>.From(T.Value);

            var result = await maybe.AsValueTask().ToResult(E.Value);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(T.Value);
        }

        [Fact]
        public async Task ToResult_ValueTask_returns_custom_failure_via_error_function_if_has_no_value()
        {
            Maybe<T> maybe = null;

            var result = await maybe.AsValueTask().ToResult(ErrorFunc);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(E.Value);
            AssertErrorFuncCalled();
        }

        [Fact]
        public async Task ToResult_ValueTask_custom_failure_with_error_function_returns_success_if_has_value()
        {
            var maybe = Maybe<T>.From(T.Value);

            var result = await maybe.AsValueTask().ToResult(ErrorFunc);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(T.Value);
            AssertErrorFuncNotCalled();
        }
    }
}