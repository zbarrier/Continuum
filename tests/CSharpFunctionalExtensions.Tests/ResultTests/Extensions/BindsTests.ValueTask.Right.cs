using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTests_ValueTask_Right : BindTestsBase
    {
        [Fact]
        public async ValueTask Bind_ValueTask_Right_returns_failure_and_does_not_execute_func()
        {
            Result output = await Failure().Bind(ValueTask_Success);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_selects_new_result()
        {
            Result output = await Success().Bind(ValueTask_Success);

            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_T_returns_failure_and_does_not_execute_func()
        {
            Result output = await Failure_T().Bind(ValueTask_Success_T);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_T_selects_new_result()
        {
            Result output = await Success_T(T.Value).Bind(ValueTask_Success_T);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await Failure().Bind(ValueTask_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_K_selects_new_result()
        {
            Result<K> output = await Success().Bind(ValueTask_Success_K);

            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_T_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await Failure_T().Bind(Func_T_ValueTask_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Right_T_K_selects_new_result()
        {
            Result<K> output = await Success_T(T.Value).Bind(Func_T_ValueTask_Success_K);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }
    }
}
