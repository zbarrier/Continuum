#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization
{
    /// <summary>
    ///     Extension methods for reading serialized results from <see cref="HttpResponseMessage"/> content.
    /// </summary>
    /// <remarks>
    ///     The overloads without serializer metadata use <see cref="CSharpFunctionalExtensionsJsonSerializerOptions.Options"/> and
    ///     reflection-based serialization. For trimming and Native AOT, use the overloads that take
    ///     <see cref="JsonSerializerOptions"/> or <see cref="JsonTypeInfo{T}"/>.
    /// </remarks>
    public static class HttpResponseMessageJsonExtensions
    {
        private const string DynamicCodeMessage =
            "Uses reflection-based JSON serialization. For Native AOT, use the overload that takes JsonSerializerOptions or JsonTypeInfo.";

        /// <summary>
        ///     Reads a serialized <see cref="Result"/> from the response content.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="ensureSuccessStatusCode">When <see langword="true"/>, a non-success status code produces a failure containing the status code and response body.</param>
        /// <param name="cancellationToken">A token to cancel the read.</param>
        /// <returns>The deserialized result, or a failure if the response is null, unsuccessful, or does not contain a result.</returns>
        [RequiresUnreferencedCode(DynamicCodeMessage)]
        [RequiresDynamicCode(DynamicCodeMessage)]
        public static Task<Result> ReadResultAsync(this HttpResponseMessage response, bool ensureSuccessStatusCode = true, 
            CancellationToken cancellationToken = default(CancellationToken))
            => ReadResultAsync(response, CSharpFunctionalExtensionsJsonSerializerOptions.Options, ensureSuccessStatusCode, cancellationToken);

        /// <summary>
        ///     Reads a serialized <see cref="Result"/> from the response content using <paramref name="options"/>. Native AOT safe
        ///     when <paramref name="options"/> has a source-generated type info resolver and the <see cref="Result"/> converter registered.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="options">Serializer options with the <see cref="Result"/> converter registered.</param>
        /// <param name="ensureSuccessStatusCode">When <see langword="true"/>, a non-success status code produces a failure containing the status code and response body.</param>
        /// <param name="cancellationToken">A token to cancel the read.</param>
        /// <returns>The deserialized result, or a failure if the response is null, unsuccessful, or does not contain a result.</returns>
        public static async Task<Result> ReadResultAsync(this HttpResponseMessage response, JsonSerializerOptions options,
            bool ensureSuccessStatusCode = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentNullException.ThrowIfNull(options);
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
                    response.Content.ReadFromJsonAsync((JsonTypeInfo<Result>)options.GetTypeInfo(typeof(Result)), cancellationToken),
                    ex => DtoMessages.ContentJsonNotResult
                )
                .Bind(result => result)
                .DefaultAwait();
        }

        /// <summary>
        ///     Reads a serialized <see cref="Result{T}"/> from the response content.
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <param name="response">The HTTP response.</param>
        /// <param name="ensureSuccessStatusCode">When <see langword="true"/>, a non-success status code produces a failure containing the status code and response body.</param>
        /// <param name="cancellationToken">A token to cancel the read.</param>
        /// <returns>The deserialized result, or a failure if the response is null, unsuccessful, or does not contain a result.</returns>
        [RequiresUnreferencedCode(DynamicCodeMessage)]
        [RequiresDynamicCode(DynamicCodeMessage)]
        public static Task<Result<T>> ReadResultAsync<T>(this HttpResponseMessage response, bool ensureSuccessStatusCode = true, 
            CancellationToken cancellationToken = default(CancellationToken))
            => ReadResultAsync(response,
                (JsonTypeInfo<Result<T>>)CSharpFunctionalExtensionsJsonSerializerOptions.Options.GetTypeInfo(typeof(Result<T>)),
                ensureSuccessStatusCode, cancellationToken);

        /// <summary>
        ///     Reads a serialized <see cref="Result{T}"/> from the response content using <paramref name="typeInfo"/>. Native AOT safe.
        /// </summary>
        /// <typeparam name="T">The type of the success value.</typeparam>
        /// <param name="response">The HTTP response.</param>
        /// <param name="typeInfo">Metadata for <see cref="Result{T}"/>, for example from a source-generated <c>JsonSerializerContext</c>
        ///     whose options register the <see cref="Result{T}"/> converter.</param>
        /// <param name="ensureSuccessStatusCode">When <see langword="true"/>, a non-success status code produces a failure containing the status code and response body.</param>
        /// <param name="cancellationToken">A token to cancel the read.</param>
        /// <returns>The deserialized result, or a failure if the response is null, unsuccessful, or does not contain a result.</returns>
        public static async Task<Result<T>> ReadResultAsync<T>(this HttpResponseMessage response, JsonTypeInfo<Result<T>> typeInfo,
            bool ensureSuccessStatusCode = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentNullException.ThrowIfNull(typeInfo);
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
                    response.Content.ReadFromJsonAsync(typeInfo, cancellationToken),
                    ex => DtoMessages.ContentJsonNotResult
                )
                .Bind(result => result)
                .DefaultAwait();
        }
    }
}
