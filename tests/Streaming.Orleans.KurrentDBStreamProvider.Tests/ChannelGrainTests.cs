using System.Text;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

[Collection(ClusterCollection.Name)]
public class ChannelGrainTests
{
    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ChannelGrainTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Join_And_Send_Message_Test()
    {
        // Arrange
        var channel = _fixture.Cluster.Client.GetGrain<IChannelGrain>(Guid.NewGuid());
        var nickname = "Boss";
        var sentMessage = new ChatMessage(nickname, "Hey, everyone!", DateTimeOffset.UtcNow);

        // Act
        var streamId = await channel.Join(nickname);
        await channel.SendMessage(sentMessage);
        var history = await channel.ReadHistory(10);

        // Assert
        Assert.NotNull(history);
        Assert.Contains(history, m => m.Author == sentMessage.Author && m.Text == sentMessage.Text);
    }

    [Theory]
    [InlineData("Boss", "Hey, everyone!")]
    [InlineData("UserA", "Hello!")]
    [InlineData("UserB", "What's up?")]
    public async Task SendMessage_Test(string author, string text)
    {
        // Arrange
        var channel = _fixture.Cluster.Client.GetGrain<IChannelGrain>(Guid.NewGuid());
        await channel.Join(author);
        var sentMessage = new ChatMessage(author, text, DateTimeOffset.UtcNow);

        // Act
        await channel.SendMessage(sentMessage);
        var history = await channel.ReadHistory(10);

        // Assert
        Assert.NotNull(history);
        Assert.Contains(history, m => m.Author == author && m.Text == text);
    }

    [Fact]
    public async Task Leave_Test()
    {
        // Arrange
        var channel = _fixture.Cluster.Client.GetGrain<IChannelGrain>(Guid.NewGuid());
        var nickname = "TestUser";
        await channel.Join(nickname);

        // Act
        await channel.Leave(nickname);
        var members = await channel.GetMembers();

        // Assert
        Assert.DoesNotContain(nickname, members);
    }

    [Fact]
    public async Task Multiple_Clients_Test()
    {
        // Arrange
        var channel = _fixture.Cluster.Client.GetGrain<IChannelGrain>(Guid.NewGuid());
        var nicknames = new[]
                        {
                            "UserA",
                            "UserB",
                            "UserC"
                        };

        // Act
        foreach (var nickname in nicknames)
        {
            await channel.Join(nickname);
        }
        var members = await channel.GetMembers();

        // Assert
        Assert.Equal(nicknames.Length, members.Length);
        foreach (var nickname in nicknames)
        {
            Assert.Contains(nickname, members);
        }

        // Act
        foreach (var nickname in nicknames)
        {
            await channel.Leave(nickname);
        }
        members = await channel.GetMembers();

        // Assert
        foreach (var nickname in nicknames)
        {
            Assert.DoesNotContain(nickname, members);
        }
    }

    [Fact]
    public async Task Chat_History_Test()
    {
        // Arrange
        var channel = _fixture.Cluster.Client.GetGrain<IChannelGrain>(Guid.NewGuid());
        var nickname = "TestUser";
        var sentMessages = new List<ChatMessage>
                           {
                               new(nickname, "Hello, everyone!", DateTimeOffset.UtcNow),
                               new(nickname, "How are you?", DateTimeOffset.UtcNow.AddSeconds(1)),
                               new(nickname, "Bye!", DateTimeOffset.UtcNow.AddSeconds(2))
                           };

        // Act
        await channel.Join(nickname);
        foreach (var message in sentMessages)
        {
            await channel.SendMessage(message);
        }
        var history = await channel.ReadHistory(10);

        // Assert
        Assert.Equal(sentMessages.Count, history.Length);
        foreach (var message in sentMessages)
        {
            Assert.Contains(history, m => m.Author == message.Author && m.Text == message.Text);
        }
    }

    [Fact]
    public async Task Test_Send_Message_And_Subscribe()
    {
        // Arrange
        var nickname = "TestUser";
        var messageText = "Hello, world!";
        var grainFactory = _fixture.Cluster.GrainFactory;
        var channelGrain = grainFactory.GetGrain<IChannelGrain>(Guid.NewGuid());
        var subscriberGrain = grainFactory.GetGrain<ISubscriberGrain>(Guid.NewGuid());

        // Act
        var streamId = await channelGrain.Join(nickname);
        await subscriberGrain.Subscribe(streamId);
        for (var i = 0; i < 5; i++)
        {
            await channelGrain.SendMessage(new ChatMessage(nickname, $"{messageText} {i}", DateTimeOffset.UtcNow));
            await Task.Delay(500, TestContext.Current.CancellationToken);
        }
        await channelGrain.Leave(nickname);
        await subscriberGrain.Unsubscribe();

        // Assert
        var history = await channelGrain.ReadHistory(10);
        Assert.True(history.Length >= 5);
        Assert.Contains(history, x => x.Author == nickname && x.Text.StartsWith($"{messageText}"));
    }

    [Fact]
    public async Task Test_StreamId()
    {
        var id = StreamId.Create("ChatRoom", Guid.NewGuid().ToString("D"));
        var idString = id.ToString();
        _output.WriteLine(idString);
        var idBuffer = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(idString));
        var newId = StreamId.Parse(idBuffer.Span);
        Assert.Equal(id, newId);
    }
}
