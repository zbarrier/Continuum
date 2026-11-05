using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultHttpExtensions
{
    /// <inheritdoc cref="ToOkHttpResult(Result, Action{ProblemDetails}?)" />
    public static async Task<Results<Ok, ErrorHttpResult>> ToOkHttpResult(this Task<Result> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToOkHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToOkHttpResult(Result, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Ok, ErrorHttpResult>> ToOkHttpResult(this ValueTask<Result> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToOkHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToOkHttpResult{T}(Result{T}, Action{ProblemDetails}?)" />
    public static async Task<Results<Ok<T>, ErrorHttpResult>> ToOkHttpResult<T>(this Task<Result<T>> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToOkHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToOkHttpResult{T}(Result{T}, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Ok<T>, ErrorHttpResult>> ToOkHttpResult<T>(this ValueTask<Result<T>> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToOkHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToNoContentHttpResult(Result, Action{ProblemDetails}?)" />
    public static async Task<Results<NoContent, ErrorHttpResult>> ToNoContentHttpResult(this Task<Result> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToNoContentHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToNoContentHttpResult(Result, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<NoContent, ErrorHttpResult>> ToNoContentHttpResult(this ValueTask<Result> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToNoContentHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToNoContentHttpResult{T}(Result{T}, Action{ProblemDetails}?)" />
    public static async Task<Results<NoContent, ErrorHttpResult>> ToNoContentHttpResult<T>(this Task<Result<T>> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToNoContentHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToNoContentHttpResult{T}(Result{T}, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<NoContent, ErrorHttpResult>> ToNoContentHttpResult<T>(this ValueTask<Result<T>> task, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToNoContentHttpResult(customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedHttpResult(Result, string?, Action{ProblemDetails}?)" />
    public static async Task<Results<Created, ErrorHttpResult>> ToCreatedHttpResult(this Task<Result> task, string? uri, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedHttpResult(Result, string?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Created, ErrorHttpResult>> ToCreatedHttpResult(this ValueTask<Result> task, string? uri, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedHttpResult{T}(Result{T}, string?, Action{ProblemDetails}?)" />
    public static async Task<Results<Created<T>, ErrorHttpResult>> ToCreatedHttpResult<T>(this Task<Result<T>> task, string? uri, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedHttpResult{T}(Result{T}, string?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Created<T>, ErrorHttpResult>> ToCreatedHttpResult<T>(this ValueTask<Result<T>> task, string? uri, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedHttpResult{T}(Result{T}, Func{T, string?}, Action{ProblemDetails}?)" />
    public static async Task<Results<Created<T>, ErrorHttpResult>> ToCreatedHttpResult<T>(this Task<Result<T>> task, Func<T, string?> uriSelector, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedHttpResult(uriSelector, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedHttpResult{T}(Result{T}, Func{T, string?}, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Created<T>, ErrorHttpResult>> ToCreatedHttpResult<T>(this ValueTask<Result<T>> task, Func<T, string?> uriSelector, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedHttpResult(uriSelector, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedAtRouteHttpResult(Result, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async Task<Results<CreatedAtRoute, ErrorHttpResult>> ToCreatedAtRouteHttpResult(this Task<Result> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedAtRouteHttpResult(Result, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<CreatedAtRoute, ErrorHttpResult>> ToCreatedAtRouteHttpResult(this ValueTask<Result> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedAtRouteHttpResult{T}(Result{T}, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async Task<Results<CreatedAtRoute<T>, ErrorHttpResult>> ToCreatedAtRouteHttpResult<T>(this Task<Result<T>> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedAtRouteHttpResult{T}(Result{T}, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<CreatedAtRoute<T>, ErrorHttpResult>> ToCreatedAtRouteHttpResult<T>(this ValueTask<Result<T>> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedAtRouteHttpResult{T}(Result{T}, string?, Func{T, RouteValueDictionary?}, Action{ProblemDetails}?)" />
    public static async Task<Results<CreatedAtRoute<T>, ErrorHttpResult>> ToCreatedAtRouteHttpResult<T>(this Task<Result<T>> task, string? routeName, Func<T, RouteValueDictionary?> routeValuesSelector, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedAtRouteHttpResult(routeName, routeValuesSelector, customizeProblemDetails);

    /// <inheritdoc cref="ToCreatedAtRouteHttpResult{T}(Result{T}, string?, Func{T, RouteValueDictionary?}, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<CreatedAtRoute<T>, ErrorHttpResult>> ToCreatedAtRouteHttpResult<T>(this ValueTask<Result<T>> task, string? routeName, Func<T, RouteValueDictionary?> routeValuesSelector, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToCreatedAtRouteHttpResult(routeName, routeValuesSelector, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedHttpResult(Result, string?, Action{ProblemDetails}?)" />
    public static async Task<Results<Accepted, ErrorHttpResult>> ToAcceptedHttpResult(this Task<Result> task, string? uri = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedHttpResult(Result, string?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Accepted, ErrorHttpResult>> ToAcceptedHttpResult(this ValueTask<Result> task, string? uri = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedHttpResult{T}(Result{T}, string?, Action{ProblemDetails}?)" />
    public static async Task<Results<Accepted<T>, ErrorHttpResult>> ToAcceptedHttpResult<T>(this Task<Result<T>> task, string? uri = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedHttpResult{T}(Result{T}, string?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<Accepted<T>, ErrorHttpResult>> ToAcceptedHttpResult<T>(this ValueTask<Result<T>> task, string? uri = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedHttpResult(uri, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedAtRouteHttpResult(Result, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async Task<Results<AcceptedAtRoute, ErrorHttpResult>> ToAcceptedAtRouteHttpResult(this Task<Result> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedAtRouteHttpResult(Result, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<AcceptedAtRoute, ErrorHttpResult>> ToAcceptedAtRouteHttpResult(this ValueTask<Result> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedAtRouteHttpResult{T}(Result{T}, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async Task<Results<AcceptedAtRoute<T>, ErrorHttpResult>> ToAcceptedAtRouteHttpResult<T>(this Task<Result<T>> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);

    /// <inheritdoc cref="ToAcceptedAtRouteHttpResult{T}(Result{T}, string?, RouteValueDictionary?, Action{ProblemDetails}?)" />
    public static async ValueTask<Results<AcceptedAtRoute<T>, ErrorHttpResult>> ToAcceptedAtRouteHttpResult<T>(this ValueTask<Result<T>> task, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        (await task.ConfigureAwait(false)).ToAcceptedAtRouteHttpResult(routeName, routeValues, customizeProblemDetails);
}
