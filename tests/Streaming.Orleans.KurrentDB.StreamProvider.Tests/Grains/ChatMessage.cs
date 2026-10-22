using Continuum.TypeMapping;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests.Grains;

[DomainEventType("Tests.ChatMessage"), GenerateSerializer, Immutable]
public record ChatMessage(string Author, string Text, DateTimeOffset Created);
