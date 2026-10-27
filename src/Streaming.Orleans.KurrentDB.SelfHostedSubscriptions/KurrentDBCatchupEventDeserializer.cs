using System.Diagnostics.CodeAnalysis;

using Continuum.Serialization.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

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
            logger.EventHasNoPayload(eventType, streamId, subscriptionName);
            return false;
        }
        try
        {
            deserializedEvent = serializer.Deserialize<object>(new BinaryDataWithType(data, type));
        }
        catch (Exception ex)
        {
            logger.EventDeserializationFailed(ex, eventType, streamId, subscriptionName);
            return false;
        }
        if (deserializedEvent is null)
        {
            logger.EventDeserializedToNull(eventType, streamId, subscriptionName);
            return false;
        }
        return true;
    }
}

internal static partial class KurrentDBCatchupEventDeserializerLog
{
	[LoggerMessage(Level = LogLevel.Warning, Message = "Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it has no payload.")]
	public static partial void EventHasNoPayload(this ILogger logger, string eventType, string streamName, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it could not be deserialized.")]
	public static partial void EventDeserializationFailed(this ILogger logger, Exception exception, string eventType, string streamName, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it deserialized to null.")]
	public static partial void EventDeserializedToNull(this ILogger logger, string eventType, string streamName, string subscriptionName);
}
