using System.Net;

using Continuum.CSharpFunctionalExtensions;

using GetRDone.Orders.Abstractions;

using Grpc.Core;

namespace GetRDone.Orders.Grains.Tests.Automations;

/// <summary>
///     The automations' work is required, so they skip an event only when it can never be handled and retry every other
///     failure, including unexpected and permission errors.
/// </summary>
public sealed class AutomationErrorsTests
{
    [Theory]
    [InlineData(StatusCode.InvalidArgument, false)]
    [InlineData(StatusCode.OutOfRange, false)]
    [InlineData(StatusCode.NotFound, false)]
    [InlineData(StatusCode.FailedPrecondition, false)]
    [InlineData(StatusCode.AlreadyExists, false)]
    [InlineData(StatusCode.Unknown, true)]
    [InlineData(StatusCode.Internal, true)]
    [InlineData(StatusCode.PermissionDenied, true)]
    [InlineData(StatusCode.Unauthenticated, true)]
    [InlineData(StatusCode.Unavailable, true)]
    [InlineData(StatusCode.DeadlineExceeded, true)]
    [InlineData(StatusCode.Aborted, true)]
    [InlineData(StatusCode.ResourceExhausted, true)]
    public void IsRetryable_IsFalseOnlyWhenTheEventCanNeverBeHandled(StatusCode statusCode, bool isRetryable)
        => Assert.Equal(isRetryable, AutomationErrors.IsRetryable(RequestErrors.New(HttpStatusCode.BadRequest, statusCode)));
}
