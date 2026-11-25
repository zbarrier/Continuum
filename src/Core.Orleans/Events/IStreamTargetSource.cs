using Continuum.Orleans.Tenancy;

namespace Continuum.Orleans.Events;

/// <summary>
///     Implemented by a journaled grain that routes its events to additional streams. The log view adaptor asks it for
///     each event's targets while appending.
/// </summary>
/// <remarks>
///     <para>
///         Each event always goes to its own grain's stream. Targets are additional grains whose streams receive the
///         event, recorded with the event when it is appended so catch-up subscriptions can fan it out without a
///         projection. A <see cref="StreamTargetScope.Transaction"/> target receives the event's whole transaction; a
///         <see cref="StreamTargetScope.Event"/> target receives only the events of the transaction that name it.
///     </para>
///     <para>
///         A grain always chooses the targets of its own events. When it takes part in another grain's multi-stream
///         transaction, it chooses them while it prepares its events, and the coordinator appends them unchanged.
///     </para>
///     <para>
///         There is deliberately no cap on the number of targets. Every transaction-scoped target receives a copy of
///         the whole transaction, so the cost grows with the number of targets and with the size of the transaction.
///         Event-scoped targets only cost the events that name them.
///     </para>
/// </remarks>
public interface IStreamTargetSource
{
    /// <summary>
    ///     Gets the additional grains whose streams should receive <paramref name="event"/>, and whether each receives
    ///     the whole transaction it is appended in or only the events that name it.
    /// </summary>
    /// <param name="event">The event being appended.</param>
    /// <returns>The targets, or an empty list for none.</returns>
    IReadOnlyList<StreamTarget> GetStreamTargets(object @event);
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
    /// <returns>
    ///     The distinct target identifiers by scope, excluding <paramref name="source"/>, or <see langword="null"/> for
    ///     none. A target named in both scopes is kept only as transaction scoped, since the whole transaction already
    ///     includes the event.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     A target is the default <see cref="GrainId"/>, has a malformed key, or belongs to a different tenant than
    ///     <paramref name="source"/>.
    /// </exception>
    public static FormattedStreamTargets? Format(GrainId source, IReadOnlyList<StreamTarget>? targets)
    {
        if (targets is not { Count: > 0 })
        {
            return null;
        }
        var sourceTenant = GetTenantId(source, nameof(source));
        List<string>? transactionScoped = null;
        List<string>? eventScoped = null;
        foreach (var (target, scope) in targets)
        {
            if (target.IsDefault)
            {
                throw new ArgumentException("A stream target must not be the default grain id.", nameof(targets));
            }
            if (!Enum.IsDefined(scope))
            {
                throw new ArgumentException($"Stream target '{target}' has an unknown scope '{scope}'.", nameof(targets));
            }
            if (!string.Equals(GetTenantId(target, nameof(targets)), sourceTenant, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Stream target '{target}' belongs to a different tenant than '{source}'.", nameof(targets));
            }
            // The event already goes to its own stream, so naming it again would only deliver it twice.
            if (target == source)
            {
                continue;
            }
            var text = target.ToString();
            if (scope == StreamTargetScope.Transaction)
            {
                eventScoped?.Remove(text);
                transactionScoped ??= [];
                if (!transactionScoped.Contains(text))
                {
                    transactionScoped.Add(text);
                }
            }
            else if (transactionScoped?.Contains(text) != true)
            {
                eventScoped ??= [];
                if (!eventScoped.Contains(text))
                {
                    eventScoped.Add(text);
                }
            }
        }
        return transactionScoped is not { Count: > 0 } && eventScoped is not { Count: > 0 }
            ? null
            : new FormattedStreamTargets(transactionScoped?.ToArray() ?? [], eventScoped?.ToArray() ?? []);
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
