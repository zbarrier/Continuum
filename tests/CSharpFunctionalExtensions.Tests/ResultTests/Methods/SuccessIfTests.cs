using FluentAssertions;
using System.Threading.Tasks;
using System;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Methods
{
    public class SuccessIfTests : TestBase
    {
        [Fact]
        public void Create_value_is_null_Success_result_expected()
        {
            Result result = Result.SuccessIf(true, 7, (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_error_is_null_Exception_expected()
        {
            Action resultAction = () =>
                Result.SuccessIf<int>(false, 7, (Error)null);

            FluentActions.Invoking(resultAction).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Create_argument_is_true_Success_result_expected()
        {
            Result result = Result.SuccessIf(true, (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_argument_is_false_Failure_result_expected()
        {
            Result result = Result.SuccessIf(false, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Create_predicate_is_true_Success_result_expected()
        {
            Result result = Result.SuccessIf(() => true, (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Create_predicate_is_false_Failure_result_expected()
        {
            Result result = Result.SuccessIf(() => false, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Create_async_predicate_is_true_Success_result_expected()
        {
            Result result = await Result.SuccessIf(() => Task.FromResult(true), (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Create_async_predicate_is_false_Failure_result_expected()
        {
            Result result = await Result.SuccessIf(() => Task.FromResult(false), ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Create_generic_argument_is_true_Success_result_expected()
        {
            byte val = 7;
            Result<byte> result = Result.SuccessIf(true, val, (Error)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(val);
        }

        [Fact]
        public void Create_generic_argument_is_false_Failure_result_expected()
        {
            double val = .56;
            Result<double> result = Result.SuccessIf(false, val, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Create_generic_predicate_is_true_Success_result_expected()
        {
            DateTime val = new DateTime(2000, 1, 1);

            Result<DateTime> result = Result.SuccessIf(() => true, val, (Error)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(val);
        }

        [Fact]
        public void Create_generic_predicate_is_false_Failure_result_expected()
        {
            string val = "string value";

            Result<string> result = Result.SuccessIf(() => false, val, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Create_generic_async_predicate_is_true_Success_result_expected()
        {
            int val = 42;

            Result<int> result = await Result.SuccessIf(() => Task.FromResult(true), val, (Error)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(val);
        }

        [Fact]
        public async Task Create_generic_async_predicate_is_false_Failure_result_expected()
        {
            bool val = true;

            Result<bool> result = await Result.SuccessIf(() => Task.FromResult(false), val, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }
    }
}
