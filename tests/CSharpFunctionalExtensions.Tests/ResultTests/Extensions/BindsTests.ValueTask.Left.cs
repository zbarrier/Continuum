using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.Tests;
using Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTests_ValueTask_Left : BindTestsBase
    {
        [Fact]
        public async ValueTask Bind_ValueTask_Left_returns_failure_and_does_not_execute_func()
        {
            Result output = await ValueTask_Failure().Bind(Success);
            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_selects_new_result()
        {
            Result output = await ValueTask_Success().Bind(Success);
            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_T_returns_failure_and_does_not_execute_func()
        {
            Result output = await ValueTask_Failure_T().Bind(Success_T);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_T_selects_new_result()
        {
            Result output = await ValueTask_Success_T(T.Value).Bind(Success_T);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await ValueTask_Failure().Bind(Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_K_selects_new_result()
        {
            Result<K> output = await ValueTask_Success().Bind(Success_K);

            AssertSuccess(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_T_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await ValueTask_Failure_T().Bind(Success_T_Func_K);

            AssertFailure(output);
        }

        [Fact]
        public async ValueTask Bind_ValueTask_Left_T_K_selects_new_result()
        {
            Result<K> output = await ValueTask_Success_T(T.Value).Bind(Success_T_Func_K);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }
    }
}
