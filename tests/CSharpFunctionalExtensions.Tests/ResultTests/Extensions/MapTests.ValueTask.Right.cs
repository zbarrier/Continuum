using FluentAssertions;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using Xunit;
using CSharpFunctionalExtensions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions 
{
    public class MapTests_ValueTask_Right : MapTestsBase 
    {
        [Fact]
        public async Task Map_ValueTask_Right_executes_on_success_returns_new_success()
        {
            Result result = Result.Success();
            Result<K> actual = await result.Map(valueTask: ValueTask_Func_K);

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Right_executes_on_failure_returns_new_failure()
        {
            Result result = Result.Failure(ErrorMessage);
            Result<K> actual = await result.Map(valueTask: ValueTask_Func_K);

            actual.IsSuccess.Should().BeFalse();
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_ValueTask_Right_T_executes_on_success_returns_new_success()
        {
            Result<T> result = Result.Success(T.Value);
            Result<K> actual = await result.Map(valueTask: ValueTask_Func_T_K);

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Right_T_executes_on_failure_returns_new_failure()
        {
            Result<T> result = Result.Failure<T>(ErrorMessage);
            Result<K> actual = await result.Map(valueTask: ValueTask_Func_T_K);

            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(ErrorMessage);
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_ValueTask_Right_with_context_executes_on_success_and_passes_correct_context()
        {
            Result result = Result.Success();
            Result<K> actual = await result.Map(
                valueTask: context =>
                {
                    context.Should().Be(ContextMessage);
                    return ValueTask_Func_K();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Right_with_context_executes_on_failure_and_passes_correct_context()
        {
            Result result = Result.Failure(ErrorMessage);
            Result<K> actual = await result.Map(
                valueTask: context =>
                {
                    context.Should().Be(ContextMessage);
                    return ValueTask_Func_K();
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            FuncExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task Map_ValueTask_Right_T_with_context_executes_on_success_and_passes_correct_context()
        {
            Result<T> result = Result.Success(T.Value);
            Result<K> actual = await result.Map(
                valueTask: (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return ValueTask_Func_T_K(value);
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().Be(K.Value);
            FuncExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task Map_ValueTask_Right_T_with_context_executes_on_failure_and_passes_correct_context()
        {
            Result<T> result = Result.Failure<T>(ErrorMessage);
            Result<K> actual = await result.Map(
                valueTask: (value, context) =>
                {
                    context.Should().Be(ContextMessage);
                    return ValueTask_Func_T_K(value);
                },
                ContextMessage
            );

            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(ErrorMessage);
            FuncExecuted.Should().BeFalse();
        }
    }
}
