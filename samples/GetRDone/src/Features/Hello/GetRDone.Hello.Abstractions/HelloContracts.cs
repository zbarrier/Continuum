using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Hello;

/// <summary>
///     Temporary grain that proves the Web → Silo → KurrentDB round trip. Replace it once the real event model exists.
/// </summary>
public interface IHelloGrain : IEventDrivenGrain
{
    Task<Result<SayHelloResponse>> SayHelloAsync(SayHelloCommand cmd);
}

[GenerateSerializer, Immutable]
public sealed record SayHelloCommand([property: Id(0)] string Message);

[GenerateSerializer, Immutable]
public sealed record SayHelloResponse([property: Id(0)] string Greeting, [property: Id(1)] int GreetingCount);

[GenerateSerializer, Immutable]
public abstract record HelloEvent([property: Id(0)] Guid InitiatedBy, [property: Id(1)] DateTimeOffset OccurredAt);

[DomainEventType("GetRDone.Hello.HelloSaid"), GenerateSerializer, Immutable]
public sealed record HelloSaidEvent(Guid InitiatedBy, [property: Id(0)] string Message, DateTimeOffset OccurredAt)
    : HelloEvent(InitiatedBy, OccurredAt);
