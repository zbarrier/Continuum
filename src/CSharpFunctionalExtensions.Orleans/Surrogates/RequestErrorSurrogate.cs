using System.Net;

using Orleans;

using GrpcStatusCodeEnum = Grpc.Core.StatusCode;

namespace Continuum.CSharpFunctionalExtensions.Orleans;

/// <summary>
///     Orleans surrogate for <see cref="RequestError"/>.
/// </summary>
/// <remarks>
///     <see cref="Error.PriorityCode"/> is not stored because it is derived from <see cref="Error.GrpcStatusCode"/>.
/// </remarks>
[GenerateSerializer, Immutable, Alias("Continuum.RequestError")]
public struct RequestErrorSurrogate
{
    /// <summary>The HTTP status code.</summary>
    [Id(0)] public HttpStatusCode HttpStatusCode;
    /// <summary>The gRPC status code.</summary>
    [Id(1)] public GrpcStatusCodeEnum GrpcStatusCode;
    /// <summary>The machine-readable error code.</summary>
    [Id(2)] public string Code;
    /// <summary>The composite format string, or empty to use the localized template.</summary>
    [Id(3)] public string? Format;
    /// <summary>The format arguments.</summary>
    [Id(4)] public ErrorArgument[]? Arguments;
    /// <summary>The resource, parameter or field the error is about.</summary>
    [Id(5)] public string? Target;
    /// <summary>How long the caller should wait before retrying.</summary>
    [Id(6)] public TimeSpan? RetryAfter;
}

/// <summary>
///     Converts between <see cref="RequestError"/> and <see cref="RequestErrorSurrogate"/>.
/// </summary>
[RegisterConverter]
public sealed class RequestErrorSurrogateConverter : IConverter<RequestError, RequestErrorSurrogate>
{
    /// <inheritdoc/>
    public RequestError ConvertFromSurrogate(in RequestErrorSurrogate surrogate) =>
        new(surrogate.HttpStatusCode, surrogate.GrpcStatusCode, surrogate.Code, surrogate.Format, surrogate.Arguments,
            surrogate.Target, surrogate.RetryAfter);

    /// <inheritdoc/>
    public RequestErrorSurrogate ConvertToSurrogate(in RequestError value) => new()
    {
        HttpStatusCode = value.HttpStatusCode,
        GrpcStatusCode = value.GrpcStatusCode,
        Code = value.Code,
        Format = value.Format,
        Arguments = value.Arguments,
        Target = value.Target,
        RetryAfter = value.RetryAfter,
    };
}
