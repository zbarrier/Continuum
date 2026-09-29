using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
    public class LinqQueryValueTaskTests
    {
        private static ValueTask<Maybe<int>> VtOf(Maybe<int> maybe) => ValueTask.FromResult(maybe);

        [Fact]
        public async Task Select_ValueTask_maps_value()
        {
            Maybe<int> result = await (from x in VtOf(2) select x * 10);

            result.Value.Should().Be(20);
        }

        [Fact]
        public async Task SelectMany_ValueTask_both_async_combines_values()
        {
            Maybe<int> result = await (
                from x in VtOf(2)
                from y in VtOf(3)
                select x + y);

            result.Value.Should().Be(5);
        }

        [Fact]
        public async Task SelectMany_ValueTask_Left_combines_values()
        {
            Maybe<int> result = await (
                from x in VtOf(2)
                from y in Maybe.From(3)
                select x + y);

            result.Value.Should().Be(5);
        }

        [Fact]
        public async Task SelectMany_ValueTask_Right_combines_values()
        {
            Maybe<int> result = await (
                from x in Maybe.From(2)
                from y in VtOf(3)
                select x + y);

            result.Value.Should().Be(5);
        }

        [Fact]
        public async Task SelectMany_ValueTask_returns_none_when_first_has_no_value()
        {
            Maybe<int> result = await (
                from x in VtOf(Maybe<int>.None)
                from y in VtOf(3)
                select x + y);

            result.HasNoValue.Should().BeTrue();
        }
    }
}
