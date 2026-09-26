using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTryTestsBase : BindTestsBase
    {
        protected static Result Throwing() => throw new Exception(ErrorMessageString);        
        protected static Result<K> Throwing_K() => throw new Exception(ErrorMessageString);        

        protected static Task<Result> Task_Throwing() => throw new Exception(ErrorMessageString);        
        protected static Task<Result<K>> Task_Throwing_K() => throw new Exception(ErrorMessageString);        

        protected static ValueTask<Result> ValueTask_Throwing() => throw new Exception(ErrorMessageString);        
        protected static ValueTask<Result<K>> ValueTask_Throwing_K() => throw new Exception(ErrorMessageString);        


        protected void AssertFailure(Result result, Error message)
        {
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(message);
            FuncExecuted.Should().BeFalse();
        }
    }
}
