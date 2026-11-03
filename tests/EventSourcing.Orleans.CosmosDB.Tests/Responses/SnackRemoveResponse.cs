using Continuum.Domain;
using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Responses;

[Immutable]
[GenerateSerializer]
public sealed class SnackRemoveResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);
