using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests.Integration;
using Continuum.Streams;

using Grpc.Core;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests;

public class KurrentDBCatchupSubscriptionIsTransientTests
{
    private const string ConnectionName = "kurrentdb";

    public static TheoryData<Exception> TransientExceptions() =>
    [
        new TimeoutException(),
        new SiloUnavailableException(),
        new GatewayTooBusyException(),
        new HttpRequestException(),
        new RpcException(new Status(StatusCode.Unavailable, "")),
        new RpcException(new Status(StatusCode.DeadlineExceeded, "")),
        new RpcException(new Status(StatusCode.ResourceExhausted, "")),
        new RpcException(new Status(StatusCode.Aborted, "")),
        new InvalidOperationException("wrapper", new TimeoutException()),
        new AggregateException(new TimeoutException(), new HttpRequestException()),
    ];

    public static TheoryData<Exception> NonTransientExceptions() =>
    [
        new InvalidOperationException(),
        new ArgumentException(),
        new RpcException(new Status(StatusCode.PermissionDenied, "")),
        new RpcException(new Status(StatusCode.Internal, "")),
        new InvalidOperationException("wrapper", new ArgumentException()),
        new AggregateException(),
        new AggregateException(new TimeoutException(), new ArgumentException()),
    ];

    [Theory]
    [MemberData(nameof(TransientExceptions))]
    public async Task Transient_exceptions_are_retried(Exception exception)
    {
        await using var services = CreateServices();
        var subscription = new ProbeCatchupSubscription(services);

        Assert.True(subscription.CallIsTransient(exception));
    }

    [Theory]
    [MemberData(nameof(NonTransientExceptions))]
    public async Task Other_exceptions_are_not_retried(Exception exception)
    {
        await using var services = CreateServices();
        var subscription = new ProbeCatchupSubscription(services);

        Assert.False(subscription.CallIsTransient(exception));
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<KurrentDBCatchupSubscriptionStatusRegistry>();
        services.AddSingleton(sp => new KurrentDBCatchupSubscriptionMetrics(sp.GetRequiredService<KurrentDBCatchupSubscriptionStatusRegistry>()));
        services.AddKeyedSingleton(ConnectionName, (_, _) => new KurrentDBClient(KurrentDBClientSettings.Create("kurrentdb://localhost:2113?tls=false")));
        services.AddOptions<KurrentDBCatchupSubscriptionOptions>(ProbeCatchupSubscription.Name)
            .Configure(options =>
            {
                options.ConnectionName = ConnectionName;
                options.CheckpointStore = new KurrentDBCatchupSubscriptionIntegrationTests.InMemoryCheckpointStore();
            });
        return services.BuildServiceProvider();
    }

    private sealed class ProbeCatchupSubscription(IServiceProvider serviceProvider)
        : KurrentDBCatchupSubscription<ProbeCatchupSubscription, KurrentDBCatchupSubscriptionOptions>(serviceProvider)
    {
        public override string StreamName => "probe";

        public override IEventFilter? FilterOptions => null;

        public bool CallIsTransient(Exception exception) => IsTransient(exception);

        protected override Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents) => Task.CompletedTask;
    }
}
