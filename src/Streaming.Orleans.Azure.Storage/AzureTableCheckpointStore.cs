using System.Globalization;

using Azure.Data.Tables;

namespace Continuum.Streaming.Orleans.Azure.Storage;

/// <summary>
/// <see cref="ICheckpointStore{TStreamPosition}"/> that persists subscription checkpoints in an Azure Storage table.
/// </summary>
/// <remarks>
/// Checkpoints are stored in the <c>Checkpoints</c> table with the subscription name as the partition key and the
/// subscription partition ID as the row key. Azure Tables has no unsigned or arbitrary-precision integer type, so the
/// position is stored as an invariant-culture string, which supports any formattable and parsable position type
/// (e.g. <see cref="ulong"/> for KurrentDB or <see cref="System.Numerics.BigInteger"/> for Kinesis).
/// </remarks>
/// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
public sealed class AzureTableCheckpointStore<TPosition> : ICheckpointStore<TPosition>
    where TPosition : IComparable<TPosition>, ISpanFormattable, IParsable<TPosition>
{
    private const string CheckpointsTableName = "Checkpoints";
    private const string PositionPropertyName = "Position";

    private readonly TableClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTableCheckpointStore{TPosition}"/> class, creating the
    /// checkpoints table if it does not exist.
    /// </summary>
    /// <param name="serviceClient">The table service client for the storage account that holds the checkpoints.</param>
    public AzureTableCheckpointStore(TableServiceClient serviceClient)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);

        _client = serviceClient.GetTableClient(CheckpointsTableName);
        _client.CreateIfNotExists();
    }

    /// <inheritdoc/>
    public async Task<TPosition> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetEntityIfExistsAsync<TableEntity>(subscriptionName, partitionId, [PositionPropertyName], cancellationToken).ConfigureAwait(false);
        return response.HasValue && response.Value!.GetString(PositionPropertyName) is { } position
            ? TPosition.Parse(position, CultureInfo.InvariantCulture)
            : default!;
    }

    /// <inheritdoc/>
    public Task StoreCheckpointAsync(string subscriptionName, string partitionId, TPosition streamPosition, CancellationToken cancellationToken = default)
    {
        var entity = new TableEntity(subscriptionName, partitionId)
        {
            [PositionPropertyName] = streamPosition.ToString(null, CultureInfo.InvariantCulture),
        };
        return _client.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
    }
}
