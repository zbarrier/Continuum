using Continuum.Orleans.KurrentDB;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Serialization;
using Orleans.TestingHost;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class KurrentDBExceptionSerializationTests(ClusterFixture fixture)
{
    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    // Storage failures cross grain boundaries (e.g. pub/sub grain -> calling grain), so the wrapped client exception must
    // survive Orleans serialization; otherwise the caller logs a CodecNotFoundException instead of the real failure.
    [Fact]
    public void Storage_Exception_Round_Trips_With_Its_Grpc_Inner_Exception()
    {
        var serializer = SiloServices.GetRequiredService<Serializer>();
        var original = new KurrentDBStorageException("Failed to write state.",
            new RpcException(new Status(StatusCode.DeadlineExceeded, "Append session timed out")));

        var copy = serializer.Deserialize<Exception>(serializer.SerializeToArray<Exception>(original));

        var storageException = Assert.IsType<KurrentDBStorageException>(copy);
        Assert.Equal(original.Message, storageException.Message);
        var inner = Assert.IsType<RpcException>(storageException.InnerException);
        Assert.Contains("Append session timed out", inner.Message, StringComparison.Ordinal);
    }
}
