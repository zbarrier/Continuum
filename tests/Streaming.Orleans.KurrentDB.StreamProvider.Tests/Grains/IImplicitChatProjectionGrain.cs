namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests.Grains;

public interface IImplicitChatProjectionGrain : IGrainWithStringKey
{
    /// <summary>
    ///     The authors seen so far, in the order they were first applied.
    /// </summary>
    Task<string[]> GetAuthors();

    /// <summary>
    ///     The number of events in each delivery this activation applied, in order.
    /// </summary>
    Task<int[]> GetBatchSizes();
}
