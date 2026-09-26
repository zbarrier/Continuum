using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTests_Task_Right : BindTestsBase
    {
        [Fact]
        public async Task Bind_Task_Right_returns_failure_and_does_not_execute_func()
        {
            Result output = await Failure().Bind(Task_Success);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_Right_selects_new_result()
        {
            Result output = await Success().Bind(Task_Success);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Bind_Task_Right_T_returns_failure_and_does_not_execute_func()
        {
            Result output = await Failure_T().Bind(Task_Success_T);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_Right_T_selects_new_result()
        {
            Result output = await Success_T(T.Value).Bind(Task_Success_T);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }

        [Fact]
        public async Task Bind_Task_Right_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await Failure().Bind(Task_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_Right_K_selects_new_result()
        {
            Result<K> output = await Success().Bind(Task_Success_K);

            AssertSuccess(output);
        }

        [Fact]
        public async Task Bind_Task_Right_T_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await Failure_T().Bind(Func_T_Task_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_Right_T_K_selects_new_result()
        {
            Result<K> output = await Success_T(T.Value).Bind(Func_T_Task_Success_K);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }
    }
}
