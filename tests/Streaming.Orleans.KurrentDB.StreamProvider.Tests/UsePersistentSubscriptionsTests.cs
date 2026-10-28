using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that selecting the persistent subscription strategy registers its receiver factory, options and
///     validator, and nothing belonging to the $all strategy.
/// </summary>
public class UsePersistentSubscriptionsTests
{
    private const string ProviderName = "test-provider";

    private static ServiceProvider BuildServices(Action<ISiloKurrentDBStreamConfigurator> configure)
    {
        var services = new ServiceCollection();
        var configurator = new SiloKurrentDBStreamConfigurator(ProviderName, configureServices => configureServices(services));
        configure(configurator);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Registers_The_Persistent_Subscription_Receiver_Factory_For_The_Provider()
    {
        using var services = BuildServices(configurator => configurator.UsePersistentSubscriptions());

        var factory = services.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName);

        Assert.IsType<KurrentDBPersistentSubscriptionReceiverFactory>(factory);
        Assert.IsType<KurrentDBPersistentSubscriptionReceiverOptions>(factory.ReceiverOptions);
    }

    [Fact]
    public void Applies_The_Configured_Receiver_Options()
    {
        using var services = BuildServices(configurator => configurator.UsePersistentSubscriptions(ob => ob.Configure(options =>
        {
            options.PrefetchCount = 42;
            options.UpdateExistingSubscription = true;
        })));

        var options = Assert.IsType<KurrentDBPersistentSubscriptionReceiverOptions>(
            services.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName).ReceiverOptions);

        Assert.Equal(42, options.PrefetchCount);
        Assert.True(options.UpdateExistingSubscription);
        Assert.Same(options, services.GetOptionsByName<KurrentDBPersistentSubscriptionReceiverOptions>(ProviderName));
    }

    [Fact]
    public void Uses_The_Default_Receiver_Options_When_None_Are_Configured()
    {
        using var services = BuildServices(configurator => configurator.UsePersistentSubscriptions());

        var options = services.GetOptionsByName<KurrentDBPersistentSubscriptionReceiverOptions>(ProviderName);

        Assert.Equal(10, options.PrefetchCount);
        Assert.False(options.UpdateExistingSubscription);
    }

    [Fact]
    public void Registers_The_Persistent_Subscription_Options_Validator()
    {
        using var services = BuildServices(configurator => configurator.UsePersistentSubscriptions());

        var validators = services.GetServices<IConfigurationValidator>().ToList();

        Assert.Contains(validators, validator => validator is KurrentDBPersistentSubscriptionReceiverOptionsValidator);
        Assert.DoesNotContain(validators, validator => validator is KurrentDBAllStreamReceiverOptionsValidator);
    }

    [Fact]
    public void Registers_Only_One_Receiver_Factory_For_The_Provider()
    {
        using var services = BuildServices(configurator => configurator.UsePersistentSubscriptions());

        Assert.Single(services.GetKeyedServices<IKurrentDBReceiverFactory>(ProviderName));
    }

    [Fact]
    public void Rejects_A_Null_Configurator()
    {
        Assert.Throws<ArgumentNullException>(() => KurrentDBSubscriptionStrategyConfiguratorExtensions.UsePersistentSubscriptions(null!));
    }
}
