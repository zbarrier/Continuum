namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public interface IMultiStreamProjectionGrain : IGrainWithStringKey
{
    Task<string[]> GetAuthors();
}
