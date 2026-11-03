using Continuum.Domain;
using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Responses;

[Immutable]
[GenerateSerializer]
public sealed class SnackChangeNameResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);
