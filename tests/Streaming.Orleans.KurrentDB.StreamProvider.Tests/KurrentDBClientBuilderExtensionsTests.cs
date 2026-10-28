using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that the cluster client overloads of AddKurrentDBStreams register the KurrentDB options and their
///     validator for the named provider.
/// </summary>
public class KurrentDBClientBuilderExtensionsTests
{
    private const string ProviderName = "client-provider";

    private static IServiceProvider BuildClientServices(Action<IClientBuilder> configure)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.UseOrleansClient(client =>
        {
            client.UseLocalhostClustering();
            configure(client);
        });
        return builder.Build().Services;
    }

    private static void ConfigureOptions(KurrentDBOptions options)
    {
        options.ConnectionName = "client-connection";
        options.Name = "client-name";
        options.Queues = ["client-queue"];
    }

    [Fact]
    public void The_Options_Overload_Applies_The_KurrentDB_Options()
    {
        var services = BuildClientServices(client => client.AddKurrentDBStreams(ProviderName, ConfigureOptions));

        var options = services.GetOptionsByName<KurrentDBOptions>(ProviderName);

        Assert.Equal("client-connection", options.ConnectionName);
        Assert.Equal("client-name", options.Name);
        Assert.Equal(["client-queue"], options.Queues);
    }

    [Fact]
    public void The_Configurator_Overload_Applies_The_KurrentDB_Options()
    {
        var services = BuildClientServices(client => client.AddKurrentDBStreams(ProviderName,
            configurator => configurator.ConfigureKurrentDB(ob => ob.Configure(ConfigureOptions))));

        var options = services.GetOptionsByName<KurrentDBOptions>(ProviderName);

        Assert.Equal("client-connection", options.ConnectionName);
        Assert.Equal(["client-queue"], options.Queues);
    }

    [Fact]
    public void Registers_The_KurrentDB_Options_Validator()
    {
        var services = BuildClientServices(client => client.AddKurrentDBStreams(ProviderName, ConfigureOptions));

        Assert.Contains(services.GetServices<IConfigurationValidator>(), validator => validator is KurrentDBOptionsValidator);
    }

    [Fact]
    public void Registers_The_Named_Stream_Provider()
    {
        var services = BuildClientServices(client => client.AddKurrentDBStreams(ProviderName, ConfigureOptions));

        Assert.NotNull(services.GetKeyedService<global::Orleans.Streams.IStreamProvider>(ProviderName));
    }

    [Fact]
    public void Returns_The_Same_Builder()
    {
        IClientBuilder? configured = null;
        IClientBuilder? returned = null;

        BuildClientServices(client =>
        {
            configured = client;
            returned = client.AddKurrentDBStreams(ProviderName, ConfigureOptions);
        });

        Assert.Same(configured, returned);
    }
}
