using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
    public class ExecuteNoValueTests_ValueTask_Left : MaybeTestBase
    {
        [Fact]
        public async Task ExecuteNoValue_ValueTask_Left_executes_action_when_no_value()
        {
            string property = null;

            Maybe<T> maybe = null;

            await maybe.AsValueTask().ExecuteNoValue(() => property = "Some value");

            property.Should().Be("Some value");
        }
    }
}