using Continuum.EventSourcing.Orleans;
using Continuum.Orleans.Tenancy;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

public class TenantLogConsistentStorageTests
{
    private static GrainId GrainFor(string? tenantId, string key) =>
        GrainId.Create("snack", TenantKey.Create(tenantId, key));

    [Fact]
    public async Task Grain_without_tenant_uses_default_storage()
    {
        var defaultStorage = new FakeStorage();
        var created = new List<string>();
        var sut = new TenantLogConsistentStorage(defaultStorage, (tenantId, _) =>
        {
            created.Add(tenantId);
            return Task.FromResult<ILogConsistentStorage>(new FakeStorage());
        });

        await sut.GetLastVersionAsync("snack", GrainFor(null, "1"));

        Assert.Equal(1, defaultStorage.Calls);
        Assert.Empty(created);
    }

    [Fact]
    public async Task Tenant_storage_is_created_once_and_reused()
    {
        var defaultStorage = new FakeStorage();
        var storages = new Dictionary<string, FakeStorage>();
        var sut = new TenantLogConsistentStorage(defaultStorage, (tenantId, _) =>
        {
            var storage = new FakeStorage();
            storages.Add(tenantId, storage);
            return Task.FromResult<ILogConsistentStorage>(storage);
        });

        await sut.GetLastVersionAsync("snack", GrainFor("acme", "1"));
        await sut.AppendAsync("snack", GrainFor("acme", "2"), new List<string> { "e" }, 0);
        await sut.ReadAsync<string>("snack", GrainFor("contoso", "1"), 0, 10);

        Assert.Equal(0, defaultStorage.Calls);
        Assert.Equal(2, storages["acme"].Calls);
        Assert.Equal(1, storages["contoso"].Calls);
    }

    [Fact]
    public async Task Failed_tenant_storage_creation_is_retried()
    {
        var attempts = 0;
        var sut = new TenantLogConsistentStorage(new FakeStorage(), (_, _) =>
        {
            attempts++;
            return attempts == 1
                ? Task.FromException<ILogConsistentStorage>(new InvalidOperationException("boom"))
                : Task.FromResult<ILogConsistentStorage>(new FakeStorage());
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetLastVersionAsync("snack", GrainFor("acme", "1")));
        await sut.GetLastVersionAsync("snack", GrainFor("acme", "1"));

        Assert.Equal(2, attempts);
    }

    private sealed class FakeStorage : ILogConsistentStorage
    {
        public int Calls { get; private set; }

        public Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int maxCount)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<TLogEntry>>([]);
        }

        public Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId)
        {
            Calls++;
            return Task.FromResult(0);
        }

        public Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion)
        {
            Calls++;
            return Task.FromResult(expectedVersion + entries.Count);
        }
    }
}
