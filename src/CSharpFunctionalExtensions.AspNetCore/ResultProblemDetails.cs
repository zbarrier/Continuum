using System.Diagnostics;
using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Converts an <see cref="Error" /> into a problem details response body. Shared by the MVC and minimal API
///     extensions so both produce the same responses.
/// </summary>
public static class ResultProblemDetails
{
    /// <summary>
    ///     Creates the problem details for <paramref name="error" /> and sets any response headers it implies, such as
    ///     <c>Retry-After</c>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <param name="httpContext">The current request; supplies options, the localizer, the culture, and the trace id.</param>
    /// <param name="customizeProblemDetails">Optional per-call changes applied last.</param>
    public static ProblemDetails Create(Error error, HttpContext httpContext, Action<ProblemDetails>? customizeProblemDetails = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(httpContext);

        var services = httpContext.RequestServices;
        var options = services?.GetService<IOptions<ResultProblemDetailsOptions>>()?.Value ?? new ResultProblemDetailsOptions();
        var localizer = services?.GetService<IErrorMessageLocalizer>();
        var culture = CultureInfo.CurrentUICulture;

        ProblemDetails problemDetails;
        switch (error)
        {
            case RequestError requestError:
                problemDetails = new ProblemDetails
                {
                    Status = (int)requestError.HttpStatusCode,
                    Detail = requestError.GetFormattedMessage(culture, localizer)
                };
                problemDetails.Extensions["code"] = requestError.Code;
                if (requestError.Target is not null)
                {
                    problemDetails.Extensions["target"] = requestError.Target;
                }
                if (requestError.RetryAfter is { } retryAfter)
                {
                    var seconds = (long)Math.Ceiling(Math.Max(0, retryAfter.TotalSeconds));
                    httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                }
                break;

            case ValidationError validationError:
                var errors = validationError.Entries
                    .GroupBy(entry => entry.Target, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(entry => entry.GetFormattedMessage(culture, localizer)).ToArray(),
                        StringComparer.Ordinal);
                problemDetails = new HttpValidationProblemDetails(errors)
                {
                    Status = (int)validationError.HttpStatusCode
                };
                problemDetails.Extensions["code"] = validationError.Code;
                break;

            default:
                problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = options.UnknownErrorDetail
                };
                break;
        }

        var status = problemDetails.Status!.Value;
        if (options.Mappings.TryGetValue(status, out var mapping))
        {
            problemDetails.Title = mapping.Title;
            problemDetails.Type = mapping.Type;
        }
        else
        {
            problemDetails.Title = ReasonPhrases.GetReasonPhrase(status);
        }
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        customizeProblemDetails?.Invoke(problemDetails);
        return problemDetails;
    }
}
