namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

[CollectionDefinition(ClusterCollection.Name)]
public class ClusterCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = "ClusterCollection";
}
