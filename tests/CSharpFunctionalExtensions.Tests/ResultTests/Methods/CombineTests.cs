using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests
{
    [Collection(NonParallelTestCollectionDefinition.Name)]
    public class CombineTests : TestBase
    {
        const string FirstNameProperty = "FirstName";
        const string LastNameProperty = "LastName";

        const string MultiPartPropertyNameFormat = "{0} {1}";

        const string ValidationErrorFormat = "'{0}' must not be empty.";

        static readonly Exception CombineNotSupportedException = new NotSupportedException("Only validation errors can be combined.");

        [Fact]
        public void Combine_ResultWithoutErrors_Success()
        {
            //Arrange
            Result result1 = Result.Success();
            Result result2 = Result.Success();
            Result result3 = Result.Success();

            //Act
            Result result = Result.Combine(result1, result2, result3);

            //Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Combine_ResultWithErrors_Success()
        {
            //Arrange
            Result result1 = Result.Success();
            Result result2 = Result.Failure(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
            Result result3 = Result.Failure(RequestErrors.NewUnknown("Unknown error"));

            //Act
            Result result = Result.Combine(result1, result2, result3);

            //Assert
            result.IsSuccess.Should().BeFalse();

            var apiError = result.Error as RequestError;
            Assert.NotNull(apiError);
            Assert.Equal(ErrorPriorityCode.UNKNOWN, apiError.PriorityCode);
            Assert.Equal((int)HttpStatusCode.InternalServerError, apiError.HttpStatusCode);
            Assert.Equal((int)Grpc.Core.StatusCode.Unknown, apiError.GrpcStatusCode);
            Assert.Equal("Unknown error", apiError.GetFormattedMessage());
        }

        [Fact]
        public void Combine_ResultWithoutErrorsEnumerable_Success()
        {
            //Arrange
            Result result1 = Result.Success();
            Result result2 = Result.Success();
            Result result3 = Result.Success();
            var results = new List<Result> { result1, result2, result3 };

            //Act
            Result result = Result.Combine(results.AsEnumerable());

            //Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Combine_ResultWithErrorsEnumerable_Success()
        {
            //Arrange
            Result result1 = Result.Success();
            Result result2 = Result.Failure(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
            Result result3 = Result.Failure(RequestErrors.NewUnknown("Unknown error"));
            var results = new List<Result> { result1, result2, result3 };

            //Act
            Result result = Result.Combine(results.AsEnumerable());

            //Assert
            result.IsSuccess.Should().BeFalse();

            var apiError = result.Error as RequestError;
            Assert.NotNull(apiError);
            Assert.Equal(ErrorPriorityCode.UNKNOWN, apiError.PriorityCode);
            Assert.Equal((int)HttpStatusCode.InternalServerError, apiError.HttpStatusCode);
            Assert.Equal((int)Grpc.Core.StatusCode.Unknown, apiError.GrpcStatusCode);
            Assert.Equal("Unknown error", apiError.GetFormattedMessage());
        }

        [Fact]
        public void Combine_ResultTWithoutErrors_Success()
        {
            //Arrange
            var result1 = Result.Success("test");
            var result2 = Result.Success("test");
            var result3 = Result.Success("test");

            //Act
            Result result = Result.Combine(result1, result2, result3);

            //Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Combine_ResultTWithErrors_Success()
        {
            //Arrange
            var result1 = Result.Success("test");
            var result2 = Result.Failure<string>(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
            var result3 = Result.Failure<string>(RequestErrors.NewUnknown("Unknown error"));

            //Act
            Result result = Result.Combine(result1, result2, result3);

            //Assert
            result.IsSuccess.Should().BeFalse();

            var apiError = result.Error as RequestError;
            Assert.NotNull(apiError);
            Assert.Equal(ErrorPriorityCode.UNKNOWN, apiError.PriorityCode);
            Assert.Equal((int)HttpStatusCode.InternalServerError, apiError.HttpStatusCode);
            Assert.Equal((int)Grpc.Core.StatusCode.Unknown, apiError.GrpcStatusCode);
            Assert.Equal("Unknown error", apiError.GetFormattedMessage());
        }

        [Fact]
        public void Combine_ResultTWithoutErrorsEnumerable_Success()
        {
            //Arrange
            var result1 = Result.Success("test");
            var result2 = Result.Success("test");
            var result3 = Result.Success("test");
            var results = new List<Result<string>> { result1, result2, result3 };

            //Act
            Result result = Result.Combine(results.AsEnumerable());

            //Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Combine_ResultTWithErrorsEnumerable_Success()
        {
            //Arrange
            var result1 = Result.Success("test");
            var result2 = Result.Failure<string>(RequestErrors.NewInvalidArg("'{0}' must not be empty.", FirstNameProperty));
            var result3 = Result.Failure<string>(RequestErrors.NewUnknown("Unknown error"));
            var results = new List<Result<string>> { result1, result2, result3 };

            //Act
            Result result = Result.Combine(results.AsEnumerable());

            //Assert
            result.IsSuccess.Should().BeFalse();

            var apiError = result.Error as RequestError;
            Assert.NotNull(apiError);
            Assert.Equal(ErrorPriorityCode.UNKNOWN, apiError.PriorityCode);
            Assert.Equal((int)HttpStatusCode.InternalServerError, apiError.HttpStatusCode);
            Assert.Equal((int)Grpc.Core.StatusCode.Unknown, apiError.GrpcStatusCode);
            Assert.Equal("Unknown error", apiError.GetFormattedMessage());
        }
    }
}
