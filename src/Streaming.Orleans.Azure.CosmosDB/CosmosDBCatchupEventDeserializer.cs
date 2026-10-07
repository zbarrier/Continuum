using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB;

/// <summary>
/// Deserializes changes received by a catch-up subscription, logging and skipping any change that cannot be read or
/// reads as null, so that a bad document written by a producer outside our control does not halt the subscription.
/// </summary>
internal static class CosmosDBCatchupEventDeserializer
{
    internal static bool TryDeserialize(ChangeFeedEventItem change, Type type, JsonSerializer jsonSerializer, ILogger logger,
        [NotNullWhen(true)] out object? deserializedEvent)
    {
        try
        {
            deserializedEvent = change.Data.ToObject(type, jsonSerializer);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skipping change {Id} of type {DataType} in stream {StreamName} because it could not be deserialized.", change.Id, change.DataType, change.StreamName);
            deserializedEvent = null;
            return false;
        }
        if (deserializedEvent is null)
        {
            logger.LogWarning("Skipping change {Id} of type {DataType} in stream {StreamName} because it deserialized to null.", change.Id, change.DataType, change.StreamName);
            return false;
        }
        return true;
    }
}
