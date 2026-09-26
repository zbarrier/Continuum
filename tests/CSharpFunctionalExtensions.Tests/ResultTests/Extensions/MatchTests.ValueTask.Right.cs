using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MatchTests_ValueTask_Right : MatchTestsBase
    {
        [Fact]
        public async Task Match_ValueTask_Right_Result_Success()
        {
            var result = Result.Success();

            await result.Match(OnSuccess_ValueTask, OnFailure_Error_ValueTask);

            AssertSuccess();
        }

        [Fact]
        public async Task Match_ValueTask_Right_Result_Failure()
        {
            var result = Result.Failure(ErrorMessage);

            await result.Match(OnSuccess_ValueTask, OnFailure_Error_ValueTask);

            AssertFailure();
        }
        
        [Fact]
        public async Task Match_ValueTask_Right_Result_Success_Returns_K()
        {
            var result = Result.Success();

            var matched = await result.Match(OnSuccess_K_ValueTask, OnFailure_Error_K_ValueTask);

            AssertSuccess();
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_ValueTask_Right_Result_Failure_Returns_K()
        {
            var result = Result.Failure(ErrorMessage);

            var matched = await result.Match(OnSuccess_K_ValueTask, OnFailure_Error_K_ValueTask);

            AssertFailure();
            matched.Should().Be(K.Value2);
        }
        
        [Fact]
        public async Task Match_ValueTask_Right_Result_T_Success()
        {
            var result = Result.Success(T.Value);

            var matched = await result.Match(OnSuccess_T_K_ValueTask, OnFailure_Error_K_ValueTask);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_ValueTask_Right_Result_T_Failure()
        {
            var result = Result.Failure<T>(ErrorMessage);

            await result.Match(OnSuccess_T_ValueTask, OnFailure_Error_ValueTask);

            AssertFailure();
        }
        
        [Fact]
        public async Task Match_ValueTask_Right_Result_T_Success_Returns_K()
        {
            var result = Result.Success(T.Value);

            var matched = await result.Match(OnSuccess_T_K_ValueTask, OnFailure_Error_K_ValueTask);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_ValueTask_Right_Result_T_Failure_Returns_K()
        {
            var result = Result.Failure<T>(ErrorMessage);

            var matched = await result.Match(OnSuccess_T_K_ValueTask, OnFailure_Error_K_ValueTask);

            AssertFailure();
            
            matched.Should().Be(K.Value2);
        }
    }
}