using Orleans.Runtime;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// Identifies a stream that a <see cref="ProjectionGrain{TGrain, TState}"/> subscribes to explicitly.
/// </summary>
/// <remarks>
/// The provider is part of the identity because a stream namespace is not globally unique: the same namespace may
/// be carried by more than one provider, and a grain may consume streams from several of them. This type is only
/// meaningful for explicit subscriptions; for an implicit subscription the runtime selects both the provider and
/// the stream, so there is nothing for the grain to declare.
/// </remarks>
/// <param name="ProviderName">The name of the stream provider to subscribe through.</param>
/// <param name="StreamId">The identity of the stream to subscribe to.</param>
[Alias("Continuum.Streaming.StreamSubscriptionSource.V1"), GenerateSerializer, Immutable]
public readonly record struct StreamSubscriptionSource(string ProviderName, StreamId StreamId);
