using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CheckIfTests_ValueTask_Left : CheckIfTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_ValueTask_Left_T_executes_func_result_T_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsValueTask().CheckIf(condition, Func_Result);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_ValueTask_Left_T_executes_func_result_K_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsValueTask().CheckIf(condition, Func_Result_K);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_ValueTask_Left_T_executes_func_result_T_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsValueTask().CheckIf(Predicate, Func_Result);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_ValueTask_Left_T_executes_func_result_K_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsValueTask().CheckIf(Predicate, Func_Result_K);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }
    }
}