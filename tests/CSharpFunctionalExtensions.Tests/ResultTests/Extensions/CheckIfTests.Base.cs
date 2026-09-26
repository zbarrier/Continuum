using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public abstract class CheckIfTestsBase : TestBase
    {
        protected bool actionExecuted;
        protected bool predicateExecuted;

        protected CheckIfTestsBase()
        {
            actionExecuted = false;
            predicateExecuted = false;
        }
        
        protected Result Func_Result(bool _) { actionExecuted = true; return Result.Success(); } 
        
        protected Result<K> Func_Result_K(bool _) { actionExecuted = true; return Result.Success(K.Value); }

        protected Task<Result> Task_Func_Result(bool _) { actionExecuted = true; return Result.Success().AsTask(); }
        protected Task<Result<K>> Task_Func_Result_K(bool _) { actionExecuted = true; return Result.Success(K.Value).AsTask(); }

        protected ValueTask<Result> ValueTask_Func_Result(bool _) { actionExecuted = true; return Result.Success().AsValueTask(); }
        protected ValueTask<Result<K>> ValueTask_Func_Result_K(bool _) { actionExecuted = true; return Result.Success(K.Value).AsValueTask(); }
        
        protected bool Predicate(bool b) { predicateExecuted = true; return b; }
    }
}