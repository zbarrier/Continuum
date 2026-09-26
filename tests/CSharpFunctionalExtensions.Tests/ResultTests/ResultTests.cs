using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    public class ResultTests : TestBase
    { 
        [Fact]
        public void Success_argument_is_null_Success_result_expected()
        {
            Result result = Result.Success<string>(null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void CreateFailure_value_is_null_Success_result_expected()
        {
            Result result = Result.FailureIf<string>(false, null, (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void CreateFailure_argument_is_false_Success_result_expected()
        {
            Result result = Result.FailureIf(false, (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void CreateFailure_argument_is_true_Failure_result_expected()
        {
            Result result = Result.FailureIf(true, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void CreateFailure_predicate_is_false_Success_result_expected()
        {
            Result result = Result.FailureIf(() => false, (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void CreateFailure_predicate_is_true_Failure_result_expected()
        {
            Result result = Result.FailureIf(() => true, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task CreateFailure_async_predicate_is_false_Success_result_expected()
        {
            Result result = await Result.FailureIf(() => Task.FromResult(false), (Error)null);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateFailure_async_predicate_is_true_Failure_result_expected()
        {
            Result result = await Result.FailureIf(() => Task.FromResult(true), ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void CreateFailure_generic_argument_is_false_Success_result_expected()
        {
            byte val = 7;
            Result<byte> result = Result.FailureIf(false, val, (Error)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(val);
        }

        [Fact]
        public void CreateFailure_generic_argument_is_true_Failure_result_expected()
        {
            double val = .56;
            Result<double> result = Result.FailureIf(true, val, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void CreateFailure_generic_predicate_is_false_Success_result_expected()
        {
            DateTime val = new DateTime(2000, 1, 1);

            Result<DateTime> result = Result.FailureIf(() => false, val, (Error)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(val);
        }

        [Fact]
        public void CreateFailure_generic_predicate_is_true_Failure_result_expected()
        {
            string val = "string value";

            Result<string> result = Result.FailureIf(() => true, val, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task CreateFailure_generic_async_predicate_is_false_Success_result_expected()
        {
            int val = 42;

            Result<int> result = await Result.FailureIf(() => Task.FromResult(false), val, (Error)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(val);
        }

        [Fact]
        public async Task CreateFailure_generic_async_predicate_is_true_Failure_result_expected()
        {
            bool val = true;

            Result<bool> result = await Result.FailureIf(() => Task.FromResult(true), val, ErrorMessage);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Can_work_with_nullable_sructs()
        {
            Result<DateTime?> result = Result.Success((DateTime?)null);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(null);
        }

        [Fact]
        public void Can_work_with_maybe_of_struct()
        {
            Maybe<DateTime> maybe = Maybe<DateTime>.None;

            Result<Maybe<DateTime>> result = Result.Success(maybe);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Maybe<DateTime>.None);
        }

        [Fact]
        public void Can_work_with_maybe_of_ref_type()
        {
            Maybe<string> maybe = Maybe<string>.None;

            Result<Maybe<string>> result = Result.Success(maybe);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Maybe<string>.None);
        }
    }
}