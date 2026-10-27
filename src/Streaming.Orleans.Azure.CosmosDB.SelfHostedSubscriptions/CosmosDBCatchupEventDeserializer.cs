using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;

using Continuum.Serialization.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions;

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
            logger.ChangeHasNoPayload(change.Id, change.DataType, change.StreamName);
            return false;
        }
        try
        {
            // The payload's raw UTF-8 is copied as-is rather than re-serialized from the parsed element.
            var data = JsonMarshal.GetRawUtf8Value(change.Data).ToArray();
            deserializedEvent = serializer.Deserialize<object>(new BinaryDataWithType(data, type));
        }
        catch (Exception ex)
        {
            logger.ChangeDeserializationFailed(ex, change.Id, change.DataType, change.StreamName);
            return false;
        }
        if (deserializedEvent is null)
        {
            logger.ChangeDeserializedToNull(change.Id, change.DataType, change.StreamName);
            return false;
        }
        return true;
    }
}

internal static partial class CosmosDBCatchupEventDeserializerLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipping change {Id} of type {DataType} in stream {StreamName} because it has no payload.")]
    public static partial void ChangeHasNoPayload(this ILogger logger, string id, string dataType, string streamName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipping change {Id} of type {DataType} in stream {StreamName} because it could not be deserialized.")]
    public static partial void ChangeDeserializationFailed(this ILogger logger, Exception exception, string id, string dataType, string streamName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipping change {Id} of type {DataType} in stream {StreamName} because it deserialized to null.")]
    public static partial void ChangeDeserializedToNull(this ILogger logger, string id, string dataType, string streamName);
}
