using Azure;
using Azure.Data.Tables;

namespace Continuum.Streaming.Orleans.Azure.Storage;

public sealed class AzureTableCheckpointStore : ICheckpointStore<ulong>
{
    private const string CheckpointsTableName = "Checkpoints";

    private readonly TableClient _client;

    public AzureTableCheckpointStore(TableServiceClient serviceClient)
    {
        _client = serviceClient.GetTableClient(CheckpointsTableName);
        _client.CreateIfNotExists();
    }

    public async Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetEntityIfExistsAsync<Checkpoint>(subscriptionName, partitionId, null, cancellationToken).ConfigureAwait(false);
        return response.HasValue ? response.Value!.Position : default;
    }

    public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong position, CancellationToken cancellationToken = default)
    {
        return _client.UpsertEntityAsync(new Checkpoint
        {
            PartitionKey = subscriptionName,
            RowKey = partitionId,
            Position = position
        }, TableUpdateMode.Replace, cancellationToken);
    }

    private sealed record Checkpoint : ITableEntity
    {
        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;

        public ulong Position { get; init; } = 0;

        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;
    };
}
