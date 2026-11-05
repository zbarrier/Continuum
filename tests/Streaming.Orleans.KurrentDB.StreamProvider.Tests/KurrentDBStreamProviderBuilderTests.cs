using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

public class KurrentDBStreamProviderBuilderTests
{
    private sealed class TestSiloBuilder(IConfiguration configuration) : ISiloBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();

        public IConfiguration Configuration { get; } = configuration;
    }

    private static TestSiloBuilder Configure(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var builder = new TestSiloBuilder(configuration);
        new KurrentDBStreamProviderBuilder().Configure(builder, "Provider", configuration.GetSection("Orleans:Streaming:Provider"));
        return builder;
    }

    [Fact]
    public void Configure_BindsProviderAndPersistentOptions()
    {
        var builder = Configure(new()
        {
            ["Orleans:Streaming:Provider:ConnectionName"] = "kurrentdb",
            ["Orleans:Streaming:Provider:Credentials:UseDefault"] = "false",
            ["Orleans:Streaming:Provider:Credentials:Username"] = "admin",
            ["Orleans:Streaming:Provider:Queues:0"] = "$ce-Order",
            ["Orleans:Streaming:Provider:StartFromNow"] = "false",
            ["Orleans:Streaming:Provider:PrefetchCount"] = "25",
        });

        using var services = builder.Services.BuildServiceProvider();
        var options = services.GetRequiredService<IOptionsMonitor<KurrentDBOptions>>().Get("Provider");
        var receiverOptions = services.GetRequiredService<IOptionsMonitor<KurrentDBPersistentSubscriptionReceiverOptions>>().Get("Provider");

        Assert.Equal("kurrentdb", options.ConnectionName);
        Assert.False(options.Credentials.UseDefault);
        Assert.Equal("admin", options.Credentials.Username);
        Assert.Equal(["$ce-Order"], options.Queues);
        Assert.False(receiverOptions.StartFromNow);
        Assert.Equal(25, receiverOptions.PrefetchCount);
        Assert.NotNull(services.GetKeyedService<IKurrentDBReceiverFactory>("Provider"));
    }

    [Fact]
    public void Configure_BindsAllStreamCatchupOptions()
    {
        var builder = Configure(new()
        {
            ["Orleans:Streaming:Provider:SubscriptionType"] = "AllStreamCatchup",
            ["Orleans:Streaming:Provider:ConnectionName"] = "kurrentdb",
            ["Orleans:Streaming:Provider:ResolveLinkTos"] = "false",
            ["Orleans:Streaming:Provider:BufferCapacityMultiplier"] = "4",
            ["Orleans:Streaming:Provider:CheckpointMonitorOptions:MaxEventsBeforeCommit"] = "50",
            ["Orleans:Streaming:Provider:UnknownEventTypeBehavior"] = "Skip",
        });
        var adapterDescriptor = builder.Services.First(d => d.ServiceType == typeof(IConfigureOptions<KurrentDBDataAdapterOptions>) && d.ImplementationInstance is IConfigureNamedOptions<KurrentDBDataAdapterOptions>);
        var adapterOptions = new KurrentDBDataAdapterOptions();
        ((IConfigureNamedOptions<KurrentDBDataAdapterOptions>)adapterDescriptor.ImplementationInstance!).Configure("Provider", adapterOptions);
        Assert.Equal(UnknownEventTypeBehavior.Skip, adapterOptions.UnknownEventTypeBehavior);

        var descriptor = builder.Services.First(d => d.ServiceType == typeof(IConfigureOptions<KurrentDBAllStreamCatchupSubscriptionReceiverOptions>));
        var configure = (IConfigureNamedOptions<KurrentDBAllStreamCatchupSubscriptionReceiverOptions>)descriptor.ImplementationInstance!;
        var receiverOptions = new KurrentDBAllStreamCatchupSubscriptionReceiverOptions();
        configure.Configure("Provider", receiverOptions);

        Assert.False(receiverOptions.ResolveLinkTos);
        Assert.Equal(4, receiverOptions.BufferCapacityMultiplier);
        Assert.Equal(50UL, receiverOptions.CheckpointMonitorOptions.MaxEventsBeforeCommit);
    }

    [Fact]
    public void Configure_UnknownSubscriptionType_Throws()
    {
        Assert.Throws<OrleansConfigurationException>(() => Configure(new()
        {
            ["Orleans:Streaming:Provider:SubscriptionType"] = "Bogus",
        }));
    }
}
