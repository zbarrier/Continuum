using Continuum.CSharpFunctionalExtensions.Json.Serialization;

using FluentAssertions;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Xunit;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Json.Serialization
{
    public class HttpResponseMessageJsonExtensionsTests
    {
        JsonSerializerOptions _options;

        public HttpResponseMessageJsonExtensionsTests() 
        {
            _options = new JsonSerializerOptions();
            _options.AddCSharpFunctionalExtensionsConverters();
        }

        [Fact]
        public async Task ReadResultAsync_NullHttpResponseMessage_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = null;

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ReadResultAsyncOfT_NullHttpResponseMessage_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = null;

            // Act
            var result = await httpResponseMessage.ReadResultAsync<int>();

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ReadResultAsync_NullHttpResponseMessageContent_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsyncOfT_NullHttpResponseMessageContent_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            var result = await httpResponseMessage.ReadResultAsync<int>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsync_EmptyResponseMessageContent_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent(string.Empty);

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsyncOfT_EmptyResponseMessageContent_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent(string.Empty);

            // Act
            var result = await httpResponseMessage.ReadResultAsync<int>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsync_JsonNull_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create<string>(null);


            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsyncOfT_JsonNull_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create<string>(null);


            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsync_JsonInt_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(8);

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsyncOfT_JsonInt_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(8);

            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsyncOfTE_JsonInt_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(8);

            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact(Skip = "Fails when building on Linux")]
        public async Task ReadResultAsync_JsonObject_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(new object());

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact(Skip = "Fails when building on Linux")]
        public async Task ReadResultAsyncOfT_JsonObject_Failure()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(new object());

            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(DtoMessages.ContentJsonNotResult);
        }

        [Fact]
        public async Task ReadResultAsync_JsonResultDtoOfSuccess_Success()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(Result.Success(), null, _options);

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ReadResultAsyncOfT_JsonResultDtoOfSuccess_Success()
        {
            // Assign
            const string value = "Great Success";
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(Result.Success(value), null, _options);

            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(value);
        }

        [Fact]
        public async Task ReadResultAsync_JsonResultDtoOfFailure_Failure()
        {
            // Assign
            Error error = RequestErrors.NewUnknown("The response content is not a Result.");
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(Result.Failure(error));

            // Act
            var result = await httpResponseMessage.ReadResultAsync();
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public async Task ReadResultAsyncOfT_JsonResultDtoOfFailure_Failure()
        {
            // Assign
            Error error = RequestErrors.NewUnknown("The response content is not a Result.");
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = JsonContent.Create(Result.Failure<string>(error));

            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public async Task ReadResultAsync_StringResultDtoOfSuccess_Success()
        {
            // Assign
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent("{ \"IsSuccess\": true }");

            // Act
            var result = await httpResponseMessage.ReadResultAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ReadResultAsyncOfT_StringResultDtoOfSuccess_Success()
        {
            // Assign
            const string value = "Great Success";
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent($"{{ \"IsSuccess\": true, \"Value\": \"{value}\"}}");

            // Act
            var result = await httpResponseMessage.ReadResultAsync<string>();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(value);
        }
    }
}
