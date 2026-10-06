namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

public static class Constants
{
    public const string PubSubStoreName = "PubSubStore";
    public const string StreamProviderName = "KurrentDBStream-V2";
    public const string ChatNamespace = "Test.Chat-V2";

    /// <summary>
    ///     A second stream provider consuming <c>$all</c>, used to read events written outside the stream provider.
    /// </summary>
    public const string AllStreamProviderName = "KurrentDBStream-All";

    /// <summary>
    ///     The KurrentDB stream name prefix the <c>$all</c> provider is filtered to, made unique per process.
    /// </summary>
    /// <remarks>
    ///     The <c>$all</c> subscription is global and the strategy allows exactly one queue, so every test in the
    ///     collection reads the same filtered log behind the same cursor. A fixed prefix would make each test replay
    ///     the events every earlier test appended, and with <c>StartFromNow = false</c> that backlog grows until
    ///     delivery no longer completes within a test's timeout. Scoping the prefix to the process keeps each run
    ///     reading only its own events.
    ///
    ///     The default stream id mapper splits a stream name on the first '-', so this must not contain one; the
    ///     separator is added where the stream name is composed.
    /// </remarks>
    public static readonly string EventSourcedStreamPrefix = $"eventsourced{Guid.NewGuid():N}";

    /// <summary>
    ///     The queue name of the <c>$all</c> provider, made unique per process.
    /// </summary>
    /// <remarks>
    ///     The receiver derives its checkpoint identity from the provider and queue name. The KurrentDB container is
    ///     reused between runs, so a fixed name would let one run resume from the position another run checkpointed
    ///     and skip the events under test.
    /// </remarks>
    public static readonly string AllStreamQueueName = $"all-stream-queue-{Guid.NewGuid():N}";

    /// <summary>
    ///     A third stream provider consuming <c>$all</c>, used by the implicit subscription tests.
    /// </summary>
    public const string ImplicitProviderName = "KurrentDBStream-Implicit";

    /// <summary>
    ///     The KurrentDB stream name prefix the implicit subscription provider is filtered to.
    /// </summary>
    /// <remarks>
    ///     Unlike <see cref="EventSourcedStreamPrefix" /> this is a compile time constant, because
    ///     <c>[ImplicitStreamSubscription]</c> takes the stream namespace as an attribute argument and an attribute
    ///     argument cannot be generated per run. Isolation between runs still holds: the fixture starts a fresh,
    ///     auto removed container each time, so the log never carries another run's events. Isolation between tests
    ///     comes from each test using its own grain key, which is also the stream key.
    ///
    ///     The default stream id mapper splits a stream name on the first '-', so this must not contain one.
    /// </remarks>
    public const string ImplicitStreamPrefix = "implicitprojection";

    /// <summary>
    ///     The queue name of the implicit subscription provider, made unique per process.
    /// </summary>
    public static readonly string ImplicitQueueName = $"implicit-queue-{Guid.NewGuid():N}";
}
