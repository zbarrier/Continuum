using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class BindTests_Task : BindTestsBase
    {
        [Fact]
        public async Task Bind_Task_returns_failure_and_does_not_execute_func()
        {
            Result output = await Task_Failure().Bind(Task_Success);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_selects_new_result()
        {
            Result output = await Task_Success().Bind(Task_Success);
            
            AssertSuccess(output);
        }

        [Fact]
        public async Task Bind_Task_T_returns_failure_and_does_not_execute_func()
        {
            Result output = await Task_Failure_T().Bind(Task_Success_T);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_T_selects_new_result()
        {
            Result output = await Task_Success_T(T.Value).Bind(Task_Success_T);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }

        [Fact]
        public async Task Bind_Task_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await Task_Failure().Bind(Task_Success_K);
            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_K_selects_new_result()
        {
            Result<K> output = await Task_Success().Bind(Task_Success_K);
            AssertSuccess(output);
        }

        [Fact]
        public async Task Bind_Task_T_K_returns_failure_and_does_not_execute_func()
        {
            Result<K> output = await Task_Failure_T().Bind(Func_T_Task_Success_K);

            AssertFailure(output);
        }

        [Fact]
        public async Task Bind_Task_T_K_selects_new_result()
        {
            Result<K> output = await Task_Success_T(T.Value).Bind(Func_T_Task_Success_K);

            FuncParam.Should().Be(T.Value);
            AssertSuccess(output);
        }
    }
}
