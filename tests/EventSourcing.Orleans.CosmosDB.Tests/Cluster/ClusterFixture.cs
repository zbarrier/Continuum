using Microsoft.Azure.Cosmos;

using Orleans.TestingHost;

using Testcontainers.CosmosDb;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

public sealed class ClusterFixture : IAsyncLifetime
{
    public const string DatabaseName = "orleans";
    public const string ContainerName = "events";

    // The classic Linux emulator is used because the provider relies on transactional batches, ETags and ORDER BY queries.
    // Classic emulator builds stop starting once their evaluation period expires, so the image cannot be pinned to a version.
    private readonly CosmosDbContainer _container = new CosmosDbBuilder("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
        .Build();

    private const int EmulatorPort = 8081;

    // Well-known, publicly documented emulator key; not a secret.
    private const string EmulatorAccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    private TestCluster? _cluster;

    public TestCluster Cluster => _cluster ?? throw new InvalidOperationException("The cluster has not been deployed.");

    internal static CosmosDbContainer? Emulator { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        Emulator = _container;

        using (var client = CreateClient(_container))
        {
            var database = await WaitUntilReadyAsync(() => client.CreateDatabaseIfNotExistsAsync(DatabaseName));
            await database.Database.CreateContainerIfNotExistsAsync(CreateContainerProperties());
        }

        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        _cluster = builder.Build();
        await _cluster.DeployAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_cluster is not null)
        {
            await _cluster.StopAllSilosAsync();
            await _cluster.DisposeAsync();
        }
        Emulator = null;
        await _container.DisposeAsync();
    }

    // Mirrors the indexing policy used by production containers so queries are tested against the same indexes.
    private static ContainerProperties CreateContainerProperties()
    {
        var properties = new ContainerProperties(ContainerName, "/streamName")
        {
            IndexingPolicy = new IndexingPolicy
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true,
            },
        };

        properties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
        properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/\"_etag\"/?" });
        properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/\"data\"/?" });
        properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/\"meta\"/?" });

        return properties;
    }

    // The container reports started before the emulator's gateway serves requests, so retry the first call for a while.
    // While the emulator is still provisioning, it can also answer 403 with substatus 1008 ("Database Account localhost
    // does not exist"); this window is longer when the machine is busy, for example when other test containers start too.
    private static async Task<T> WaitUntilReadyAsync<T>(Func<Task<T>> action)
    {
        var deadline = DateTime.UtcNow.AddMinutes(3);
        while (true)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (DateTime.UtcNow < deadline && IsEmulatorStarting(ex))
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    private static bool IsEmulatorStarting(Exception ex) => ex
        is HttpRequestException
        or CosmosException { StatusCode: System.Net.HttpStatusCode.ServiceUnavailable }
        or CosmosException { StatusCode: System.Net.HttpStatusCode.Forbidden, SubStatusCode: 1008 };

    // The classic emulator only serves HTTPS with a self-signed certificate, while the Testcontainers HttpClient targets
    // the HTTP-only vNext emulator. Connect over HTTPS to the mapped port and stay on that endpoint instead of the
    // container-internal addresses the gateway advertises.
    internal static CosmosClient CreateClient(CosmosDbContainer container)
    {
        var endpoint = new UriBuilder(Uri.UriSchemeHttps, container.Hostname, container.GetMappedPublicPort(EmulatorPort)).Uri;
        return new CosmosClient(endpoint.ToString(), EmulatorAccountKey, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true,
            HttpClientFactory = () => new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            }),
        });
    }
}
