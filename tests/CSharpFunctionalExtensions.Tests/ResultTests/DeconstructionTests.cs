using FluentAssertions;
using System;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    public class DeconstructionTests : TestBase
    {
        [Fact]
        public void Can_deconstruct_non_generic_Ok_to_isSuccess_and_isFailure()
        {
            var (isSuccess, isFailure) = Result.Success();

            isSuccess.Should().Be(true);
            isFailure.Should().Be(false);
        }

        [Fact]
        public void Can_deconstruct_non_generic_Fail_to_isSuccess_and_isFailure()
        {
            var (isSuccess, isFailure) = Result.Failure(ErrorMessage);

            isSuccess.Should().Be(false);
            isFailure.Should().Be(true);
        }

        [Fact]
        public void Can_deconstruct_non_generic_Ok_to_isSuccess_and_isFailure_and_error()
        {
            var (isSuccess, isFailure, error) = Result.Success();

            isSuccess.Should().Be(true);
            isFailure.Should().Be(false);
            error.Should().Be(null);
        }

        [Fact]
        public void Can_deconstruct_non_generic_Fail_to_isSuccess_and_isFailure_and_error()
        {
            var (isSuccess, isFailure, error) = Result.Failure(ErrorMessage);

            isSuccess.Should().Be(false);
            isFailure.Should().Be(true);
            error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Can_deconstruct_generic_Ok_to_isSuccess_and_isFailure()
        {
            var (isSuccess, isFailure) = Result.Success(true);

            isSuccess.Should().Be(true);
            isFailure.Should().Be(false);
        }

        [Fact]
        public void Can_deconstruct_generic_Fail_to_isSuccess_and_isFailure()
        {
            var (isSuccess, isFailure) = Result.Failure<bool>(ErrorMessage);

            isSuccess.Should().Be(false);
            isFailure.Should().Be(true);
        }

        [Fact]
        public void Can_deconstruct_generic_Ok_to_isSuccess_and_isFailure_and_value()
        {
            var (isSuccess, isFailure, value) = Result.Success(100);

            isSuccess.Should().Be(true);
            isFailure.Should().Be(false);
            value.Should().Be(100);
        }

        [Fact]
        public void Can_deconstruct_generic_Ok_to_isSuccess_and_isFailure_and_value_with_ignored_error()
        {
            var (isSuccess, isFailure, value, _) = Result.Success(100);

            isSuccess.Should().Be(true);
            isFailure.Should().Be(false);
            value.Should().Be(100);
        }

        [Fact]
        public void Can_deconstruct_generic_Ok_to_isSuccess_and_isFailure_and_error_with_ignored_value()
        {
            var (isSuccess, isFailure, _, error) = Result.Success(100);

            isSuccess.Should().Be(true);
            isFailure.Should().Be(false);
            error.Should().Be(null);
        }

        [Fact]
        public void Can_deconstruct_generic_Fail_to_isSuccess_and_isFailure_and_error_with_ignored_value()
        {
            var (isSuccess, isFailure, _, error) = Result.Failure<bool>(ErrorMessage);

            isSuccess.Should().Be(false);
            isFailure.Should().Be(true);
            error.Should().Be(ErrorMessage);
        }
    }
}
