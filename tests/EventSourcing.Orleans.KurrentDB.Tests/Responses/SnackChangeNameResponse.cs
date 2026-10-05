using Continuum.Domain;
using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Responses;

[Immutable]
[GenerateSerializer]
public sealed class SnackChangeNameResponse(IEnumerable<IChange> changes, int version)
    : EventSourcedCommandResponseBase(changes, version);
