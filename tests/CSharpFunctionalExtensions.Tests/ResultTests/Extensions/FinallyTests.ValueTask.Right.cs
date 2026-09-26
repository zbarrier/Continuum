using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class FinallyTests_ValueTask_Right : FinallyTestsBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Finally_ValueTask_Right_result_returns_K(bool isSuccess)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);
            K output = await result.Finally(Task_Func_Result);

            AssertCalled(result, output);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Finally_ValueTask_Right_result_T_returns_K(bool isSuccess)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);
            K output = await result.Finally(Task_Func_Result_T);

            AssertCalled(result, output);
        }
    }
}
