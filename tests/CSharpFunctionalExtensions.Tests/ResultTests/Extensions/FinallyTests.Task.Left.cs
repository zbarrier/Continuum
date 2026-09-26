using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class FinallyTests_Task_Left : FinallyTestsBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Finally_Task_Left_result_returns_K(bool isSuccess)
        {
            Result result = Result.SuccessIf(isSuccess, ErrorMessage);
            K output = await result.AsTask().Finally(Func_Result);

            AssertCalled(result, output);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Finally_Task_Left_result_T_returns_K(bool isSuccess)
        {
            Result<T> result = Result.SuccessIf(isSuccess, T.Value, ErrorMessage);
            K output = await result.AsTask().Finally(Func_Result_T);

            AssertCalled(result, output);
        }
    }
}
