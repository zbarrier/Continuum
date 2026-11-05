using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     An ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult" /> that writes an <see cref="CSharpFunctionalExtensions.Error" /> as a problem details
///     response. The body is built with <see cref="ResultProblemDetails.Create" /> when the result executes, so it uses the
///     request's options, localizer, and culture.
/// </summary>
public sealed class ErrorHttpResult : Microsoft.AspNetCore.Http.IResult, IStatusCodeHttpResult
{
    private readonly Action<ProblemDetails>? _customizeProblemDetails;

    internal ErrorHttpResult(Error error, Action<ProblemDetails>? customizeProblemDetails)
    {
        ArgumentNullException.ThrowIfNull(error);
        Error = error;
        _customizeProblemDetails = customizeProblemDetails;
    }

    /// <summary>The error written to the response.</summary>
    public Error Error { get; }

    /// <summary>The response status code: the error's status code, or 500 for errors that are not request or validation errors.</summary>
    public int StatusCode => Error is RequestError or ValidationError ? (int)Error.HttpStatusCode : StatusCodes.Status500InternalServerError;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var problemDetails = ResultProblemDetails.Create(Error, httpContext, _customizeProblemDetails);
        return TypedResults.Problem(problemDetails).ExecuteAsync(httpContext);
    }
}
