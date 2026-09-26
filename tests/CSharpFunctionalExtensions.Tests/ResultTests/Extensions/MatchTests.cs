using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MatchTests : MatchTestsBase
    {
        [Fact]
        public void Match_Result_Success()
        {
            var result = Result.Success();

            result.Match(OnSuccess, OnFailure_Error);

            AssertSuccess();
        }

        [Fact]
        public void Match_Result_Failure()
        {
            var result = Result.Failure(ErrorMessage);

            result.Match(OnSuccess, OnFailure_Error);

            AssertFailure();
        }
        
        [Fact]
        public void Match_Result_Success_Returns_K()
        {
            var result = Result.Success();

            var matched = result.Match(OnSuccess_K, OnFailure_Error_K);

            AssertSuccess();
            matched.Should().Be(K.Value);
        }

        [Fact]
        public void Match_Result_Failure_Returns_K()
        {
            var result = Result.Failure(ErrorMessage);

            var matched = result.Match(OnSuccess_K, OnFailure_Error_K);

            AssertFailure();
            matched.Should().Be(K.Value2);
        }
        
        [Fact]
        public void Match_Result_T_Success()
        {
            var result = Result.Success(T.Value);

            var matched = result.Match(OnSuccess_T_K, OnFailure_Error_K);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public void Match_Result_T_Failure()
        {
            var result = Result.Failure<T>(ErrorMessage);

            result.Match(OnSuccess_T, OnFailure_Error);

            AssertFailure();
        }
        
        [Fact]
        public void Match_Result_T_Success_Returns_K()
        {
            var result = Result.Success(T.Value);

            var matched = result.Match(OnSuccess_T_K, OnFailure_Error_K);

            AssertSuccess();
            
            matched.Should().Be(K.Value);
        }

        [Fact]
        public void Match_Result_T_Failure_Returns_K()
        {
            var result = Result.Failure<T>(ErrorMessage);

            var matched = result.Match(OnSuccess_T_K, OnFailure_Error_K);

            AssertFailure();
            
            matched.Should().Be(K.Value2);
        }
    }
}