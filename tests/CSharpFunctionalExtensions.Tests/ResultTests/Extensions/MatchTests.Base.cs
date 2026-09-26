using System.Threading.Tasks;
using FluentAssertions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class MatchTestsBase : TestBase
    {
        private bool _failure;
        private bool _success;
        
        protected void OnFailure_Error(Error arg)
        {
            _failure = true;
        }
        
        protected void OnSuccess()
        {
            _success = true;
        }
        
        protected K OnSuccess_K()
        {
            _success = true;
            return K.Value;
        }
        
        protected K OnFailure_Error_K(Error arg)
        {
            _failure = true;
            return K.Value2;
        }
        
        protected void OnSuccess_T(T value)
        {
            _success = true;
        }
        
        protected K OnSuccess_T_K(T value)
        {
            _success = true;
            return K.Value;
        }

        protected Task OnFailure_Error_Task(Error arg)
        {
            _failure = true;
            return Task.CompletedTask;
        }
        
        protected Task OnSuccess_Task()
        {
            OnSuccess();
            return Task.CompletedTask;
        }
        
        protected Task<K> OnSuccess_K_Task()
        {
            return OnSuccess_K().AsTask();
        }
        
        protected Task<K> OnFailure_Error_K_Task(Error arg)
        {
            return OnFailure_Error_K(arg).AsTask();
        }
        
        protected Task OnSuccess_T_Task(T value)
        {
            _success = true;
            return Task.CompletedTask;
        }
        
        protected Task<K> OnSuccess_T_K_Task(T value)
        {
            return OnSuccess_T_K(value).AsTask();
        }
        
        protected ValueTask OnFailure_Error_ValueTask(Error arg)
        {
            _failure = true;
            return ValueTask.CompletedTask;
        }
        
        protected ValueTask OnSuccess_ValueTask()
        {
            OnSuccess();
            return ValueTask.CompletedTask;
        }
        
        protected ValueTask<K> OnSuccess_K_ValueTask()
        {
            return OnSuccess_K().AsValueTask();
        }
        
        protected ValueTask<K> OnFailure_Error_K_ValueTask(Error arg)
        {
            return OnFailure_Error_K(arg).AsValueTask();
        }
        
        protected ValueTask OnSuccess_T_ValueTask(T value)
        {
            _success = true;
            return ValueTask.CompletedTask;
        }
        
        protected ValueTask<K> OnSuccess_T_K_ValueTask(T value)
        {
            return OnSuccess_T_K(value).AsValueTask();
        }
        
        protected void AssertSuccess()
        {
            _success.Should().BeTrue();
            _failure.Should().BeFalse();
        }
        
        protected void AssertFailure()
        {
            _failure.Should().BeTrue();
            _success.Should().BeFalse();
        }
    }
}