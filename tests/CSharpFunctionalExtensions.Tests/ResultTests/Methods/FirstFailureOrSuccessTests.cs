using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods
{
    public class FirstFailureOrSuccessTests : TestBase
    {
        [Fact]
        public void FirstFailureOrSuccess_returns_the_first_failed_result()
        {
            Result result1 = Result.Success();
            Result result2 = Result.Failure(ErrorMessage);
            Result result3 = Result.Failure(ErrorMessage2);

            Result result = Result.FirstFailureOrSuccess(result1, result2, result3);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
            result.Should().Be(result2);
        }

        [Fact]
        public void FirstFailureOrSuccess_returns_success_if_no_failures()
        {
            Result result1 = Result.Success();
            Result result2 = Result.Success();
            Result result3 = Result.Success();

            Result result = Result.FirstFailureOrSuccess(result1, result2, result3);

            result.IsSuccess.Should().BeTrue();
            result.Should().Be(Result.Success());
        }
    }
}
