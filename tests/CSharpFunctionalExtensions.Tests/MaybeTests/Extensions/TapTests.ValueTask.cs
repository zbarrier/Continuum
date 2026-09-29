using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
	public class TapTests_ValueTask : MaybeTestBase
	{
		[Fact]
		public async Task Tap_ValueTask_executes_action_and_returns_original_maybe_when_value()
		{
			var executed = false;
			var maybe = ValueTask.FromResult<Maybe<T>>(T.Value);

			Maybe<T> returned = await maybe.Tap(value => { executed = true; value.Should().Be(T.Value); return ValueTask.CompletedTask; });

			executed.Should().BeTrue();
			returned.Value.Should().Be(T.Value);
		}

		[Fact]
		public async Task Tap_ValueTask_does_not_execute_action_and_returns_original_maybe_when_no_value()
		{
			var executed = false;
			var maybe = ValueTask.FromResult<Maybe<T>>(null);

			Maybe<T> returned = await maybe.Tap(value => { executed = true; value.Should().Be(T.Value); return ValueTask.CompletedTask; });

			executed.Should().BeFalse();
			returned.HasNoValue.Should().BeTrue();
		}
	}
}
