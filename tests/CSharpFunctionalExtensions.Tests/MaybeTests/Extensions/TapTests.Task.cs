using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
	public class TapTests_Task : MaybeTestBase
	{
		[Fact]
		public async Task Tap_Task_executes_action_and_returns_original_maybe_when_value()
		{
			var executed = false;
			var maybe = Task.FromResult<Maybe<T>>(T.Value);

			Maybe<T> returned = await maybe.Tap(value => { executed = true; value.Should().Be(T.Value); return Task.CompletedTask; });

			executed.Should().BeTrue();
			returned.Value.Should().Be(T.Value);
		}

		[Fact]
		public async Task Tap_Task_does_not_execute_action_and_returns_original_maybe_when_no_value()
		{
			var executed = false;
			var maybe = Task.FromResult<Maybe<T>>(null);

			Maybe<T> returned = await maybe.Tap(value => { executed = true; value.Should().Be(T.Value); return Task.CompletedTask; });

			executed.Should().BeFalse();
			returned.HasNoValue.Should().BeTrue();
		}
	}
}
