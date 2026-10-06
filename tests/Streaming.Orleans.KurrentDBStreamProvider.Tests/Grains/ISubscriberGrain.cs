namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public interface ISubscriberGrain : IGrainWithGuidKey
{
    Task Subscribe(StreamId streamId);

    Task Unsubscribe();
}
