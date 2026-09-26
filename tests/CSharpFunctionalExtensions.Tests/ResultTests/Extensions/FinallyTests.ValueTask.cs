using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class FinallyTests_ValueTask : FinallyTestsBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Finally_ValueTask_result_returns_K(bool isSuccess)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);
            K output = await result.AsValueTask().Finally(ValueTask_Func_Result);

            AssertCalled(result, output);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Finally_ValueTask_result_T_returns_K(bool isSuccess)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);
            K output = await result.AsValueTask().Finally(ValueTask_Func_Result_T);

            AssertCalled(result, output);
        }
    }
}
