using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Thrown when a multi-stream append fails because one or more logs do not have their expected version. Nothing
///     was appended.
/// </summary>
[Alias("Continuum.EventSourcing.LogAppendConflictException.V1"), GenerateSerializer]
public sealed class LogAppendConflictException : InconsistentStateException
{
    /// <summary>
    ///     Creates a new <see cref="LogAppendConflictException"/>.
    /// </summary>
    /// <param name="conflicts">The logs whose version did not match.</param>
    /// <param name="innerException">The store-specific exception.</param>
    public LogAppendConflictException(IReadOnlyList<LogAppendConflict> conflicts, Exception? innerException)
        : base(CreateMessage(conflicts), string.Empty, string.Empty, innerException!)
    {
        Conflicts = conflicts;
    }

    /// <summary>
    ///     Gets the logs whose version did not match.
    /// </summary>
    [Id(0)]
    public IReadOnlyList<LogAppendConflict> Conflicts { get; }

    private static string CreateMessage(IReadOnlyList<LogAppendConflict> conflicts)
    {
        ArgumentNullException.ThrowIfNull(conflicts);
        return "Version conflict (multi-stream append): " + string.Join("; ", conflicts.Select(c => FormattableString.Invariant(
            $"GrainType={c.GrainTypeName} GrainId={c.GrainId} Version={c.ExpectedVersion} StoredVersion={c.ActualVersion}")));
    }
}

/// <summary>
///     A log whose version did not match during a multi-stream append.
/// </summary>
/// <param name="GrainTypeName">The grain type name.</param>
/// <param name="GrainId">The grain ID.</param>
/// <param name="ExpectedVersion">The event count the log was expected to have.</param>
/// <param name="ActualVersion">The event count the log had.</param>
[Alias("Continuum.EventSourcing.LogAppendConflict.V1"), GenerateSerializer, Immutable]
public sealed record LogAppendConflict(
    [property: Id(0)] string GrainTypeName,
    [property: Id(1)] GrainId GrainId,
    [property: Id(2)] int ExpectedVersion,
    [property: Id(3)] int ActualVersion);
