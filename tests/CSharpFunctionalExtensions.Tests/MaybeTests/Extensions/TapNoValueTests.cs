using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
	public class TapNoValueTests : MaybeTestBase
	{
		[Fact]
		public void TapNoValue_executes_action_and_returns_original_maybe_when_no_value()
		{
			var executed = false;
			Maybe<T> maybe = null;

			Maybe<T> returned = maybe.TapNoValue(() => executed = true);

			executed.Should().BeTrue();
			returned.HasNoValue.Should().BeTrue();
		}

		[Fact]
		public void TapNoValue_does_not_execute_action_and_returns_original_maybe_when_value()
		{
			var executed = false;
			Maybe<T> maybe = T.Value;

			Maybe<T> returned = maybe.TapNoValue(() => executed = true);

			executed.Should().BeFalse();
			returned.Value.Should().Be(T.Value);
		}
	}
}
