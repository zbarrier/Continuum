#nullable enable
using System.Net.Http.Json;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization
{
    public static class HttpResponseMessageJsonExtensions
    {
        public static async Task<Result> ReadResultAsync(this HttpResponseMessage response, bool ensureSuccessStatusCode = true, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (response is null)
            {
                return Result.Failure(DtoMessages.HttpResponseMessageIsNull);
            }
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
            {
                return Result.Failure(
                    DtoMessages.NotSuccessStatusCodeFormat(
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync().DefaultAwait()
                    )
                );
            }
            return await Result
                .Try(() =>
                    response.Content.ReadFromJsonAsync<Result>(CSharpFunctionalExtensionsJsonSerializerOptions.Options, cancellationToken),
                    ex => DtoMessages.ContentJsonNotResult
                )
                .Bind(result => result)
                .DefaultAwait();
        }

        public static async Task<Result<T>> ReadResultAsync<T>(this HttpResponseMessage response, bool ensureSuccessStatusCode = true, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (response is null)
            {
                return Result.Failure<T>(DtoMessages.HttpResponseMessageIsNull);
            }
            if (ensureSuccessStatusCode && !response.IsSuccessStatusCode)
            {
                return Result.Failure<T>(
                    DtoMessages.NotSuccessStatusCodeFormat(
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync().DefaultAwait()
                    )
                );
            }
            return await Result
                .Try(() =>
                    response.Content.ReadFromJsonAsync<Result<T>>(CSharpFunctionalExtensionsJsonSerializerOptions.Options, cancellationToken),
                    ex => DtoMessages.ContentJsonNotResult
                )
                .Bind(result => result)
                .DefaultAwait();
        }
    }
}
