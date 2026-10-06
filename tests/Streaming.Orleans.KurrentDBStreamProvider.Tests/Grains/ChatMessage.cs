using Continuum.TypeMapping;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

[DomainEventType("Tests.ChatMessage"), GenerateSerializer, Immutable]
public record ChatMessage(string Author, string Text, DateTimeOffset Created);
