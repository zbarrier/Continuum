namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public interface IChatProjectionGrain : IGrainWithStringKey
{
    /// <summary>
    ///     The authors seen so far, in the order they were first applied.
    /// </summary>
    Task<string[]> GetAuthors();

    /// <summary>
    ///     The number of events the projection state applied.
    /// </summary>
    Task<int> GetAppliedCount();
}
