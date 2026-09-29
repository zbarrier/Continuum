using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
    public class LinqQueryAsyncTests
    {
        private static Task<Maybe<int>> TaskOf(Maybe<int> maybe) => Task.FromResult(maybe);

        [Fact]
        public async Task Select_Task_maps_value()
        {
            Maybe<int> result = await (from x in TaskOf(2) select x * 10);

            result.Value.Should().Be(20);
        }

        [Fact]
        public async Task Select_Task_returns_none_when_no_value()
        {
            Maybe<int> result = await (from x in TaskOf(Maybe<int>.None) select x * 10);

            result.HasNoValue.Should().BeTrue();
        }

        [Fact]
        public async Task SelectMany_Task_both_async_combines_values()
        {
            Maybe<int> result = await (
                from x in TaskOf(2)
                from y in TaskOf(3)
                select x + y);

            result.Value.Should().Be(5);
        }

        [Fact]
        public async Task SelectMany_Task_Left_combines_values()
        {
            Maybe<int> result = await (
                from x in TaskOf(2)
                from y in Maybe.From(3)
                select x + y);

            result.Value.Should().Be(5);
        }

        [Fact]
        public async Task SelectMany_Task_Right_combines_values()
        {
            Maybe<int> result = await (
                from x in Maybe.From(2)
                from y in TaskOf(3)
                select x + y);

            result.Value.Should().Be(5);
        }

        [Fact]
        public async Task SelectMany_Task_Right_does_not_invoke_selector_when_no_value()
        {
            var invoked = false;

            Maybe<int> result = await (
                from x in Maybe<int>.None
                from y in Invoke()
                select x + y);

            invoked.Should().BeFalse();
            result.HasNoValue.Should().BeTrue();

            Task<Maybe<int>> Invoke()
            {
                invoked = true;
                return TaskOf(3);
            }
        }

        [Fact]
        public async Task SelectMany_Task_returns_none_when_second_has_no_value()
        {
            Maybe<int> result = await (
                from x in TaskOf(2)
                from y in TaskOf(Maybe<int>.None)
                select x + y);

            result.HasNoValue.Should().BeTrue();
        }

        [Fact]
        public async Task SelectMany_Task_with_three_sources_combines_values()
        {
            Maybe<int> result = await (
                from x in TaskOf(1)
                from y in TaskOf(2)
                from z in TaskOf(3)
                select x + y + z);

            result.Value.Should().Be(6);
        }
    }
}
