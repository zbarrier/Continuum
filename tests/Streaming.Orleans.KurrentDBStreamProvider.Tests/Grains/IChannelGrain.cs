namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public interface IChannelGrain : IGrainWithGuidKey
{
    Task<StreamId> Join(string nickname);

    Task<StreamId> Leave(string nickname);

    Task<bool> SendMessage(ChatMessage message);

    Task<ChatMessage[]> ReadHistory(int numberOfMessages);

    Task<string[]> GetMembers();
}
