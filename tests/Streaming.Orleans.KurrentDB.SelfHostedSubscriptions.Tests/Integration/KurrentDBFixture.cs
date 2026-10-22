using System.Net;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests.Integration;

public sealed class KurrentDBFixture : IAsyncLifetime
{
    private const int KurrentDBPort = 2113;

    // Pinned so a new KurrentDB release cannot change test behavior unnoticed.
    // A random host port lets parallel runs and leftover containers coexist.
    private readonly IContainer _container = new ContainerBuilder("docker.kurrent.io/kurrent-latest/kurrentdb:25.1")
        .WithPortBinding(KurrentDBPort, true)
        .WithEnvironment("KURRENTDB_INSECURE", "true")
        .WithEnvironment("KURRENTDB_MEM_DB", "true")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(r => r
                .ForPort(KurrentDBPort)
                .ForPath("/health/live")
                .ForStatusCode(HttpStatusCode.NoContent)
                .ForStatusCode(HttpStatusCode.OK)))
        .Build();

    public string ConnectionString { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = FormattableString.Invariant(
            $"kurrentdb://{_container.Hostname}:{_container.GetMappedPublicPort(KurrentDBPort)}?tls=false");
        await WaitUntilReadyAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    // /health/live passes once the process is up, before the node has become leader and accepts writes, so retry a
    // write for a while. Otherwise the first test can fail with "Server Is Not Ready".
    private async Task WaitUntilReadyAsync()
    {
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var deadline = DateTime.UtcNow.AddMinutes(1);
        while (true)
        {
            try
            {
                await client.AppendToStreamAsync($"readiness-{Guid.NewGuid():N}", StreamState.NoStream,
                    [new EventData(Uuid.NewUuid(), "readiness", ReadOnlyMemory<byte>.Empty)]);
                return;
            }
            catch (Exception) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }
    }
}

[CollectionDefinition(Name)]
public class KurrentDBCollection : ICollectionFixture<KurrentDBFixture>
{
    public const string Name = "KurrentDBCollection";
}
