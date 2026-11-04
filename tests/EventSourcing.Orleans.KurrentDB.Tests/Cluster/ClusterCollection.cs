namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;

[CollectionDefinition(ClusterCollection.Name)]
public class ClusterCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = "ClusterCollection";
}
