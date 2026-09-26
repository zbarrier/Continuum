using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class CheckTests_ValueTask_Left : CheckTestsBase
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task Check_ValueTask_Left_T_func_result(bool resultSuccess, bool funcSuccess)
        {
            Result<T> result = Result.SuccessIf(resultSuccess, T.Value, ErrorMessage);

            var returned = await result.AsTask().Check(_ => GetResult(funcSuccess));

            actionExecuted.Should().Be(resultSuccess);
            returned.Should().Be(funcSuccess ? result : FailedResultT);
        }
        
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task Check_ValueTask_Left_T_func_result_K(bool resultSuccess, bool funcSuccess)
        {
            Result<T> result = Result.SuccessIf(resultSuccess, T.Value, ErrorMessage);

            var returned = await result.AsTask().Check(Func_Result_K(funcSuccess));

            actionExecuted.Should().Be(resultSuccess);
            returned.Should().Be(funcSuccess ? result : FailedResultT);
        }
    }
}