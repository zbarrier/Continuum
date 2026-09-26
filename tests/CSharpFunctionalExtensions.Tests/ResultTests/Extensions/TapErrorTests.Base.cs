using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public abstract class TapErrorTestsBase : TestBase
    {
        protected bool actionExecuted;

        protected TapErrorTestsBase()
        {
            actionExecuted = false;
        }

        protected void Action() => actionExecuted = true;

        protected void ActionError(Error _) => actionExecuted = true;


        protected Task TaskAction()
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }

        protected Task TaskActionError(Error _)
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }
        
        protected ValueTask ValueTaskAction()
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }

        protected ValueTask ValueTaskActionError(Error _)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }
    }
}