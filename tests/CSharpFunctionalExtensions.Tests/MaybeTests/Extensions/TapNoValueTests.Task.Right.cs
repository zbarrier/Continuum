using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
	public class TapNoValueTests_Task_Right : MaybeTestBase
	{
		[Fact]
		public async Task TapNoValue_Task_Right_executes_action_and_returns_original_maybe_when_no_value()
		{
			var executed = false;
			Maybe<T> maybe = null;

			Maybe<T> returned = await maybe.TapNoValue(() => { executed = true; return Task.CompletedTask; });

			executed.Should().BeTrue();
			returned.HasNoValue.Should().BeTrue();
		}

		[Fact]
		public async Task TapNoValue_Task_Right_does_not_execute_action_and_returns_original_maybe_when_value()
		{
			var executed = false;
			Maybe<T> maybe = T.Value;

			Maybe<T> returned = await maybe.TapNoValue(() => { executed = true; return Task.CompletedTask; });

			executed.Should().BeFalse();
			returned.Value.Should().Be(T.Value);
		}
	}
}
