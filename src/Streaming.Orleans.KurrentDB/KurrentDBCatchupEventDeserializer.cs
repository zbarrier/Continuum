using System.Diagnostics.CodeAnalysis;

using Continuum.Serialization.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
/// Deserializes events received by a catch-up subscription, logging and skipping any event that has no payload, cannot
/// be read or reads as null, so that a bad event written by a producer outside our control does not halt the subscription.
/// </summary>
internal static class KurrentDBCatchupEventDeserializer
{
    internal static bool TryDeserialize(IGrainStorageSerializer serializer, ILogger logger, string eventType, string streamId, ReadOnlyMemory<byte> data, Type type, string subscriptionName,
        [NotNullWhen(true)] out object? deserializedEvent)
    {
        deserializedEvent = null;
        if (data.IsEmpty)
        {
            logger.LogWarning("Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it has no payload.",
                eventType, streamId, subscriptionName);
            return false;
        }
        try
        {
            deserializedEvent = serializer.Deserialize<object>(new BinaryDataWithType(data, type));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it could not be deserialized.",
                eventType, streamId, subscriptionName);
            return false;
        }
        if (deserializedEvent is null)
        {
            logger.LogWarning("Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it deserialized to null.",
                eventType, streamId, subscriptionName);
            return false;
        }
        return true;
    }
}
