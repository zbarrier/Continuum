using System.Net;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Serialization;

using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Continuum.CSharpFunctionalExtensions.Orleans.Tests;

public sealed class SurrogateRoundTripTests
{
    private static readonly Serializer Serializer = new ServiceCollection()
        .AddSerializer()
        .BuildServiceProvider()
        .GetRequiredService<Serializer>();

    private static T RoundTrip<T>(T value) => Serializer.Deserialize<T>(Serializer.SerializeToArray(value))!;

    [Fact]
    public void Result_Success_RoundTrips()
    {
        var result = RoundTrip(Result.Success());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Result_Failure_RoundTripsError()
    {
        var error = RequestErrors.NewNotFound("Order {0} was not found.", 42);

        var result = RoundTrip(Result.Failure(error));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultT_Success_RoundTripsValue()
    {
        var result = RoundTrip(Result.Success("hello"));

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ResultT_Failure_RoundTripsValidationError()
    {
        var error = new ValidationError("Name", ValidationErrorCodes.NotEmpty, "");

        var result = RoundTrip(Result.Failure<int>(error));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Maybe_WithValue_RoundTrips()
    {
        var maybe = RoundTrip(Maybe<int>.From(7));

        Assert.True(maybe.HasValue);
        Assert.Equal(7, maybe.Value);
    }

    [Fact]
    public void Maybe_None_RoundTrips()
    {
        var maybe = RoundTrip(Maybe<string>.None);

        Assert.True(maybe.HasNoValue);
    }

    [Fact]
    public void RequestError_RoundTripsAllFields()
    {
        var error = new RequestError(HttpStatusCode.TooManyRequests, GrpcStatusCode.ResourceExhausted, ErrorCodes.ResourceExhausted,
            "Retry in {0}.", [TimeSpan.FromSeconds(30)], target: "orders", retryAfter: TimeSpan.FromSeconds(30));

        var copy = RoundTrip(error);

        Assert.Equal(error, copy);
        Assert.Equal("orders", copy.Target);
        Assert.Equal(TimeSpan.FromSeconds(30), copy.RetryAfter);
        Assert.Equal(error.PriorityCode, copy.PriorityCode);
    }

    [Fact]
    public void RequestError_WithoutOptionalFields_RoundTrips()
    {
        var error = RequestErrors.NewUnknown("An unexpected error occurred.");

        var copy = RoundTrip(error);

        Assert.Equal(error, copy);
        Assert.Null(copy.Target);
        Assert.Null(copy.RetryAfter);
    }

    [Fact]
    public void ValidationError_RoundTripsAllEntries()
    {
        var error = new ValidationError(
        [
            new ValidationErrorEntry(ValidationSeverity.Error, "Name", ValidationErrorCodes.NotEmpty, null, ["Name"]),
            new ValidationErrorEntry(ValidationSeverity.Warning, "Age", "age_range", "{0} should be between {1} and {2}.", ["Age", 18, 99]),
        ]);

        var copy = RoundTrip(error);

        Assert.Equal(error, copy);
        Assert.Equal(2, copy.Entries.Length);
        Assert.Equal(ValidationSeverity.Warning, copy.Entries[1].Severity);
    }

    public static TheoryData<ErrorArgument> Arguments() =>
    [
        ErrorArgument.Null,
        ErrorArgument.From("text"),
        ErrorArgument.From(long.MinValue),
        ErrorArgument.From(1.0 / 3.0),
        ErrorArgument.From(double.NaN),
        ErrorArgument.From(123.4500m),
        ErrorArgument.From(true),
        ErrorArgument.From(new DateTime(2024, 2, 29, 13, 14, 15, 123, DateTimeKind.Utc)),
        ErrorArgument.From(new DateTimeOffset(2024, 2, 29, 13, 14, 15, TimeSpan.FromHours(-5))),
        ErrorArgument.From(TimeSpan.FromMilliseconds(90061001)),
        ErrorArgument.From(Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e")),
        ErrorArgument.From(new DateOnly(2024, 2, 29)),
        ErrorArgument.From(new TimeOnly(23, 59, 59, 999)),
        ErrorArgument.From(BigInteger.Parse("123456789012345678901234567890")),
    ];

    [Theory]
    [MemberData(nameof(Arguments))]
    public void ErrorArgument_RoundTripsKindAndValue(ErrorArgument argument)
    {
        var copy = RoundTrip(argument);

        Assert.Equal(argument.Kind, copy.Kind);
        Assert.Equal(argument, copy);
    }

    [Fact]
    public void GlobalGrainErrorHandler_Timeout_MapsToDeadlineExceeded()
    {
        var error = GlobalGrainErrorHandler.Default(new TimeoutException(), NullLogger.Instance);

        Assert.Equal(GrpcStatusCode.DeadlineExceeded, error.GrpcStatusCode);
    }

    [Fact]
    public void GlobalGrainErrorHandler_Other_MapsToUnknown()
    {
        var error = GlobalGrainErrorHandler.Default(new InvalidOperationException(), NullLogger.Instance);

        Assert.Equal(GrpcStatusCode.Unknown, error.GrpcStatusCode);
    }
}
