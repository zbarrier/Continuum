using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MatchTests_Task_Left : MatchTestsBase
    {
        [Fact]
        public async Task Match_Task_Left_Result_Success()
        {
            var result = Result.Success().AsTask();

            await result.Match(OnSuccess, OnFailure_Error);

            AssertSuccess();
        }

        [Fact]
        public async Task Match_Task_Left_Result_Failure()
        {
            var result = Result.Failure(ErrorMessage).AsTask();

            await result.Match(OnSuccess, OnFailure_Error);

            AssertFailure();
        }
        
        [Fact]
        public async Task Match_Task_Left_Result_Success_Returns_K()
        {
            var result = Result.Success().AsTask();

            var matched = await result.Match(OnSuccess_K, OnFailure_Error_K);

            AssertSuccess();
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_Task_Left_Result_Failure_Returns_K()
        {
            var result = Result.Failure(ErrorMessage).AsTask();

            var matched = await result.Match(OnSuccess_K, OnFailure_Error_K);

            AssertFailure();
            matched.Should().Be(K.Value2);
        }
        
        [Fact]
        public async Task Match_Task_Left_Result_T_Success()
        {
            var result = Result.Success(T.Value).AsTask();

            var matched = await result.Match(OnSuccess_T_K, OnFailure_Error_K);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_Task_Left_Result_T_Failure()
        {
            var result = Result.Failure<T>(ErrorMessage).AsTask();

            await result.Match(OnSuccess_T, OnFailure_Error);

            AssertFailure();
        }
        
        [Fact]
        public async Task Match_Task_Left_Result_T_Success_Returns_K()
        {
            var result = Result.Success(T.Value).AsTask();

            var matched = await result.Match(OnSuccess_T_K, OnFailure_Error_K);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public async Task Match_Task_Left_Result_T_Failure_Returns_K()
        {
            var result = Result.Failure<T>(ErrorMessage).AsTask();

            var matched = await result.Match(OnSuccess_T_K, OnFailure_Error_K);

            AssertFailure();
            
            matched.Should().Be(K.Value2);
        }
    }
}