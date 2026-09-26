using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CheckIfTests_Task : CheckIfTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_Task_T_executes_func_result_T_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsTask().CheckIf(condition, Task_Func_Result);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_Task_T_executes_func_result_K_conditionally_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsTask().CheckIf(condition, Task_Func_Result_K);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_Task_T_executes_func_result_T_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsTask().CheckIf(Predicate, Task_Func_Result);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CheckIf_Task_T_executes_func_result_K_per_predicate_and_returns_self(bool isSuccess, bool condition)
        {
            Result<bool> result = Result.SuccessIf(isSuccess, condition, ErrorMessage);

            var returned = await result.AsTask().CheckIf(Predicate, Task_Func_Result_K);

            actionExecuted.Should().Be(isSuccess && condition);
            result.Should().Be(returned);
        }
    }
}