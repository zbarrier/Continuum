using FluentAssertions;
using System;
using Xunit;


namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    public class SucceededResultTests
    {
        [Fact]
        public void Can_create_a_non_generic_version()
        {
            Result result = Result.Success();

            result.IsFailure.Should().Be(false);
            result.IsSuccess.Should().Be(true);
        }

        [Fact]
        public void Can_create_a_generic_version()
        {
            var myClass = new MyClass();

            Result<MyClass> result = Result.Success(myClass);

            result.IsFailure.Should().Be(false);
            result.IsSuccess.Should().Be(true);
            result.Value.Should().Be(myClass);
        }

        [Fact]
        public void Can_create_without_Value()
        {
            Result<MyClass> result = Result.Success((MyClass)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }

        [Fact]
        public void Cannot_access_Error_non_generic_version()
        {
            Result result = Result.Success();

            Action action = () =>
            {
                Error error = result.Error;
            };

            action.Should().Throw<ResultSuccessException>();
        }

        [Fact]
        public void Cannot_access_Error_generic_version()
        {
            Result<MyClass> result = Result.Success(new MyClass());

            Action action = () =>
            {
                Error error = result.Error;
            };

            action.Should().Throw<ResultSuccessException>();
        }

        private class MyClass
        {
        }
    }
}
