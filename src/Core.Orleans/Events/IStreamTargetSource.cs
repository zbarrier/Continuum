using Continuum.Orleans.Tenancy;

namespace Continuum.Orleans.Events;

/// <summary>
///     Implemented by a journaled grain that routes its events to additional streams. The log view adaptor asks it for
///     each event's targets while appending.
/// </summary>
/// <remarks>
///     <para>
///         Each event always goes to its own grain's stream. Targets are additional grains whose streams receive the
///         event's whole transaction, recorded with the event when it is appended so catch-up subscriptions can fan the
///         transaction out without a projection.
///     </para>
///     <para>
///         There is deliberately no cap on the number of targets. Every target receives a copy of the transaction, so
///         the cost grows with the number of targets and with the size of the transaction.
///     </para>
/// </remarks>
public interface IStreamTargetSource
{
    /// <summary>
    ///     Gets the additional grains whose streams should receive the transaction that <paramref name="event"/> is
    ///     appended in.
    /// </summary>
    /// <param name="event">The event being appended.</param>
    /// <returns>The targets, or an empty list for none.</returns>
    IReadOnlyList<GrainId> GetStreamTargets(object @event);
}

/// <summary>
///     Validates and formats stream targets at write time.
/// </summary>
public static class StreamTargets
{
    /// <summary>
    ///     Validates the targets of an event appended by <paramref name="source"/> and returns them as the text written
    ///     to the event's metadata.
    /// </summary>
    /// <param name="source">The grain appending the event.</param>
    /// <param name="targets">The event's targets.</param>
    /// <returns>The distinct target identifiers, excluding <paramref name="source"/>, or <see langword="null"/> for none.</returns>
    /// <exception cref="ArgumentException">
    ///     A target is the default <see cref="GrainId"/>, has a malformed key, or belongs to a different tenant than
    ///     <paramref name="source"/>.
    /// </exception>
    public static string[]? Format(GrainId source, IReadOnlyList<GrainId>? targets)
    {
        if (targets is not { Count: > 0 })
        {
            return null;
        }
        var sourceTenant = GetTenantId(source, nameof(source));
        var formatted = new List<string>(targets.Count);
        foreach (var target in targets)
        {
            if (target.IsDefault)
            {
                throw new ArgumentException("A stream target must not be the default grain id.", nameof(targets));
            }
            if (!string.Equals(GetTenantId(target, nameof(targets)), sourceTenant, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Stream target '{target}' belongs to a different tenant than '{source}'.", nameof(targets));
            }
            // The event already goes to its own stream, so naming it again would only deliver the transaction twice.
            if (target == source)
            {
                continue;
            }
            var text = target.ToString();
            if (!formatted.Contains(text))
            {
                formatted.Add(text);
            }
        }
        return formatted.Count == 0 ? null : [.. formatted];
    }

    private static string? GetTenantId(GrainId grainId, string paramName)
    {
        try
        {
            TenantKey.Split(grainId.Key.ToString()!, out var tenantId, out _);
            return tenantId;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"Grain id '{grainId}' has a malformed key.", paramName, ex);
        }
    }
}
