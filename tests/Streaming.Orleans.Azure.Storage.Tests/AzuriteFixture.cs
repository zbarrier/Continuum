using Azure.Data.Tables;

using Testcontainers.Azurite;

namespace Continuum.Streaming.Orleans.Azure.Storage.Tests;

/// <summary>
/// Starts one Azurite container for the test collection. Azurite binds to random host ports, so runs never collide
/// with each other or with a locally running emulator.
/// </summary>
public sealed class AzuriteFixture : IAsyncLifetime
{
    private readonly AzuriteContainer _container = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:latest")
        .WithInMemoryPersistence()
        .Build();

    public TableServiceClient ServiceClient { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        ServiceClient = new TableServiceClient(_container.GetConnectionString());
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}

[CollectionDefinition(Name)]
public sealed class AzuriteCollection : ICollectionFixture<AzuriteFixture>
{
    public const string Name = "Azurite";
}
