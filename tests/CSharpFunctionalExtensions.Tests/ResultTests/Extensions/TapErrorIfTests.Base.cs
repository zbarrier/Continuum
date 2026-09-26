using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public abstract class TapErrorIfTestsBase : TestBase
    {
        protected bool actionExecuted;
        protected bool predicateExecuted;

        protected TapErrorIfTestsBase()
        {
            actionExecuted = false;
            predicateExecuted = false;
        }

        protected void Action()
        {
            actionExecuted = true;
        }

        protected void Action_Error(Error _)
        {
            actionExecuted = true;
        }

        protected Task Task_Action()
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }

        protected Task Task_Action(Error _)
        {
            actionExecuted = true;
            return Task.CompletedTask;
        }

        protected ValueTask ValueTask_Action()
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }

        protected ValueTask ValueTask_Action_Error(Error _)
        {
            actionExecuted = true;
            return ValueTask.CompletedTask;
        }

        protected Func<Error, bool> Predicate_Error(bool condition)
        {
            return _ =>
            {
                predicateExecuted = true;
                return condition;
            };
        }
    }
}
