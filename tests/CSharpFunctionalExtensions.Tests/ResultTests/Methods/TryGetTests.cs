using System;

using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods
{
    public class TryGetTests
    {
        public const string ErrorMessageString = "Error from result";

        public static Error ErrorMessage = RequestErrors.NewUnknown(ErrorMessageString);

        [Fact]
        public void Simple_result_tryGetError_is_false_Success_value_expected()
        {
            Result result = Result.Success(ErrorMessage);
            result.TryGetError(out Error error).Should().BeFalse();
            error.Should().BeNull();
        }

        [Fact]
        public void Simple_result_tryGetError_is_true_Failure_value_expected()
        {
            Result result = Result.Failure(ErrorMessage);
            result.TryGetError(out Error error).Should().BeTrue();
            error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Generic_result_tryGetError_is_false_Success_value_expected()
        {
            Result<string> result = Result.Success("Success");
            result.TryGetError(out Error error).Should().BeFalse();
            error.Should().BeNull();
        }

        [Fact]
        public void Generic_result_tryGetError_is_true_Failure_value_expected()
        {
            Result<string> result = Result.Failure<string>(ErrorMessage);
            result.TryGetError(out Error error).Should().BeTrue();
            error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Generic_result_tryGetSuccess_is_false_Failure_value_expected()
        {
            Result<string> result = Result.Failure<string>(ErrorMessage);
            result.TryGetValue(out string value).Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void Generic_result_tryGetSuccess_is_true_Success_value_expected()
        {
            Result<string> result = Result.Success("Success");
            result.TryGetValue(out string value).Should().BeTrue();
            value.Should().Be("Success");
        }
    }
}