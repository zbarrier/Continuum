using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Converts a <see cref="Result" /> or <see cref="Result{T}" /> into a typed <see cref="IResult" /> for minimal APIs and
///     controllers. Success maps to the named result; failure maps to an <see cref="ErrorHttpResult" /> problem details
///     response.
/// </summary>
public static partial class ResultHttpExtensions
{
    /// <summary>Converts the error into a problem details result.</summary>
    /// <param name="error">The error to write.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details.</param>
    public static ErrorHttpResult ToHttpResult(this Error error, Action<ProblemDetails>? customizeProblemDetails = null) =>
        new(error, customizeProblemDetails);

    /// <summary>Returns <c>200 OK</c> on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Ok, ErrorHttpResult> ToOkHttpResult(this Result result, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.Ok() : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>200 OK</c> with the value on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Ok<T>, ErrorHttpResult> ToOkHttpResult<T>(this Result<T> result, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>204 No Content</c> on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<NoContent, ErrorHttpResult> ToNoContentHttpResult(this Result result, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Discards the value and returns <c>204 No Content</c> on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<NoContent, ErrorHttpResult> ToNoContentHttpResult<T>(this Result<T> result, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>201 Created</c> with a <c>Location</c> header on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="uri">The location of the created resource.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Created, ErrorHttpResult> ToCreatedHttpResult(this Result result, string? uri, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.Created(uri) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>201 Created</c> with the value and a <c>Location</c> header on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="uri">The location of the created resource.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Created<T>, ErrorHttpResult> ToCreatedHttpResult<T>(this Result<T> result, string? uri, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.Created(uri, result.Value) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>201 Created</c> with the value and a <c>Location</c> header built from the value on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="uriSelector">Builds the location of the created resource from the value, for example from its id.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Created<T>, ErrorHttpResult> ToCreatedHttpResult<T>(this Result<T> result, Func<T, string?> uriSelector, Action<ProblemDetails>? customizeProblemDetails = null)
    {
        ArgumentNullException.ThrowIfNull(uriSelector);
        return result.IsSuccess ? TypedResults.Created(uriSelector(result.Value), result.Value) : result.Error.ToHttpResult(customizeProblemDetails);
    }

    /// <summary>Returns <c>201 Created</c> with a <c>Location</c> header for the named route on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route that locates the created resource.</param>
    /// <param name="routeValues">The values for the route.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<CreatedAtRoute, ErrorHttpResult> ToCreatedAtRouteHttpResult(this Result result, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.CreatedAtRoute(routeName, routeValues) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>201 Created</c> with the value and a <c>Location</c> header for the named route on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route that locates the created resource.</param>
    /// <param name="routeValues">The values for the route.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<CreatedAtRoute<T>, ErrorHttpResult> ToCreatedAtRouteHttpResult<T>(this Result<T> result, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.CreatedAtRoute(result.Value, routeName, routeValues) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>201 Created</c> with the value and a <c>Location</c> header for the named route, with route values built from the value, on success; otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route that locates the created resource.</param>
    /// <param name="routeValuesSelector">Builds the route values from the value, for example from its id.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<CreatedAtRoute<T>, ErrorHttpResult> ToCreatedAtRouteHttpResult<T>(this Result<T> result, string? routeName, Func<T, RouteValueDictionary?> routeValuesSelector, Action<ProblemDetails>? customizeProblemDetails = null)
    {
        ArgumentNullException.ThrowIfNull(routeValuesSelector);
        return result.IsSuccess ? TypedResults.CreatedAtRoute(result.Value, routeName, routeValuesSelector(result.Value)) : result.Error.ToHttpResult(customizeProblemDetails);
    }

    /// <summary>Returns <c>202 Accepted</c> with an optional <c>Location</c> header on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="uri">The location where the status of the work can be monitored.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Accepted, ErrorHttpResult> ToAcceptedHttpResult(this Result result, string? uri = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.Accepted(uri) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>202 Accepted</c> with the value and an optional <c>Location</c> header on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="uri">The location where the status of the work can be monitored.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<Accepted<T>, ErrorHttpResult> ToAcceptedHttpResult<T>(this Result<T> result, string? uri = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.Accepted(uri, result.Value) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>202 Accepted</c> with a <c>Location</c> header for the named route on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route where the status of the work can be monitored.</param>
    /// <param name="routeValues">The values for the route.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<AcceptedAtRoute, ErrorHttpResult> ToAcceptedAtRouteHttpResult(this Result result, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.AcceptedAtRoute(routeName, routeValues) : result.Error.ToHttpResult(customizeProblemDetails);

    /// <summary>Returns <c>202 Accepted</c> with the value and a <c>Location</c> header for the named route on success, otherwise a problem details response.</summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route where the status of the work can be monitored.</param>
    /// <param name="routeValues">The values for the route.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes to the problem details on failure.</param>
    public static Results<AcceptedAtRoute<T>, ErrorHttpResult> ToAcceptedAtRouteHttpResult<T>(this Result<T> result, string? routeName, RouteValueDictionary? routeValues = null, Action<ProblemDetails>? customizeProblemDetails = null) =>
        result.IsSuccess ? TypedResults.AcceptedAtRoute(result.Value, routeName, routeValues) : result.Error.ToHttpResult(customizeProblemDetails);
}
