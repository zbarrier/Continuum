using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class SelectAsyncTests : TestBase
    {
        [Fact]
        public async Task Select_Task_maps_value_in_linq_query()
        {
            Result<int> result = await (from x in Task.FromResult(Result.Success(2)) select x * 10);

            result.Value.Should().Be(20);
        }

        [Fact]
        public async Task Select_Task_preserves_failure()
        {
            Result<int> result = await (from x in Task.FromResult(Result.Failure<int>(ErrorMessage)) select x * 10);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Select_ValueTask_maps_value_in_linq_query()
        {
            Result<int> result = await (from x in ValueTask.FromResult(Result.Success(2)) select x * 10);

            result.Value.Should().Be(20);
        }

        [Fact]
        public async Task Select_ValueTask_preserves_failure()
        {
            Result<int> result = await (from x in ValueTask.FromResult(Result.Failure<int>(ErrorMessage)) select x * 10);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task SelectMany_Task_then_Select_Task_works_in_linq_query()
        {
            Result<int> result = await (
                from x in Task.FromResult(Result.Success(2))
                from y in Result.Success(3)
                select x + y);

            result.Value.Should().Be(5);
        }
    }
}
