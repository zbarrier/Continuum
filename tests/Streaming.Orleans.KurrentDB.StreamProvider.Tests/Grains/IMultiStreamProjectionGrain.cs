namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests.Grains;

public interface IMultiStreamProjectionGrain : IGrainWithStringKey
{
    Task<string[]> GetAuthors();
}
