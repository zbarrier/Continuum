using CSharpFunctionalExtensions;

using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions 
{
    public class MapTests_Task : MapTestsBase 
    {
        [Fact]
        public async Task Map_Task_executes_on_success_returns_new_success()
        {
            Task<Result> result = Result.Success().AsTask();
            Result<K> actual = await result.Map(Task_Func_K);

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_Task_executes_on_failure_returns_new_failure()
        {
            Task<Result> result = Result.Failure(ErrorMessage).AsTask();
            Result<K> actual = await result.Map(Task_Func_K);

            actual.IsSuccess.Should().BeFalse();
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_Task_T_executes_on_success_returns_new_success()
        {
            Task<Result<T>> result = Result.Success(T.Value).AsTask();
            Result<K> actual = await result.Map(Task_Func_T_K);

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_Task_T_executes_on_failure_returns_new_failure()
        {
            Task<Result<T>> result = Result.Failure<T>(ErrorMessage).AsTask();
            Result<K> actual = await result.Map(Task_Func_T_K);

            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(ErrorMessage);
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_Task_with_context_executes_on_success_and_passes_correct_context()
        {
            Task<Result> result = Result.Success().AsTask();
            Result<K> actual = await result.Map(
                (context) =>
                {
                    context.Should().Be(ContextMessage);
                    return Task_Func_K();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_Task_with_context_executes_on_failure_and_passes_correct_context()
        {
            Task<Result> result = Result.Failure(ErrorMessage).AsTask();
            Result<K> actual = await result.Map(
                (context) =>
                {
                    context.Should().Be(ContextMessage);
                    return Task_Func_K();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_Task_T_with_context_executes_on_success_and_passes_correct_context()
        {
            Task<Result<T>> result = Result.Success(T.Value).AsTask();
            Result<K> actual = await result.Map(
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return Task_Func_T_K(value);
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_Task_T_with_context_executes_on_failure_and_passes_correct_context()
        {
            Task<Result<T>> result = Result.Failure<T>(ErrorMessage).AsTask();
            Result<K> actual = await result.Map(
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return Task_Func_T_K(value);
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(ErrorMessage);
            FuncExecuted.Should().BeFalse();
        }
    }
}
