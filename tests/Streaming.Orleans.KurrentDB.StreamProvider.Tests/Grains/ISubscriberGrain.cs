namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests.Grains;

public interface ISubscriberGrain : IGrainWithGuidKey
{
    Task Subscribe(StreamId streamId);

    Task Unsubscribe();
}
