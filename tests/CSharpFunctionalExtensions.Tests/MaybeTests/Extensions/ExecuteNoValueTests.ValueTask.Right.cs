using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.MaybeTests.Extensions
{
    public class ExecuteNoValueTests_ValueTask_Right : MaybeTestBase
    {
        [Fact]
        public async Task ExecuteNoValue_ValueTask_Right_executes_action_when_no_value()
        {
            string property = null;

            Maybe<T> maybe = null;

            await maybe.ExecuteNoValue(() =>
            {
                property = "Some value";
                return ValueTask.CompletedTask;
            });

            property.Should().Be("Some value");
        }
    }
}