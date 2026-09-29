using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
	public class TapTests : MaybeTestBase
	{
		[Fact]
		public void Tap_executes_action_and_returns_original_maybe_when_value()
		{
			var executed = false;
			Maybe<T> maybe = T.Value;

			Maybe<T> returned = maybe.Tap(value => { executed = true; value.Should().Be(T.Value); });

			executed.Should().BeTrue();
			returned.Value.Should().Be(T.Value);
		}

		[Fact]
		public void Tap_does_not_execute_action_and_returns_original_maybe_when_no_value()
		{
			var executed = false;
			Maybe<T> maybe = null;

			Maybe<T> returned = maybe.Tap(value => { executed = true; value.Should().Be(T.Value); });

			executed.Should().BeFalse();
			returned.HasNoValue.Should().BeTrue();
		}
	}
}
