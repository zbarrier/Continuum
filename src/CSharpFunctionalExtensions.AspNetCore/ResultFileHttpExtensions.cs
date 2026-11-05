using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultHttpExtensions
{
    /// <summary>Returns the bytes as a file on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="contentType">The content type; defaults to <c>application/octet-stream</c>.</param>
    /// <param name="fileDownloadName">The file name for the <c>Content-Disposition</c> header.</param>
    /// <param name="enableRangeProcessing">Whether to handle range requests.</param>
    /// <param name="lastModified">When the file was last modified.</param>
    /// <param name="entityTag">The entity tag for the file.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<FileContentHttpResult, ErrorHttpResult> ToFileHttpResult(this Result<byte[]> result, string? contentType = null, string? fileDownloadName = null, bool enableRangeProcessing = false, DateTimeOffset? lastModified = null, EntityTagHeaderValue? entityTag = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess
            ? TypedResults.File(result.Value, contentType, fileDownloadName, enableRangeProcessing, lastModified, entityTag)
            : result.Error.ToHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToFileHttpResult(Result{byte[]}, string?, string?, bool, DateTimeOffset?, EntityTagHeaderValue?, Action{ProblemDetails}?)" />
    public static async Task<Results<FileContentHttpResult, ErrorHttpResult>> ToFileHttpResult(this Task<Result<byte[]>> task, string? contentType = null, string? fileDownloadName = null, bool enableRangeProcessing = false, DateTimeOffset? lastModified = null, EntityTagHeaderValue? entityTag = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToFileHttpResult(contentType, fileDownloadName, enableRangeProcessing, lastModified, entityTag, customizeProblemDetails);

    /// <inheritdoc cref="ToFileHttpResult(Result{byte[]}, string?, string?, bool, DateTimeOffset?, EntityTagHeaderValue?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<FileContentHttpResult, ErrorHttpResult>> ToFileHttpResult(this ValueTask<Result<byte[]>> task, string? contentType = null, string? fileDownloadName = null, bool enableRangeProcessing = false, DateTimeOffset? lastModified = null, EntityTagHeaderValue? entityTag = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToFileHttpResult(contentType, fileDownloadName, enableRangeProcessing, lastModified, entityTag, customizeProblemDetails);

    /// <summary>Returns the stream as a file on success, otherwise a problem details response. The stream is disposed after it is written.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="contentType">The content type; defaults to <c>application/octet-stream</c>.</param>
    /// <param name="fileDownloadName">The file name for the <c>Content-Disposition</c> header.</param>
    /// <param name="enableRangeProcessing">Whether to handle range requests; the stream must be seekable.</param>
    /// <param name="lastModified">When the file was last modified.</param>
    /// <param name="entityTag">The entity tag for the file.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<FileStreamHttpResult, ErrorHttpResult> ToFileHttpResult(this Result<Stream> result, string? contentType = null, string? fileDownloadName = null, bool enableRangeProcessing = false, DateTimeOffset? lastModified = null, EntityTagHeaderValue? entityTag = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess
            ? TypedResults.File(result.Value, contentType, fileDownloadName, lastModified, entityTag, enableRangeProcessing)
            : result.Error.ToHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToFileHttpResult(Result{Stream}, string?, string?, bool, DateTimeOffset?, EntityTagHeaderValue?, Action{ProblemDetails}?)" />
    public static async Task<Results<FileStreamHttpResult, ErrorHttpResult>> ToFileHttpResult(this Task<Result<Stream>> task, string? contentType = null, string? fileDownloadName = null, bool enableRangeProcessing = false, DateTimeOffset? lastModified = null, EntityTagHeaderValue? entityTag = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToFileHttpResult(contentType, fileDownloadName, enableRangeProcessing, lastModified, entityTag, customizeProblemDetails);

    /// <inheritdoc cref="ToFileHttpResult(Result{Stream}, string?, string?, bool, DateTimeOffset?, EntityTagHeaderValue?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<FileStreamHttpResult, ErrorHttpResult>> ToFileHttpResult(this ValueTask<Result<Stream>> task, string? contentType = null, string? fileDownloadName = null, bool enableRangeProcessing = false, DateTimeOffset? lastModified = null, EntityTagHeaderValue? entityTag = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToFileHttpResult(contentType, fileDownloadName, enableRangeProcessing, lastModified, entityTag, customizeProblemDetails);
}
