using KurrentDB.Client;

using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a producer refuses appends it cannot write rather than dropping them silently.
/// </summary>
public class KurrentDBProducerTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";

    private static KurrentDBProducer CreateProducer(KurrentDBClient client) =>
        new(client, new KurrentProducerSettings { Options = new KurrentDBOptions(), QueueName = "queue-0" }, NullLogger.Instance);

    private static EventData CreateEvent() => new(Uuid.NewUuid(), "test", new byte[] { 1 });

    [Fact]
    public async Task AppendAsync_BeforeInit_Throws()
    {
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var producer = CreateProducer(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() => producer.AppendAsync(CreateEvent()));
    }

    [Fact]
    public async Task AppendAsync_AfterClose_Throws()
    {
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var producer = CreateProducer(client);
        producer.Init();
        await producer.CloseAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => producer.AppendAsync(CreateEvent()));
    }
}
