using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MatchTests_Task_Right : MatchTestsBase
    {
        [Fact]
        public async Task Match_Task_Right_Result_Success()
        {
            var result = Result.Success();

            await result.Match(OnSuccess_Task, OnFailure_Error_Task);

            AssertSuccess();
        }

        [Fact]
        public async Task Match_Task_Right_Result_Failure()
        {
            var result = Result.Failure(ErrorMessage);

            await result.Match(OnSuccess_Task, OnFailure_Error_Task);

            AssertFailure();
        }
        
        [Fact]
        public async Task Match_Task_Right_Result_Success_Returns_K()
        {
            var result = Result.Success();

            var matched = await result.Match(OnSuccess_K_Task, OnFailure_Error_K_Task);

            AssertSuccess();
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_Task_Right_Result_Failure_Returns_K()
        {
            var result = Result.Failure(ErrorMessage);

            var matched = await result.Match(OnSuccess_K_Task, OnFailure_Error_K_Task);

            AssertFailure();
            matched.Should().Be(K.Value2);
        }
        
        [Fact]
        public async Task Match_Task_Right_Result_T_Success()
        {
            var result = Result.Success(T.Value);

            var matched = await result.Match(OnSuccess_T_K_Task, OnFailure_Error_K_Task);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_Task_Right_Result_T_Failure()
        {
            var result = Result.Failure<T>(ErrorMessage);

            await result.Match(OnSuccess_T_Task, OnFailure_Error_Task);

            AssertFailure();
        }
        
        [Fact]
        public async Task Match_Task_Right_Result_T_Success_Returns_K()
        {
            var result = Result.Success(T.Value);

            var matched = await result.Match(OnSuccess_T_K_Task, OnFailure_Error_K_Task);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_Task_Right_Result_T_Failure_Returns_K()
        {
            var result = Result.Failure<T>(ErrorMessage);

            var matched = await result.Match(OnSuccess_T_K_Task, OnFailure_Error_K_Task);

            AssertFailure();
            
            matched.Should().Be(K.Value2);
        }
    }
}