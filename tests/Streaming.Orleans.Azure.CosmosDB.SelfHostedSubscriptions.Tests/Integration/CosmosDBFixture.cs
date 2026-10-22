using Microsoft.Azure.Cosmos;

using Testcontainers.CosmosDb;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests.Integration;

public sealed class CosmosDBFixture : IAsyncLifetime
{
    public const string DatabaseName = "subscriptions";

    private const int EmulatorPort = 8081;

    // Well-known, publicly documented emulator key; not a secret.
    private const string EmulatorAccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    // The classic Linux emulator supports the change feed processor and lease containers.
    // Classic emulator builds stop starting once their evaluation period expires, so the image cannot be pinned to a version.
    private readonly CosmosDbContainer _container = new CosmosDbBuilder("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
        .Build();

    public CosmosClient Client { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        Client = CreateClient();
        await WaitUntilReadyAsync(() => Client.CreateDatabaseIfNotExistsAsync(DatabaseName));
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        await _container.DisposeAsync();
    }

    /// <summary>Creates a monitored container and a lease container with unique names for one test.</summary>
    public async Task<(string Monitored, string Leases)> CreateContainersAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var database = Client.GetDatabase(DatabaseName);
        var monitored = $"events-{suffix}";
        var leases = $"leases-{suffix}";
        await database.CreateContainerAsync(monitored, "/streamName");
        await database.CreateContainerAsync(leases, "/id");
        return (monitored, leases);
    }

    // The container reports started before the emulator gateway serves requests, so retry the first call for a while.
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

    // The classic emulator only serves HTTPS with a self-signed certificate. Connect to the mapped port and stay on that
    // endpoint instead of the container-internal addresses the gateway advertises.
    private CosmosClient CreateClient()
    {
        var endpoint = new UriBuilder(Uri.UriSchemeHttps, _container.Hostname, _container.GetMappedPublicPort(EmulatorPort)).Uri;
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

[CollectionDefinition(Name)]
public class CosmosDBCollection : ICollectionFixture<CosmosDBFixture>
{
    public const string Name = "CosmosDBCollection";
}
