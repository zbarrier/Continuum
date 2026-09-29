using Continuum.CSharpFunctionalExtensions.Json.Serialization;

using FluentAssertions;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using SerializerOptions = Continuum.CSharpFunctionalExtensions.Json.Serialization.CSharpFunctionalExtensionsJsonSerializerOptions;

using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Json.Serialization
{
    public class SerializationTests : TestBase
    {
        [Fact]
        public void Result_Success()
        {
            // Assign
            var originalResult = Result.Success();

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
        }

        [Fact]
        public void Result_Failure()
        {
            // Assign
            var originalResult = Result.Failure(ErrorMessage);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Error.GetFormattedMessage().Should().Be(originalResult.Error.GetFormattedMessage());
        }

        [Fact]
        public void Result_Failure_RequestError()
        {
            // Assign
            var originalResult = Result.Failure(RequestError);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Error.GetFormattedMessage().Should().Be(originalResult.Error.GetFormattedMessage());
        }

        [Fact]
        public void Result_Failure_ValidationError()
        {
            // Assign
            var originalResult = Result.Failure(ValidationError);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Error.GetType().Should().Be(originalResult.Error.GetType());
            result.Error.ToString().Should().Be(originalResult.Error.ToString());

            var originalValidationError = (ValidationError)originalResult.Error;
            var deserializedValidationError = (ValidationError)result.Error;
            originalValidationError.Entries.Count.Should().Be(deserializedValidationError.Entries.Count);

            for (int i = 0; i < originalValidationError.Entries.Count; i++)
            {
                originalValidationError.Entries[i].GetFormattedMessage().Should().Be(deserializedValidationError.Entries[i].GetFormattedMessage());
            }
        }

        [Fact]
        public void ResultOfT_Success()
        {
            // Assign
            var originalResult = Result.Success(8);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Value.Should().Be(originalResult.Value);
        }

        [Fact]
        public void ResultOfT_Success_TestClass()
        {
            // Assign
            var testClass = new TestClass
            {
                Street1 = "123 Main St",
                Street2 = null,
                City = "Anytown",
                State = "NY",
                Zip = "12345"
            };
            var originalResult = Result.Success(testClass);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Value.Street1.Should().Be(originalResult.Value.Street1);
            result.Value.Street2.Should().Be(originalResult.Value.Street2);
            result.Value.City.Should().Be(originalResult.Value.City);
            result.Value.State.Should().Be(originalResult.Value.State);
            result.Value.Zip.Should().Be(originalResult.Value.Zip);
        }

        [Fact]
        public void ResultOfT_Failure()
        {
            // Assign
            var originalResult = Result.Failure<int>(ErrorMessage);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Error.GetFormattedMessage().Should().Be(originalResult.Error.GetFormattedMessage());
        }

        [Fact]
        public void ResultOfT_Failure_RequestError()
        {
            // Assign
            var originalResult = Result.Failure<int>(RequestError);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Error.GetFormattedMessage().Should().Be(originalResult.Error.GetFormattedMessage());
        }

        [Fact]
        public void ResultOfT_Failure_ValidationError()
        {
            // Assign
            var originalResult = Result.Failure<int>(ValidationError);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.IsSuccess.Should().Be(originalResult.IsSuccess);
            result.Error.GetType().Should().Be(originalResult.Error.GetType());
            result.Error.ToString().Should().Be(originalResult.Error.ToString());

            var originalValidationError = (ValidationError)originalResult.Error;
            var deserializedValidationError = (ValidationError)result.Error;
            originalValidationError.Entries.Count.Should().Be(deserializedValidationError.Entries.Count);

            for (int i = 0; i < originalValidationError.Entries.Count; i++)
            {
                originalValidationError.Entries[i].GetFormattedMessage().Should().Be(deserializedValidationError.Entries[i].GetFormattedMessage());
            }
        }

        [Fact]
        public void Result_Failure_RequestError_RoundTripsCode()
        {
            // Assign
            var originalResult = Result.Failure(RequestErrors.NewNotFound("{0} missing", "Item").WithCode("orders.not_found"));

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.Error.Code.Should().Be("orders.not_found");
            result.Error.Should().BeOfType<RequestError>();
        }

        [Fact]
        public void Result_Failure_ValidationError_RoundTripsCodes()
        {
            // Assign
            var originalResult = Result.Failure(ValidationError);

            // Act
            var result = SerializeAndDeserialize(originalResult);

            // Assert
            result.Error.Code.Should().Be(ErrorCodes.ValidationFailed);
            var entries = ((ValidationError)result.Error).Entries;
            entries.Select(e => e.Code).Should().Equal("code1", "code2", "code3");
        }

        private TResult SerializeAndDeserialize<TResult>(TResult result)
        {
            return JsonSerializer.Deserialize<TResult>(
                            JsonSerializer.SerializeToElement(result, SerializerOptions.Options),
                            SerializerOptions.Options);
        }

        public class TestClass
        {
            public string Street1 { get; set; }
            public string? Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
        }
    }
}
