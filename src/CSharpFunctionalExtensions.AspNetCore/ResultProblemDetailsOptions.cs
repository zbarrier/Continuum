namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     The <c>title</c> and <c>type</c> written to a problem details response for a status code.
/// </summary>
/// <param name="Title">A short, human-readable summary of the problem type.</param>
/// <param name="Type">A URI reference that identifies the problem type.</param>
public sealed record ProblemDetailsMapping(string? Title, string? Type);

/// <summary>
///     Options for converting a failed <see cref="Result" /> into a problem details response. Configure with
///     <c>services.Configure&lt;ResultProblemDetailsOptions&gt;(...)</c>.
/// </summary>
public sealed class ResultProblemDetailsOptions
{
    /// <summary>
    ///     The <c>title</c> and <c>type</c> for each status code. Status codes without a mapping use the standard reason
    ///     phrase as the title and no type. Defaults follow RFC 9110.
    /// </summary>
    public IDictionary<int, ProblemDetailsMapping> Mappings { get; } = new Dictionary<int, ProblemDetailsMapping>
    {
        [400] = new("Bad Request", "https://tools.ietf.org/html/rfc9110#section-15.5.1"),
        [401] = new("Unauthorized", "https://tools.ietf.org/html/rfc9110#section-15.5.2"),
        [403] = new("Forbidden", "https://tools.ietf.org/html/rfc9110#section-15.5.4"),
        [404] = new("Not Found", "https://tools.ietf.org/html/rfc9110#section-15.5.5"),
        [405] = new("Method Not Allowed", "https://tools.ietf.org/html/rfc9110#section-15.5.6"),
        [406] = new("Not Acceptable", "https://tools.ietf.org/html/rfc9110#section-15.5.7"),
        [408] = new("Request Timeout", "https://tools.ietf.org/html/rfc9110#section-15.5.9"),
        [409] = new("Conflict", "https://tools.ietf.org/html/rfc9110#section-15.5.10"),
        [412] = new("Precondition Failed", "https://tools.ietf.org/html/rfc9110#section-15.5.13"),
        [415] = new("Unsupported Media Type", "https://tools.ietf.org/html/rfc9110#section-15.5.16"),
        [422] = new("Unprocessable Entity", "https://tools.ietf.org/html/rfc9110#section-15.5.21"),
        [426] = new("Upgrade Required", "https://tools.ietf.org/html/rfc9110#section-15.5.22"),
        [429] = new("Too Many Requests", "https://tools.ietf.org/html/rfc6585#section-4"),
        [500] = new("Internal Server Error", "https://tools.ietf.org/html/rfc9110#section-15.6.1"),
        [501] = new("Not Implemented", "https://tools.ietf.org/html/rfc9110#section-15.6.2"),
        [503] = new("Service Unavailable", "https://tools.ietf.org/html/rfc9110#section-15.6.4"),
        [504] = new("Gateway Timeout", "https://tools.ietf.org/html/rfc9110#section-15.6.5"),
    };

    /// <summary>The <c>detail</c> returned for errors that are not a <see cref="RequestError" /> or <see cref="ValidationError" />.</summary>
    public string UnknownErrorDetail { get; set; } = "An unexpected error occurred.";
}
