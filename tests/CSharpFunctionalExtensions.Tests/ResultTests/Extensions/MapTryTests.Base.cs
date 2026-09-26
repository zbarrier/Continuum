using Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions;
using FluentAssertions;
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MapTryTestsBase : MapTestsBase
    {        
        protected K Throwing_K() => throw new Exception(ErrorMessageString, GetFuncException());
        protected K Throwing_T_K(T _) => throw new Exception(ErrorMessageString, GetFuncException());

        protected Task<K> Task_Throwing_K() => throw new Exception(ErrorMessageString, GetFuncException());
        protected Task<K> Task_Throwing_T_K(T _) => throw new Exception(ErrorMessageString, GetFuncException());

        protected ValueTask<K> ValueTask_Throwing_K() => throw new Exception(ErrorMessageString, GetFuncException());
        protected ValueTask<K> ValueTask_Throwing_T_K(T _) => throw new Exception(ErrorMessageString, GetFuncException());


        protected static Error ErrorHandler(Exception _) => ErrorMessage2;


        protected void AssertSuccess(Result<K> result)
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(K.Value);                   
            FuncExecuted.Should().BeTrue();
        }

        protected void AssertFailure(Result result)
        {
            AssertFailure(result, ErrorMessage, false);
        }
        protected void AssertFailureFromHandler(Result result)
        {
            AssertFailure(result, ErrorMessage2, true);
        }
        protected void AssertFailureFromDefaultHandler(Result result)
        {
            AssertFailure(result, ErrorMessage, true);
        }

        protected void AssertFailure(Result result, Error error, bool fromFunc)
        {
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(error);
            FuncExecuted.Should().Be(fromFunc);
        }
        private Exception GetFuncException()
        {
            Func_K();
            return new Exception("Thrown from function");
        }
    }
}
