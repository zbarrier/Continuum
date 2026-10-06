namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public interface IImplicitChatProjectionGrain : IGrainWithStringKey
{
    /// <summary>
    ///     The authors seen so far, in the order they were first applied.
    /// </summary>
    Task<string[]> GetAuthors();
}
