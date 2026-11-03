using System.Diagnostics.CodeAnalysis;
using System.Net;

using Continuum.CSharpFunctionalExtensions;

using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Continuum.Errors;

/// <summary>Machine-readable error codes used across the Continuum framework.</summary>
public static class ContinuumErrorCodes
{
    /// <summary>
    ///     The resource was changed by another operation after it was read (for example a stale ETag or stream version).
    ///     The caller should refresh the resource, re-validate, and retry.
    /// </summary>
    public const string OptimisticConcurrencyConflict = "optimistic_concurrency_conflict";
}

/// <summary>Factory methods for creating <see cref="RequestError"/> instances for Continuum framework failures.</summary>
public static class ContinuumErrors
{
    /// <summary>The default message for <see cref="ContinuumErrorCodes.OptimisticConcurrencyConflict"/>.</summary>
    public const string OptimisticConcurrencyConflictMessage = "The resource was modified by another operation. Refresh and retry.";

    /// <summary>
    ///     Creates a <see cref="RequestError"/> for an optimistic concurrency conflict, using HTTP 409 (Conflict), gRPC
    ///     <see cref="GrpcStatusCode.Aborted"/> and <see cref="ContinuumErrorCodes.OptimisticConcurrencyConflict"/>.
    /// </summary>
    /// <param name="format">A composite format string for the message, or null to use <see cref="OptimisticConcurrencyConflictMessage"/>.</param>
    /// <param name="arguments">The arguments used to format <paramref name="format"/>.</param>
    /// <returns>The optimistic concurrency conflict error.</returns>
    /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string for <paramref name="arguments"/>.</exception>
    public static RequestError NewOptimisticConcurrencyConflict([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format = null, params ErrorArgument[] arguments) =>
        new(HttpStatusCode.Conflict, GrpcStatusCode.Aborted, ContinuumErrorCodes.OptimisticConcurrencyConflict,
            string.IsNullOrEmpty(format) ? OptimisticConcurrencyConflictMessage : format, arguments);
}
