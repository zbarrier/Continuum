using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Extensions
{
    public class EnsureTests : TestBase
    {
        [Fact]
        public void Ensure_source_result_is_failure_predicate_do_not_invoked_expect_is_result_failure()
        {
            Result sut = Result.Failure(ErrorMessage);

            Result result = sut.Ensure(() => true, ErrorMessage2);

            result.Should().Be(sut);
        }

        [Fact]
        public void Ensure_source_result_is_success_predicate_is_failed_expected_result_failure()
        {
            Result sut = Result.Success();

            Result result = sut.Ensure(() => false, ErrorMessage);

            result.Should().NotBe(sut);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_source_result_is_success_predicate_is_passed_expected_result_success()
        {
            Result sut = Result.Success();

            Result result = sut.Ensure(() => true, ErrorMessage);

            result.Should().Be(sut);
        }

        [Fact]
        public async Task Ensure_source_result_is_failure_async_predicate_do_not_invoked_expect_is_result_failure()
        {
            Result sut = Result.Failure(ErrorMessage);

            Result result = await sut.Ensure(() => Task.FromResult(true), ErrorMessage);

            result.Should().Be(sut);
        }

        [Fact]
        public async Task Ensure_source_result_is_success_async_predicate_is_failed_expected_result_failure()
        {
            Result sut = Result.Success();

            Result result = await sut.Ensure(() => Task.FromResult(false), ErrorMessage);

            result.Should().NotBe(sut);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Ensure_source_result_is_success_async_predicate_is_passed_expected_result_success()
        {
            Result sut = Result.Success();

            Result result = await sut.Ensure(() => Task.FromResult(true), ErrorMessage);

            result.Should().Be(sut);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_success_predicate_is_passed_error_predicate_is_not_invoked()
        {
            Task<Result<int?>> sut = Task.FromResult(Result.Success<int?>(null));

            Result<int?> result = await sut.Ensure(value => !value.HasValue,
                value => RequestErrors.NewUnknown("should be null but found {0}", value.Value));

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_failure_predicate_do_not_invoked_expect_is_result_failure()
        {
            Task<Result> sut = Task.FromResult(Result.Failure(ErrorMessage));

            Result result = await sut.Ensure(() => true, ErrorMessage);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_success_predicate_is_failed_expected_result_failure()
        {
            Task<Result> sut = Task.FromResult(Result.Success());

            Result result = await sut.Ensure(() => false, ErrorMessage);

            result.Should().NotBe(sut.Result);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_success_predicate_is_passed_expected_result_success()
        {
            Task<Result> sut = Task.FromResult(Result.Success());

            Result result = await sut.Ensure(() => true, ErrorMessage);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_failure_async_predicate_do_not_invoked_expect_is_result_failure()
        {
            Task<Result> sut = Task.FromResult(Result.Failure(ErrorMessage));

            Result result = await sut.Ensure(() => Task.FromResult(false), ErrorMessage2);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_success_async_predicate_is_failed_expected_result_failure()
        {
            Task<Result> sut = Task.FromResult(Result.Success());

            Result result = await sut.Ensure(() => Task.FromResult(false), ErrorMessage);

            result.Should().NotBe(sut.Result);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task Ensure_task_source_result_is_success_async_predicate_is_passed_expected_result_success()
        {
            Task<Result> sut = Task.FromResult(Result.Success());

            Result result = await sut.Ensure(() => Task.FromResult(true), ErrorMessage);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public void Ensure_generic_source_result_is_failure_predicate_do_not_invoked_expect_is_error_result_failure()
        {
            Result<TimeSpan> sut = Result.Failure<TimeSpan>(ErrorMessage);

            Result<TimeSpan> result = sut.Ensure(time => true, ErrorMessage2);

            result.Should().Be(sut);
        }

        [Fact]
        public void Ensure_generic_source_result_is_success_predicate_is_failed_expected_error_result_failure()
        {
            Result<int> sut = Result.Success(10101);

            Result<int> result = sut.Ensure(i => false, ErrorMessage);

            result.Should().NotBe(sut);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_generic_source_result_is_success_predicate_is_passed_expected_error_result_success()
        {
            Result<decimal> sut = Result.Success(.03m);

            Result<decimal> result = sut.Ensure(d => true, ErrorMessage);

            result.Should().Be(sut);
        }

        [Fact]
        public async Task
            Ensure_generic_source_result_is_failure_async_predicate_do_not_invoked_expect_is_error_result_failure()
        {
            Result<DateTimeOffset> sut = Result.Failure<DateTimeOffset>(ErrorMessage);

            Result<DateTimeOffset> result = await sut.Ensure(d => Task.FromResult(true), ErrorMessage2);

            result.Should().Be(sut);
        }

        [Fact]
        public async Task
            Ensure_generic_source_result_is_success_async_predicate_is_failed_expected_error_result_failure()
        {
            Result<int> sut = Result.Success(333);

            Result<int> result = await sut.Ensure(i => Task.FromResult(false), ErrorMessage);

            result.Should().NotBe(sut);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task
            Ensure_generic_source_result_is_success_async_predicate_is_passed_expected_error_result_success()
        {
            Result<decimal> sut = Result.Success(.33m);

            Result<decimal> result = await sut.Ensure(d => Task.FromResult(true), ErrorMessage);

            result.Should().Be(sut);
        }

        [Fact]
        public async Task
            Ensure_generic_task_source_result_is_failure_async_predicate_do_not_invoked_expect_is_error_result_failure()
        {
            Task<Result<TimeSpan>> sut = Task.FromResult(Result.Failure<TimeSpan>(ErrorMessage));

            Result<TimeSpan> result = await sut.Ensure(t => Task.FromResult(true), ErrorMessage2);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task
            Ensure_generic_task_source_result_is_success_async_predicate_is_failed_expected_error_result_failure()
        {
            Task<Result<long>> sut = Task.FromResult(Result.Success<long>(333));

            Result<long> result = await sut.Ensure(l => Task.FromResult(false), ErrorMessage);

            result.Should().NotBe(sut);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task
            Ensure_generic_task_source_result_is_success_async_predicate_is_passed_expected_error_result_success()
        {
            Task<Result<double>> sut = Task.FromResult(Result.Success(.33));

            Result<double> result = await sut.Ensure(d => Task.FromResult(true), ErrorMessage);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task
            Ensure_generic_task_source_result_is_failure_predicate_do_not_invoked_expect_is_error_result_failure()
        {
            Task<Result<TimeSpan>> sut = Task.FromResult(Result.Failure<TimeSpan>(ErrorMessage));

            Result<TimeSpan> result = await sut.Ensure(t => true, ErrorMessage2);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public async Task
            Ensure_generic_task_source_result_is_success_predicate_is_failed_expected_error_result_failure()
        {
            Task<Result<long>> sut = Task.FromResult(Result.Success<long>(333));

            Result<long> result = await sut.Ensure(l => false, ErrorMessage);

            result.Should().NotBe(sut);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public async Task
            Ensure_generic_task_source_result_is_success_predicate_is_passed_expected_error_result_success()
        {
            Task<Result<double>> sut = Task.FromResult(Result.Success(.33));

            Result<double> result = await sut.Ensure(d => true, ErrorMessage);

            result.Should().Be(sut.Result);
        }

        [Fact]
        public void Ensure_with_successInput_and_successPredicate()
        {
            var initialResult = Result.Success("Initial message");

            var result = initialResult.Ensure(() => Result.Success("Success message"));

            result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
            result.Value.Should().Be("Initial message");
        }

        [Fact]
        public void Ensure_with_successInput_and_failurePredicate()
        {
            var initialResult = Result.Success("Initial Result");

            var result = initialResult.Ensure(() => Result.Failure(ErrorMessage));

            result.IsSuccess.Should().BeFalse("Predicate is failure result");
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_with_failureInput_and_successPredicate()
        {
            var initialResult = Result.Failure(ErrorMessage);

            var result = initialResult.Ensure(() => Result.Success("Success message"));

            result.IsSuccess.Should().BeFalse("Initial result is failure result");
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_with_failureInput_and_failurePredicate()
        {
            var initialResult = Result.Failure(ErrorMessage);

            var result = initialResult.Ensure(() => Result.Failure(ErrorMessage2));

            result.IsSuccess.Should().BeFalse("Initial result is failure result");
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_with_successInput_and_parameterisedFailurePredicate()
        {
            var initialResult = Result.Success("Initial Success message");

            var result = initialResult.Ensure(_ => Result.Failure(ErrorMessage));

            result.IsSuccess.Should().BeFalse("Predicate is failure result");
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_with_successInput_and_parameterisedSuccessPredicate()
        {
            var initialResult = Result.Success("Initial Success message");

            var result = initialResult.Ensure(_ => Result.Success("Success Message"));

            result.IsSuccess.Should().BeTrue("Initial result and predicate succeeded");
            ;
            result.Value.Should().Be("Initial Success message");
        }

        [Fact]
        public void Ensure_with_failureInput_and_parameterisedSuccessPredicate()
        {
            var initialResult = Result.Failure<string>(ErrorMessage);

            var result = initialResult.Ensure(_ => Result.Success("Success Message"));

            result.IsSuccess.Should().BeFalse("Initial result is failure result");
            ;
            result.Error.Should().Be(ErrorMessage);
        }

        [Fact]
        public void Ensure_with_failureInput_and_parameterisedFailurePredicate()
        {
            var initialResult = Result.Failure<string>(ErrorMessage);

            var result = initialResult.Ensure(_ => Result.Failure(ErrorMessage2));

            result.IsSuccess.Should().BeFalse("Initial result and predicate is failure result");
            ;
            result.Error.Should().Be(ErrorMessage);
        }
    }
}
