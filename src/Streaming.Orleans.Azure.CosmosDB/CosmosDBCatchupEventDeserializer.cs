using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Continuum.Serialization.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB;

/// <summary>
/// Deserializes changes received by a catch-up subscription, logging and skipping any change that has no payload, cannot
/// be read or reads as null, so that a bad document written by a producer outside our control does not halt the subscription.
/// </summary>
internal static class CosmosDBCatchupEventDeserializer
{
    internal static bool TryDeserialize(ChangeFeedEventItem change, Type type, IGrainStorageSerializer serializer, ILogger logger,
        [NotNullWhen(true)] out object? deserializedEvent)
    {
        deserializedEvent = null;
        if (change.Data.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            logger.LogWarning("Skipping change {Id} of type {DataType} in stream {StreamName} because it has no payload.", change.Id, change.DataType, change.StreamName);
            return false;
        }
        try
        {
            var data = JsonSerializer.SerializeToUtf8Bytes(change.Data, ChangeFeedJsonContext.Default.JsonElement);
            deserializedEvent = serializer.Deserialize<object>(new BinaryDataWithType(data, type));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skipping change {Id} of type {DataType} in stream {StreamName} because it could not be deserialized.", change.Id, change.DataType, change.StreamName);
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
