using System.Globalization;
using System.Net;

using Azure;
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
    where TPosition : struct, IComparable<TPosition>, ISpanFormattable, IParsable<TPosition>
{
    private const string CheckpointsTableName = "Checkpoints";
    private const string PositionPropertyName = "Position";

    private readonly TableClient _client;
    private volatile bool _tableCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTableCheckpointStore{TPosition}"/> class. The checkpoints
    /// table is created on first use if it does not exist.
    /// </summary>
    /// <param name="serviceClient">The table service client for the storage account that holds the checkpoints.</param>
    public AzureTableCheckpointStore(TableServiceClient serviceClient)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);

        _client = serviceClient.GetTableClient(CheckpointsTableName);
    }

    /// <inheritdoc/>
    public async Task<TPosition?> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken).ConfigureAwait(false);
        var response = await _client.GetEntityIfExistsAsync<TableEntity>(subscriptionName, partitionId, [PositionPropertyName], cancellationToken).ConfigureAwait(false);
        return response.HasValue && response.Value!.GetString(PositionPropertyName) is { } position
            ? TPosition.Parse(position, CultureInfo.InvariantCulture)
            : null;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Checkpoints only move forward. A position at or behind the stored one is ignored, and concurrent writers are
    /// resolved with optimistic concurrency so a slower writer can never overwrite a newer checkpoint.
    /// </remarks>
    public async Task StoreCheckpointAsync(string subscriptionName, string partitionId, TPosition streamPosition, CancellationToken cancellationToken = default)
    {
        await EnsureTableAsync(cancellationToken).ConfigureAwait(false);
        var entity = new TableEntity(subscriptionName, partitionId)
        {
            [PositionPropertyName] = streamPosition.ToString(null, CultureInfo.InvariantCulture),
        };
        while (true)
        {
            var existing = await _client.GetEntityIfExistsAsync<TableEntity>(subscriptionName, partitionId, [PositionPropertyName], cancellationToken).ConfigureAwait(false);
            if (!existing.HasValue)
            {
                try
                {
                    await _client.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
                {
                    // Another writer created the checkpoint first; compare against it.
                    continue;
                }
            }

            var current = existing.Value!;
            if (current.GetString(PositionPropertyName) is { } stored && TPosition.Parse(stored, CultureInfo.InvariantCulture).CompareTo(streamPosition) >= 0)
            {
                return;
            }
            try
            {
                await _client.UpdateEntityAsync(entity, current.ETag, TableUpdateMode.Replace, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.PreconditionFailed)
            {
                // Another writer changed the checkpoint since it was read; compare against the new value.
            }
        }
    }

    private async ValueTask EnsureTableAsync(CancellationToken cancellationToken)
    {
        if (_tableCreated)
        {
            return;
        }
        // Idempotent, so concurrent first callers are harmless; a failure is retried on the next call.
        await _client.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
        _tableCreated = true;
    }
}
