using System;

using FluentAssertions;

using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Errors
{
    public class ErrorCodeTests
    {
        [Fact]
        public void RequestErrors_Factories_UseDefaultCodes()
        {
            RequestErrors.NewAlreadyExists("x").Code.Should().Be(ErrorCodes.AlreadyExists);
            RequestErrors.NewDeadlineExceeded("x").Code.Should().Be(ErrorCodes.DeadlineExceeded);
            RequestErrors.NewFailedPrecondition("x").Code.Should().Be(ErrorCodes.FailedPrecondition);
            RequestErrors.NewInvalidArg("x").Code.Should().Be(ErrorCodes.InvalidArgument);
            RequestErrors.NewNotFound("x").Code.Should().Be(ErrorCodes.NotFound);
            RequestErrors.NewNotImplemented().Code.Should().Be(ErrorCodes.Unimplemented);
            RequestErrors.NewUnavailable("x").Code.Should().Be(ErrorCodes.Unavailable);
            RequestErrors.NewUnknown("x").Code.Should().Be(ErrorCodes.Unknown);
        }

        [Fact]
        public void RequestErrors_New_DerivesCodeFromGrpcStatus()
        {
            var error = RequestErrors.New(System.Net.HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied);

            error.Code.Should().Be(ErrorCodes.PermissionDenied);
        }

        [Fact]
        public void RequestError_WithCode_ReturnsCopyWithNewCode()
        {
            var original = RequestErrors.NewNotFound("{0} missing", "Order");

            var custom = original.WithCode("orders.not_found");

            custom.Code.Should().Be("orders.not_found");
            custom.GetFormattedMessage().Should().Be(original.GetFormattedMessage());
            custom.Should().NotBe(original);
            original.Code.Should().Be(ErrorCodes.NotFound);
        }

        [Fact]
        public void RequestError_EmptyCode_Throws()
        {
            Action act = () => new RequestError(System.Net.HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Unknown, " ");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ValidationError_HasFixedCode_AndEntryCode()
        {
            Error error = new ValidationError("Name", ExpectedValidationErrorCodes.NotEmpty, ExpectedValidatorErrorStrings.NotEmpty, "Name");

            error.Code.Should().Be(ErrorCodes.ValidationFailed);
            ((ValidationError)error).Entries[0].Code.Should().Be(ExpectedValidationErrorCodes.NotEmpty);
        }

        [Fact]
        public void ValidationErrorEntry_DifferentCodes_AreNotEqual()
        {
            var a = new ValidationErrorEntry(ValidationSeverity.Error, "Name", "a", "{0}", ["x"]);
            var b = new ValidationErrorEntry(ValidationSeverity.Error, "Name", "b", "{0}", ["x"]);

            a.Should().NotBe(b);
        }

        [Fact]
        public void Validator_EmitsEntryCode()
        {
            var result = Result.NotNullOrEmpty((string)null, "Name");

            ((ValidationError)result.Error).Entries[0].Code.Should().Be(ExpectedValidationErrorCodes.NotEmpty);
        }
    }
}
