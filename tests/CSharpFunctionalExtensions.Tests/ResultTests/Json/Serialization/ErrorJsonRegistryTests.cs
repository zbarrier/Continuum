using Continuum.CSharpFunctionalExtensions.Json.Serialization;

using FluentAssertions;

using System.Globalization;
using System.Net;
using System.Text.Json;

using Xunit;

using SerializerOptions = Continuum.CSharpFunctionalExtensions.Json.Serialization.CSharpFunctionalExtensionsJsonSerializerOptions;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Json.Serialization;

public class ErrorJsonRegistryTests
{
    [Fact]
    public void RequestError_RoundTrips_TargetRetryAfterAndStatusCodes()
    {
        var original = RequestErrors.NewUnavailable(TimeSpan.FromSeconds(30)).WithTarget("orders");

        var result = RoundTrip(Result.Failure(original));

        var error = result.Error.Should().BeOfType<RequestError>().Subject;
        error.Should().Be(original);
        error.Target.Should().Be("orders");
        error.RetryAfter.Should().Be(TimeSpan.FromSeconds(30));
        error.HttpStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        error.GrpcStatusCode.Should().Be(Grpc.Core.StatusCode.Unavailable);
    }

    [Fact]
    public void RequestError_RoundTrips_TypedArguments()
    {
        var date = new DateOnly(2024, 2, 29);
        var id = Guid.NewGuid();
        var original = RequestErrors.NewNotFound("{0} {1} {2:N2} {3} {4} {5}",
            "text", 42L, 1234.5m, true, date, id);

        var error = (RequestError)RoundTrip(Result.Failure(original)).Error;

        error.Arguments.Should().Equal(original.Arguments);
        error.Arguments.Select(a => a.Kind).Should().Equal(
            ErrorArgumentKind.String, ErrorArgumentKind.Int64, ErrorArgumentKind.Decimal,
            ErrorArgumentKind.Boolean, ErrorArgumentKind.DateOnly, ErrorArgumentKind.Guid);
        error.GetFormattedMessage(CultureInfo.GetCultureInfo("de-DE"))
            .Should().Be(original.GetFormattedMessage(CultureInfo.GetCultureInfo("de-DE")));
    }

    [Fact]
    public void Serialize_WritesStringDiscriminatorAndMessage()
    {
        var json = JsonSerializer.SerializeToElement(Result.Failure(RequestErrors.NewNotFound()), SerializerOptions.Options);

        json.GetProperty("ErrorType").GetString().Should().Be("RequestError");
        json.GetProperty("Error").GetProperty("message").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CustomError_RegisteredType_RoundTrips()
    {
        ErrorJsonTypeRegistry.Register(new CustomErrorConverter());

        var result = RoundTrip(Result.Failure(new CustomError("abc")));

        result.Error.Should().BeOfType<CustomError>().Which.Detail.Should().Be("abc");
    }

    [Fact]
    public void Register_DuplicateDiscriminatorForOtherType_Throws()
    {
        Action act = () => ErrorJsonTypeRegistry.Register(new ConflictingConverter());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Serialize_UnregisteredError_Throws()
    {
        Action act = () => JsonSerializer.Serialize(Result.Failure(new UnregisteredError()), SerializerOptions.Options);

        act.Should().Throw<NotSupportedException>();
    }

    private static Result RoundTrip(Result result) =>
        JsonSerializer.Deserialize<Result>(JsonSerializer.SerializeToElement(result, SerializerOptions.Options), SerializerOptions.Options);

    private sealed class CustomError(string detail) : Error(HttpStatusCode.BadRequest, Grpc.Core.StatusCode.FailedPrecondition, "custom")
    {
        public string Detail { get; } = detail;
        public override bool SupportsFormattedMessage => false;
        public override string ToString() => Code;
        public override string GetFormattedMessage() => throw new NotSupportedException();
        public override string GetFormattedMessage(CultureInfo culture, IErrorMessageLocalizer localizer = null) => throw new NotSupportedException();
        protected override bool EqualsCore(Error other) => Detail == ((CustomError)other).Detail;
        protected override void AddHashCodeCore(ref HashCode hash) => hash.Add(Detail);
    }

    private sealed class CustomErrorConverter() : ErrorJsonConverter<CustomError>("Tests.CustomError")
    {
        public override CustomError Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            string detail = null;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (ErrorJson.IsProperty(ref reader, "Detail")) { reader.Read(); detail = ErrorJson.ReadString(ref reader); }
                else { reader.Read(); reader.Skip(); }
            }
            return new CustomError(detail);
        }

        public override void WriteProperties(Utf8JsonWriter writer, CustomError error, JsonSerializerOptions options) =>
            writer.WriteString(ErrorJson.Name(options, "Detail"), error.Detail);
    }

    private sealed class ConflictingConverter() : ErrorJsonConverter<UnregisteredError>("RequestError")
    {
        public override UnregisteredError Read(ref Utf8JsonReader reader, JsonSerializerOptions options) => throw new NotSupportedException();
        public override void WriteProperties(Utf8JsonWriter writer, UnregisteredError error, JsonSerializerOptions options) { }
    }

    private sealed class UnregisteredError() : Error(HttpStatusCode.InternalServerError, Grpc.Core.StatusCode.Internal, "unregistered")
    {
        public override bool SupportsFormattedMessage => false;
        public override string ToString() => Code;
        public override string GetFormattedMessage() => throw new NotSupportedException();
        public override string GetFormattedMessage(CultureInfo culture, IErrorMessageLocalizer localizer = null) => throw new NotSupportedException();
        protected override bool EqualsCore(Error other) => false;
        protected override void AddHashCodeCore(ref HashCode hash) { }
    }
}
