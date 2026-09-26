using FluentAssertions;
using System;
using Xunit;


namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    public class FailedResultTests : TestBase
    {
        [Fact]
        public void Can_create_a_non_generic_version()
        {
            Result result = Result.Failure(ErrorMessage);

            result.Error.Should().Be(ErrorMessage);
            result.IsFailure.Should().Be(true);
            result.IsSuccess.Should().Be(false);
        }

        [Fact]
        public void Can_create_a_generic_version()
        {
            Result<MyClass> result = Result.Failure<MyClass>(ErrorMessage);

            result.Error.Should().Be(ErrorMessage);
            result.IsFailure.Should().Be(true);
            result.IsSuccess.Should().Be(false);
        }
        
        [Fact]
        public void Cannot_access_Value_property()
        {
            Result<MyClass> result = Result.Failure<MyClass>(ErrorMessage);

            Action action = () => { MyClass myClass = result.Value; };

            action.Should().Throw<ResultFailureException>()
                .WithMessage("You attempted to access the Value property for a failed result. " +
                    "A failed result has no Value. The error was: Error message");
        }

        [Fact]
        public void Cannot_create_without_error_message()
        {
            Action action1 = () => { Result.Failure(null); };
            Action action2 = () => { Result.Failure<MyClass>(null); };

            action1.Should().Throw<ArgumentNullException>();
            action2.Should().Throw<ArgumentNullException>();
        }


        private class MyClass
        {
        }
    }
}
