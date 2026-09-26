using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindIfTests : BindIfTestsBase
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void BindIf_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            Result returned = result.BindIf(condition, GetAction(isSuccessAction));

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedResult(isSuccess, condition, isSuccessAction));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void BindIf_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            Result<T> returned = result.BindIf(condition, GetValueAction(isSuccessAction));

            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition, isSuccessAction));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void BindIf_computes_predicate_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);

            Result returned = result.BindIf(GetPredicate(condition), GetAction(isSuccessAction));

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedResult(isSuccess, condition, isSuccessAction));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void BindIf_computes_predicate_T_executes_func_conditionally_and_returns_new_result(bool isSuccess, bool condition, bool isSuccessAction)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);

            Result<T> returned = result.BindIf(GetValuePredicate(condition), GetValueAction(isSuccessAction));

            predicateExecuted.Should().Be(isSuccess);
            actionExecuted.Should().Be(isSuccess && condition);
            returned.Should().Be(GetExpectedValueResult(isSuccess, condition, isSuccessAction));
        }
    }
}