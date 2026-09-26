using System;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    public class ToStringTests : TestBase
    {
        [Fact]
        public void ToString_returns_failure_with_error_when_failure()
        {
            Result subject = Result.Failure(RequestErrors.NewUnknown("BigError"));
            Assert.Equal("Failure(BigError)", subject.ToString());
        }

        [Fact]
        public void ToString_returns_failure_with_generic_result_error_when_failure()
        {
            Result<string> subject = Result.Failure<string>(RequestErrors.NewUnknown("BigError"));
            Assert.Equal("Failure(BigError)", subject.ToString());
        }

        [Fact]
        public void ToString_returns_success()
        {
            Result subject = Result.Success();
            Assert.Equal("Success", subject.ToString());
        }

        [Fact]
        public void ToString_returns_success_with_generic_result()
        {
            Result<int> subject = Result.Success(1);
            Assert.Equal("Success(1)", subject.ToString());
        }

        enum ErrorType
        {
            Error1
        }
    }
}
