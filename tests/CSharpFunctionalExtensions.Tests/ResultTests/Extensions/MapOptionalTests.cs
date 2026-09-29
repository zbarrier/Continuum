using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapOptionalTests : TestBase
    {
        [Fact]
        public void MapOptional_returns_failure_and_does_not_execute_func_for_failed_result()
        {
            var executed = false;
            Result<Maybe<int>> result = Result.Failure<Maybe<int>>(ErrorMessage);

            Result<Maybe<string>> returned = result.MapOptional(x => { executed = true; return x.ToString(); });

            executed.Should().BeFalse();
            returned.IsFailure.Should().BeTrue();
            returned.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void MapOptional_returns_none_and_does_not_execute_func_when_maybe_has_no_value()
        {
            var executed = false;
            Result<Maybe<int>> result = Result.Success(Maybe<int>.None);

            Result<Maybe<string>> returned = result.MapOptional(x => { executed = true; return x.ToString(); });

            executed.Should().BeFalse();
            returned.IsSuccess.Should().BeTrue();
            returned.Value.HasNoValue.Should().BeTrue();
        }

        [Fact]
        public void MapOptional_maps_inner_value_when_maybe_has_value()
        {
            Result<Maybe<int>> result = Result.Success(Maybe.From(5));

            Result<Maybe<string>> returned = result.MapOptional(x => x.ToString());

            returned.IsSuccess.Should().BeTrue();
            returned.Value.Value.Should().Be("5");
        }
    }
}
