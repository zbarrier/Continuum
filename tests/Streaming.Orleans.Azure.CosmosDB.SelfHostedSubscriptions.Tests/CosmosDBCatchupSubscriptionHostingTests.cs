using System.Diagnostics.Metrics;

using Continuum.Orleans.Hosting;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that subscription and health-check registration share a single set of diagnostics services.
/// </summary>
public class CosmosDBCatchupSubscriptionHostingTests
{
    private sealed class FakeSubscription;

    private abstract class RecordingSubscription(string name) : IStreamSubscription, IHostedService
    {
        public string SubscriptionName => name;
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class OrdersSubscription() : RecordingSubscription("Orders");

    private sealed class InvoicesSubscription() : RecordingSubscription("Invoices");

    private static HostApplicationBuilder CreateBuilder(params string[] subscriptionNames)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        foreach (var name in subscriptionNames)
        {
            builder.AddCosmosDBCatchupSubscription(new CosmosDBCatchupSubscriptionInfo(name, typeof(FakeSubscription)));
        }
        return builder;
    }

    [Fact]
    public async Task Registered_subscriptions_start_and_stop_with_the_host()
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.AddCosmosDBCatchupSubscription(new CosmosDBCatchupSubscriptionInfo("Orders", typeof(OrdersSubscription)));
        builder.AddCosmosDBCatchupSubscription(new CosmosDBCatchupSubscriptionInfo("Invoices", typeof(InvoicesSubscription)));

        using var host = builder.Build();
        var orders = host.Services.GetRequiredService<OrdersSubscription>();
        var invoices = host.Services.GetRequiredService<InvoicesSubscription>();
        Assert.Equal<IStreamSubscription>([orders, invoices], host.Services.GetServices<IStreamSubscription>());
        Assert.Equal<IHostedService>([orders, invoices], host.Services.GetServices<IHostedService>().OfType<RecordingSubscription>());

        await host.StartAsync(TestContext.Current.CancellationToken);
        await host.StopAsync(TestContext.Current.CancellationToken);

        Assert.All(new RecordingSubscription[] { orders, invoices }, subscription =>
        {
            Assert.Equal(1, subscription.StartCount);
            Assert.Equal(1, subscription.StopCount);
        });
    }

    [Fact]
    public void Registers_one_registry_and_one_metrics_instance_for_many_subscriptions()
    {
        var builder = CreateBuilder("Orders", "Invoices");

        Assert.Single(builder.Services, d => d.ServiceType == typeof(CosmosDBCatchupSubscriptionStatusRegistry));
        Assert.Single(builder.Services, d => d.ServiceType == typeof(CosmosDBCatchupSubscriptionMetrics));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Health_check_observes_the_registry_used_by_subscriptions(bool healthChecksFirst)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        if (healthChecksFirst)
        {
            builder.Services.AddHealthChecks().AddCosmosDBCatchupSubscriptions();
            builder.AddCosmosDBCatchupSubscription(new CosmosDBCatchupSubscriptionInfo("Orders", typeof(FakeSubscription)));
        }
        else
        {
            builder.AddCosmosDBCatchupSubscription(new CosmosDBCatchupSubscriptionInfo("Orders", typeof(FakeSubscription)));
            builder.Services.AddHealthChecks().AddCosmosDBCatchupSubscriptions();
        }

        Assert.Single(builder.Services, d => d.ServiceType == typeof(CosmosDBCatchupSubscriptionStatusRegistry));

        using var provider = builder.Services.BuildServiceProvider();
        provider.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>()
            .Report(new("Orders", CosmosDBCatchupSubscriptionState.Retrying, "failing"));

        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync(TestContext.Current.CancellationToken);

        var entry = Assert.Single(report.Entries);
        Assert.Equal("cosmosdb-catchup-subscriptions", entry.Key);
        Assert.Equal(HealthStatus.Degraded, entry.Value.Status);
    }

    [Fact]
    public void Health_check_registration_honors_custom_name_status_and_tags()
    {
        var builder = CreateBuilder("Orders");
        builder.Services.AddHealthChecks().AddCosmosDBCatchupSubscriptions("subs", HealthStatus.Unhealthy, ["ready"]);

        using var provider = builder.Services.BuildServiceProvider();
        var registration = Assert.Single(provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);

        Assert.Equal("subs", registration.Name);
        Assert.Equal(HealthStatus.Unhealthy, registration.FailureStatus);
        Assert.Contains("ready", registration.Tags);
    }

    [Fact]
    public void Metrics_use_the_registered_meter_factory()
    {
        var builder = CreateBuilder("Orders");
        builder.Services.AddMetrics();

        using var provider = builder.Services.BuildServiceProvider();
        var meterFactory = provider.GetRequiredService<IMeterFactory>();
        var published = new List<Instrument>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (instrument.Meter.Name == CosmosDBCatchupSubscriptionMetrics.MeterName)
                {
                    published.Add(instrument);
                }
            }
        };
        listener.Start();

        provider.GetRequiredService<CosmosDBCatchupSubscriptionMetrics>();

        // Other tests running in parallel can publish instruments for the same meter name without this factory.
        var fromFactory = published.Where(i => ReferenceEquals(i.Meter.Scope, meterFactory)).Select(i => i.Name).ToList();
        Assert.Equal(
            ["continuum.subscription.batch.duration", "continuum.subscription.batch.retries", "continuum.subscription.batches.processed",
             "continuum.subscription.errors", "continuum.subscription.events.processed", "continuum.subscription.state"],
            fromFactory.Order());
    }

    [Fact]
    public void Metrics_resolve_without_a_meter_factory()
    {
        var builder = CreateBuilder("Orders");
        IServiceCollection services = new ServiceCollection();
        foreach (var descriptor in builder.Services.Where(d => d.ServiceType != typeof(IMeterFactory)))
        {
            services.Add(descriptor);
        }

        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<IMeterFactory>());
        Assert.NotNull(provider.GetRequiredService<CosmosDBCatchupSubscriptionMetrics>());
    }
}
