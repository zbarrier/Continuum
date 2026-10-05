using Continuum.Domain;
using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Responses;

[Immutable]
[GenerateSerializer]
public sealed class SnackRemoveResponse(IEnumerable<IChange> changes, int version)
    : EventSourcedCommandResponseBase(changes, version);
