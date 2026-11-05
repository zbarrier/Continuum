using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultHttpExtensions
{
    /// <summary>Returns the result built from the value on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="onSuccess">Builds the success response from the value.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<TSuccess, ErrorHttpResult> ToHttpResult<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(this Result<T> result, Func<T, TSuccess> onSuccess, Action<ProblemDetails>? customizeProblemDetails = null)
        where TSuccess : Microsoft.AspNetCore.Http.IResult
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess(result.Value) : result.Error.ToHttpResult(customizeProblemDetails);
    }

    /// <inheritdoc cref="ToHttpResult{T, TSuccess}(Result{T}, Func{T, TSuccess}, Action{ProblemDetails}?)" />
    public static async Task<Results<TSuccess, ErrorHttpResult>> ToHttpResult<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(this Task<Result<T>> task, Func<T, TSuccess> onSuccess, Action<ProblemDetails>? customizeProblemDetails = null)
        where TSuccess : Microsoft.AspNetCore.Http.IResult =>
        (await task.ConfigureAwait(false)).ToHttpResult(onSuccess, customizeProblemDetails);

    /// <inheritdoc cref="ToHttpResult{T, TSuccess}(Result{T}, Func{T, TSuccess}, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<TSuccess, ErrorHttpResult>> ToHttpResult<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(this ValueTask<Result<T>> task, Func<T, TSuccess> onSuccess, Action<ProblemDetails>? customizeProblemDetails = null)
        where TSuccess : Microsoft.AspNetCore.Http.IResult =>
        (await task.ConfigureAwait(false)).ToHttpResult(onSuccess, customizeProblemDetails);

    /// <summary>
    ///     Returns the result built asynchronously from the value on success, otherwise a problem details response. Use when
    ///     building the response needs I/O, such as opening a blob stream after a grain authorizes the download.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="onSuccess">Builds the success response from the value. Not called on failure.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static async Task<Results<TSuccess, ErrorHttpResult>> ToHttpResultAsync<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(this Result<T> result, Func<T, Task<TSuccess>> onSuccess, Action<ProblemDetails>? customizeProblemDetails = null)
        where TSuccess : Microsoft.AspNetCore.Http.IResult
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? await onSuccess(result.Value).ConfigureAwait(false) : result.Error.ToHttpResult(customizeProblemDetails);
    }

    /// <inheritdoc cref="ToHttpResultAsync{T, TSuccess}(Result{T}, Func{T, Task{TSuccess}}, Action{ProblemDetails}?)" />
    public static async Task<Results<TSuccess, ErrorHttpResult>> ToHttpResultAsync<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(this Task<Result<T>> task, Func<T, Task<TSuccess>> onSuccess, Action<ProblemDetails>? customizeProblemDetails = null)
        where TSuccess : Microsoft.AspNetCore.Http.IResult =>
        await (await task.ConfigureAwait(false)).ToHttpResultAsync(onSuccess, customizeProblemDetails).ConfigureAwait(false);

    /// <inheritdoc cref="ToHttpResultAsync{T, TSuccess}(Result{T}, Func{T, Task{TSuccess}}, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<TSuccess, ErrorHttpResult>> ToHttpResultAsync<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(this ValueTask<Result<T>> task, Func<T, Task<TSuccess>> onSuccess, Action<ProblemDetails>? customizeProblemDetails = null)
        where TSuccess : Microsoft.AspNetCore.Http.IResult =>
        await (await task.ConfigureAwait(false)).ToHttpResultAsync(onSuccess, customizeProblemDetails).ConfigureAwait(false);
}
