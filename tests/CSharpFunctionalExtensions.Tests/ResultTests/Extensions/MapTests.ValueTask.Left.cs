using FluentAssertions;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using Xunit;
using CSharpFunctionalExtensions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions 
{
    public class MapTests_ValueTask_Left : MapTestsBase 
    {
        [Fact]
        public async Task Map_ValueTask_Left_executes_on_success_returns_new_success()
        {
            ValueTask<Result> result = Result.Success().AsValueTask();
            Result<K> actual = await result.Map(Func_K);

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Left_executes_on_failure_returns_new_failure()
        {
            ValueTask<Result> result = Result.Failure(ErrorMessage).AsValueTask();
            Result<K> actual = await result.Map(Func_K);

            actual.IsSuccess.Should().BeFalse();
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_ValueTask_Left_T_executes_on_success_returns_new_success()
        {
            ValueTask<Result<T>> result = Result.Success(T.Value).AsValueTask();
            Result<K> actual = await result.Map(Func_T_K);

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Left_T_executes_on_failure_returns_new_failure()
        {
            ValueTask<Result<T>> result = Result.Failure<T>(ErrorMessage).AsValueTask();
            Result<K> actual = await result.Map(Func_T_K);

            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(ErrorMessage);
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_ValueTask_Left_with_context_executes_on_success_and_passes_correct_context()
        {
            ValueTask<Result> result = Result.Success().AsValueTask();
            Result<K> actual = await result.Map(
                context =>
                {
                    context.Should().Be(ContextMessage);
                    return Func_K();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Left_with_context_executes_on_failure_and_passes_correct_context()
        {
            ValueTask<Result> result = Result.Failure(ErrorMessage).AsValueTask();
            Result<K> actual = await result.Map(
                context =>
                {
                    context.Should().Be(ContextMessage);
                    return Func_K();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_ValueTask_Left_T_with_context_executes_on_success_and_passes_correct_context()
        {
            ValueTask<Result<T>> result = Result.Success(T.Value).AsValueTask();
            Result<K> actual = await result.Map(
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return Func_T_K(value);
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Left_T_with_context_executes_on_failure_and_passes_correct_context()
        {
            ValueTask<Result<T>> result = Result.Failure<T>(ErrorMessage).AsValueTask();
            Result<K> actual = await result.Map(
                (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return Func_T_K(value);
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(ErrorMessage);
            FuncExecuted.Should().BeFalse();
        }
    }
}
