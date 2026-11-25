using Continuum.Orleans.Events;

namespace Continuum.Orleans.Transactions;

/// <summary>One grain's part of a multi-stream transaction, returned when a participant prepares.</summary>
/// <param name="GrainId">The grain's ID. Its type identifies the grain type.</param>
/// <param name="Events">The events to append. May be empty.</param>
/// <param name="ExpectedVersion">The version the grain's log must be at, or <see langword="null"/> for no version check.</param>
/// <param name="StreamTargets">
///     The validated stream targets of each event, chosen by the grain that owns the events, in the same order as
///     <paramref name="Events"/>; an entry is <see langword="null"/> for an event without targets. The list itself is
///     <see langword="null"/> when no event has any.
/// </param>
[GenerateSerializer, Immutable, Alias("Continuum.PreparedTransaction.V1")]
public sealed record PreparedTransaction(
	[property: Id(0)] GrainId GrainId,
	[property: Id(1)] IReadOnlyList<object> Events,
	[property: Id(2)] int? ExpectedVersion,
	[property: Id(3)] IReadOnlyList<FormattedStreamTargets?>? StreamTargets = null);
