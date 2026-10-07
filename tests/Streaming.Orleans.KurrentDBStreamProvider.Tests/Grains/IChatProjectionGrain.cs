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

    /// <summary>
    ///     The last event the projection state applied, or <see langword="null"/> if none.
    /// </summary>
    Task<IStreamedEvent<object>?> GetLastApplied();
}
