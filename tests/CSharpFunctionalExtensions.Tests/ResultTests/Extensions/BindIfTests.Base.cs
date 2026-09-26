using System;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public abstract class BindIfTestsBase : TestBase
    {
        protected bool actionExecuted;
        protected bool predicateExecuted;

        protected BindIfTestsBase()
        {
            actionExecuted = false;
            predicateExecuted = false;
        }

        protected Func<Result> GetAction(bool isSuccess)
        {
            if (!isSuccess)
            {
                return FailureAction;
            }

            return SuccessAction;
        }

        protected Func<Task<Result>> GetTaskAction(bool isSuccess)
        {
            if (!isSuccess)
            {
                return () => Task.FromResult(FailureAction());
            }

            return () => Task.FromResult(SuccessAction());
        }

        protected Func<ValueTask<Result>> GetValueTaskAction(bool isSuccess)
        {
            if (!isSuccess)
            {
                return () => FailureAction().AsCompletedValueTask();
            }

            return () => SuccessAction().AsCompletedValueTask();
        }
        
        protected Func<T, Result<T>> GetValueAction(bool isSuccess)
        {
            if (!isSuccess)
            {
                return FailureValueAction;
            }

            return SuccessValueAction;
        }

        protected Func<T, Task<Result<T>>> GetTaskValueAction(bool isSuccess)
        {
            if (!isSuccess)
            {
                return t => Task.FromResult(FailureValueAction(t));
            }

            return t => Task.FromResult(SuccessValueAction(t));
        }
        
        protected Func<T, ValueTask<Result<T>>> GetValueTaskValueAction(bool isSuccess)
        {
            if (!isSuccess)
            {
                return t => FailureValueAction(t).AsCompletedValueTask();
            }

            return t => SuccessValueAction(t).AsCompletedValueTask();
        }

        
        protected Result SuccessAction()
        {
            actionExecuted.Should().BeFalse();

            actionExecuted = true;
            return Result.Success();
        }

        protected Result FailureAction()
        {
            actionExecuted.Should().BeFalse();

            actionExecuted = true;
            return Result.Failure(ErrorMessage2);
        }

        protected Result<T> SuccessValueAction(T value)
        {
            actionExecuted.Should().BeFalse();
            value.Should().Be(T.Value);

            actionExecuted = true;
            return Result.Success(T.Value2);
        }

        protected Result<T> FailureValueAction(T value)
        {
            actionExecuted.Should().BeFalse();
            value.Should().Be(T.Value);

            actionExecuted = true;
            return Result.Failure<T>(ErrorMessage2);
        }

        protected Func<bool> GetPredicate(bool value)
        {
            return () =>
            {
                predicateExecuted.Should().BeFalse();

                predicateExecuted = true;
                return value;
            };
        }

        protected Func<T, bool> GetValuePredicate(bool value)
        {
            return t =>
            {
                predicateExecuted.Should().BeFalse();
                t.Should().Be(T.Value);

                predicateExecuted = true;
                return value;
            };
        }

        protected static Result GetExpectedResult(bool isSuccess, bool condition, bool isSuccessAction)
        {
            var success = CheckSuccess(isSuccess, condition, isSuccessAction);
            var resultChanged = isSuccess && condition;

            return Result.SuccessIf(success, !resultChanged ? ErrorMessage : ErrorMessage2);
        }

        protected static Result<T> GetExpectedValueResult(bool isSuccess, bool condition, bool isSuccessAction)
        {
            var success = CheckSuccess(isSuccess, condition, isSuccessAction);
            var resultChanged = isSuccess && condition;

            if (!resultChanged)
            {
                return Result.SuccessIf(success, T.Value, ErrorMessage);
            }

            return Result.SuccessIf(success, T.Value2, ErrorMessage2);
        }

        protected static bool CheckSuccess(bool isSuccess, bool condition, bool isSuccessAction)
        {
            if (!condition)
            {
                return isSuccess;
            }

            return isSuccess && isSuccessAction;
        }
    }
}