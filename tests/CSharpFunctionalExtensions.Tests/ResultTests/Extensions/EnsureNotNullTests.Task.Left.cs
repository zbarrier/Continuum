#nullable enable

using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class EnsureNotNullTests_Task_Left : EnsureNotNullTests_Base
    {
        [Fact]
        public async Task EnsureNotNull_Task_Left_T_factory_with_class_returns_failed_return_for_failed_result()
        {
            Result<T?> result = Result.Failure<T?>(ErrorMessage);

            Result<T> returned = await result.AsTask().EnsureNotNull(GetErrorFactory(ErrorMessage2));

            returned.IsSuccess.Should().BeFalse();
            returned.Error.Should().Be(ErrorMessage);
            factoryExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task EnsureNotNull_Task_Left_V_factory_with_struct_returns_failed_return_for_failed_result()
        {
            Result<V?> result = Result.Failure<V?>(ErrorMessage);

            Result<V> returned = await result.AsTask().EnsureNotNull(GetErrorFactory(ErrorMessage2));

            returned.IsSuccess.Should().BeFalse();
            returned.Error.Should().Be(ErrorMessage);
            factoryExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task EnsureNotNull_Task_Left_T_factory_with_class_returns_original_success_result_if_value_is_not_null()
        {
            Result<T?> result = Result.Success<T?>(T.Value);

            Result<T> returned = await result.AsTask().EnsureNotNull(GetErrorFactory(ErrorMessage2));

            returned.IsSuccess.Should().BeTrue();
            returned.Value.Should().Be(T.Value);
            factoryExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task EnsureNotNull_Task_Left_V_factory_with_struct_returns_original_success_result_if_value_is_not_null()
        {
            Result<V?> result = Result.Success<V?>(V.Value);

            Result<V> returned = await result.AsTask().EnsureNotNull(GetErrorFactory(ErrorMessage2));

            returned.IsSuccess.Should().BeTrue();
            returned.Value.Should().Be(V.Value);
            factoryExecuted.Should().BeFalse();
        }

        [Fact]
        public async Task EnsureNotNull_Task_Left_T_factory_with_class_returns_failed_result_for_success_result_if_value_is_null()
        {
            Result<T?> result = Result.Success<T?>(null);

            Result<T> returned = await result.AsTask().EnsureNotNull(GetErrorFactory(ErrorMessage2));

            returned.IsSuccess.Should().BeFalse();
            returned.Error.Should().Be(ErrorMessage2);
            factoryExecuted.Should().BeTrue();
        }

        [Fact]
        public async Task EnsureNotNull_Task_Left_V_factory_with_struct_returns_failed_result_for_success_result_if_value_is_null()
        {
            Result<V?> result = Result.Success<V?>(null);

            Result<V> returned = await result.AsTask().EnsureNotNull(GetErrorFactory(ErrorMessage2));

            returned.IsSuccess.Should().BeFalse();
            returned.Error.Should().Be(ErrorMessage2);
            factoryExecuted.Should().BeTrue();
        }
    }
}
