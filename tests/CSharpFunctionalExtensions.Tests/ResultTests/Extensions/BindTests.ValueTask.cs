using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTests_ValueTask : BindTestsBase
    {
        [Fact]
        public async ValueTask Bind_ValueTask_returns_failure_and_does_not_execute_func()
        {
            Result output = await ValueTask_Failure().Bind(ValueTask_Success);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_selects_new_result()
        {
            Result output = await ValueTask_Success().Bind(ValueTask_Success);
            
            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_T_returns_failure_and_does_not_execute_func()
        {
            Result output = await ValueTask_Failure_T().Bind(ValueTask_Success_T);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_T_selects_new_result()
        {
            Result output = await ValueTask_Success_T(T.Value).Bind(ValueTask_Success_T);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await ValueTask_Failure().Bind(ValueTask_Success_K);
            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_K_selects_new_result()
        {
            Result<K> output = await ValueTask_Success().Bind(ValueTask_Success_K);
            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_T_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await ValueTask_Failure_T().Bind(Func_T_ValueTask_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_T_K_selects_new_result()
        {
            Result<K> output = await ValueTask_Success_T(T.Value).Bind(Func_T_ValueTask_Success_K);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }
    }
}