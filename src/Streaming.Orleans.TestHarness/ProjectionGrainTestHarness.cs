using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Streams;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Core.Internal;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.TestingHost;

using Xunit;

namespace Continuum.Streaming.Orleans.TestHarness;

/// <summary>Given/When/Then harness for testing a <see cref="ProjectionGrain{TGrain, TState, TContext}"/> in a <see cref="TestCluster"/>.</summary>
/// <remarks>
/// <para>
///     Each test follows a single Given/When/Then flow and checks one of two things:
///     <list type="bullet">
///         <item>
///             <description>
///                 The projected state: call <c>Given</c> to deliver events, then check the stored state with
///                 <see cref="Then(Action{TState})"/>. <c>When</c> is not called.
///             </description>
///         </item>
///         <item>
///             <description>
///                 A query: call <c>Given</c> to deliver events, call <c>When</c> exactly once to run the query, then
///                 check its outcome with <see cref="Then{TValue}(Action{TValue})"/>, <c>ThenError</c> or
///                 <c>ThenException</c>.
///             </description>
///         </item>
///     </list>
///     Checking both in one test is rejected; write two tests. Create a new harness for every test (xUnit does this by
///     default by creating a new test class instance per test).
/// </para>
/// <para>
///     <c>Given</c> delivers each event in its own batch through <see cref="IProjectionGrain.OnNextBatchAsync"/>, the
///     path catch-up subscriptions use, and returns once the batch is applied, so no polling is needed. Stream provider
///     wiring, such as an implicit subscription, is not exercised and is worth one test of its own.
/// </para>
/// <para>
///     Because delivery is at least once, two checks run by default:
///     <list type="bullet">
///         <item>
///             <description>
///                 <see cref="CheckDuplicateDelivery"/> delivers every batch a second time straight after the first and
///                 fails if the second delivery writes the state.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="CheckReplayAfterReactivation"/> deactivates the grain at the end of every <c>Given</c>, so
///                 its state is read back through the configured storage and serializer, then delivers every batch given
///                 so far but the last again and fails if any of them writes the state. <c>When</c> then queries the
///                 reloaded state, so a property that does not survive storage fails the test's own assertions.
///             </description>
///         </item>
///     </list>
///     A write is detected from the ETag of the state in storage, so the grain must run on an in-process silo, which is
///     the <see cref="TestCluster"/> default.
/// </para>
/// </remarks>
/// <typeparam name="TGrainInterface">The grain interface used to query the projection.</typeparam>
/// <typeparam name="TGrain">The projection grain implementation.</typeparam>
/// <typeparam name="TState">The projection state, held in the grain's <c>[PersistentState]</c> constructor parameter.</typeparam>
public abstract class ProjectionGrainTestHarness<
    TGrainInterface,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGrain,
    TState>
    where TGrainInterface : IGrainWithStringKey
    where TGrain : Grain, IProjectionGrain, TGrainInterface
    where TState : class, IProjectionState, new()
{
    private static long _position;

    private readonly List<List<IStreamedEvent<object>>> _batches = [];
    private (string StateName, IGrainStorage Storage)? _storage;
    private TState? _state;
    private Result<object>? _result;
    private Exception? _exception;
    private bool _whenCalled;

    /// <summary>Initializes a new instance of the <see cref="ProjectionGrainTestHarness{TGrainInterface, TGrain, TState}"/> class.</summary>
    /// <param name="cluster">The test cluster hosting the grain.</param>
    /// <param name="grainKey">The key of the grain under test.</param>
    protected ProjectionGrainTestHarness(TestCluster cluster, string grainKey)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentException.ThrowIfNullOrEmpty(grainKey);

        Cluster = cluster;
        GrainKey = grainKey;
    }

    /// <summary>The test cluster hosting the grain.</summary>
    protected TestCluster Cluster { get; }

    /// <summary>The key of the grain under test.</summary>
    protected string GrainKey { get; }

    /// <summary>The grain under test.</summary>
    protected TGrainInterface Grain => Cluster.GrainFactory.GetGrain<TGrainInterface>(GrainKey);

    /// <summary>
    ///     Gets or sets whether <c>Given</c> delivers every batch a second time and fails if that writes the state.
    ///     Defaults to <see langword="true"/>.
    /// </summary>
    protected bool CheckDuplicateDelivery { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether every <c>Given</c> ends by reactivating the grain, redelivering every batch given so far but
    ///     the last, and failing if that writes the state. Defaults to <see langword="true"/>.
    /// </summary>
    protected bool CheckReplayAfterReactivation { get; set; } = true;

    /// <summary>Returns a position after every position handed out so far, in any test.</summary>
    /// <remarks>Use it for the cursor of an envelope built by the test, so its watermark always moves forward.</remarks>
    /// <returns>The next position.</returns>
    protected static long NextPosition() => Interlocked.Increment(ref _position);

    /// <summary>
    ///     Delivers events to the projection, each in its own batch, runs the at-least-once checks, then reads the stored
    ///     state for <see cref="Then(Action{TState})"/>. It can be called more than once.
    /// </summary>
    /// <param name="events">
    ///     The events, in order. An event is wrapped by <see cref="CreateStreamedEvent"/> unless it already is an
    ///     <see cref="IStreamedEvent{T}"/>, which is delivered as is.
    /// </param>
    /// <returns>A task that completes when the events have been applied and checked.</returns>
    /// <exception cref="InvalidOperationException"><c>When</c> has already been called in this test.</exception>
    protected async Task Given(params object[] events)
    {
        ArgumentNullException.ThrowIfNull(events);
        if (_whenCalled)
        {
            throw new InvalidOperationException("Given must be called before When.");
        }

        foreach (var @event in events)
        {
            List<IStreamedEvent<object>> batch = [@event as IStreamedEvent<object> ?? CreateStreamedEvent(@event, NextPosition())];
            _batches.Add(batch);
            await DeliverAsync(batch);

            if (CheckDuplicateDelivery)
            {
                var eTag = await ReadETagAsync();
                await DeliverAsync(batch);
                if (await ReadETagAsync() != eTag)
                {
                    Assert.Fail(
                        $"Delivering batch {_batches.Count} a second time wrote the projection state, so an event that " +
                        "had already been applied was applied again.");
                }
            }
        }

        if (CheckReplayAfterReactivation && _batches.Count > 0)
        {
            await ReplayAfterReactivationAsync();
        }

        _state = (await ReadStoredStateAsync()).State ?? new TState();
    }

    /// <summary>Runs the query under test and captures its result or exception.</summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="query">The query to run.</param>
    /// <returns>A task that completes when the query has run.</returns>
    /// <exception cref="InvalidOperationException"><c>When</c> has already been called; it can only be called once per test.</exception>
    protected async Task When<TValue>(Func<Task<Result<TValue>>> query) where TValue : class
    {
        ArgumentNullException.ThrowIfNull(query);
        if (_whenCalled)
        {
            throw new InvalidOperationException("When can only be called once per test.");
        }
        _whenCalled = true;

        try
        {
            _result = await query();
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    /// <summary>Checks the projection state stored after the last <c>Given</c>, in a test that runs no query.</summary>
    /// <param name="checkState">Assertions over the state.</param>
    /// <exception cref="InvalidOperationException"><c>Given</c> was not called, or <c>When</c> was.</exception>
    protected void Then(Action<TState> checkState)
    {
        ArgumentNullException.ThrowIfNull(checkState);
        if (_whenCalled)
        {
            throw new InvalidOperationException(
                "This test ran a query, so check its result with Then<TValue>. Check the state in a separate test that " +
                "does not call When.");
        }
        checkState(_state ?? throw new InvalidOperationException("Given must be called before checking the state."));
    }

    /// <summary>Asserts the query succeeded and checks the returned value.</summary>
    /// <typeparam name="TValue">The expected value type.</typeparam>
    /// <param name="checkValue">Assertions over the value.</param>
    /// <exception cref="InvalidOperationException"><c>When</c> was not called.</exception>
    protected void Then<TValue>(Action<TValue> checkValue)
    {
        ArgumentNullException.ThrowIfNull(checkValue);
        if (!_whenCalled)
        {
            throw new InvalidOperationException(
                "No query ran, so there is no result to check. Check the projection state with Then(state => ...), or " +
                "call When to run a query.");
        }
        if (GetSuccessValue() is TValue value)
        {
            checkValue(value);
            return;
        }
        throw new Exception($"The query did not return a {typeof(TValue).Name}.");
    }

    /// <summary>Asserts the query failed with an error of the given type and checks it.</summary>
    /// <typeparam name="TError">The expected error type.</typeparam>
    /// <param name="checkError">Assertions over the error.</param>
    protected void ThenError<TError>(Action<TError> checkError)
    {
        ArgumentNullException.ThrowIfNull(checkError);
        checkError(GetError<TError>());
    }

    /// <summary>Asserts the query failed with the expected error.</summary>
    /// <typeparam name="TError">The expected error type.</typeparam>
    /// <param name="expectedError">The expected error.</param>
    protected void ThenError<TError>(TError expectedError)
    {
        Assert.Equal(expectedError, GetError<TError>());
    }

    /// <summary>Asserts the query threw an exception of exactly the given type.</summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    protected void ThenException<TException>() where TException : Exception
    {
        if (_exception is not null)
        {
            Assert.Equal(typeof(TException), _exception.GetType());
            return;
        }
        if (_result.HasValue)
        {
            if (_result.Value.IsSuccess)
            {
                throw new Exception("Test was successful but an exception was expected.");
            }
            throw new Exception("Test failed but an exception was expected.");
        }
        throw new Exception("When was not called, or both its exception and result are null.");
    }

    /// <summary>Wraps an event the way a subscription delivers it.</summary>
    /// <remarks>
    ///     The default envelope has the event's CLR type name as its event type, a <c>given-events</c> stream, a
    ///     <c>given</c> source connection and topic with partition <c>0</c>, and a fixed timestamp. Override when the
    ///     projection reads any of these.
    /// </remarks>
    /// <param name="event">The event.</param>
    /// <param name="position">The event's position, after every position handed out before it.</param>
    /// <returns>The event as delivered to the projection.</returns>
    protected virtual IStreamedEvent<object> CreateStreamedEvent(object @event, long position)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            @event.GetType().Name,
            new StreamName("given-events", "given", null, "events"),
            streamVersion: 0,
            new StreamedEventCursor("given", "given", "0", position, 0),
            new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            @event);
    }

    private Task DeliverAsync(List<IStreamedEvent<object>> batch)
        => Grain.AsReference<IProjectionGrain>().OnNextBatchAsync(batch);

    private async Task ReplayAfterReactivationAsync()
    {
        await Grain.AsReference<IGrainManagementExtension>().DeactivateOnIdle();

        // An empty batch activates the grain, which reads its state from storage, without applying anything.
        await DeliverAsync([]);
        var eTag = await ReadETagAsync();

        for (var i = 0; i < _batches.Count - 1; i++)
        {
            await DeliverAsync(_batches[i]);
            if (await ReadETagAsync() != eTag)
            {
                Assert.Fail(
                    $"Delivering batch {i + 1} again after the grain was reactivated wrote the projection state, so the " +
                    "positions already applied were not restored from storage.");
            }
        }
    }

    private async Task<string?> ReadETagAsync() => (await ReadStoredStateAsync()).ETag;

    private async Task<GrainState<TState>> ReadStoredStateAsync()
    {
        var (stateName, storage) = _storage ??= ResolveStorage();
        var state = new GrainState<TState>();
        await storage.ReadStateAsync(stateName, Grain.GetGrainId(), state);
        return state;
    }

    private (string StateName, IGrainStorage Storage) ResolveStorage()
    {
        var attribute = typeof(TGrain).GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Where(parameter => parameter.ParameterType == typeof(IPersistentState<TState>))
            .Select(parameter => parameter.GetCustomAttribute<PersistentStateAttribute>())
            .FirstOrDefault(attribute => attribute is not null)
            ?? throw new InvalidOperationException(
                $"{typeof(TGrain).Name} has no [PersistentState] constructor parameter of type IPersistentState<{typeof(TState).Name}>.");

        var services = (Cluster.Primary as InProcessSiloHandle)?.ServiceProvider
            ?? throw new InvalidOperationException("The harness reads the projection state from storage, which needs an in-process silo.");
        var storageName = attribute.StorageName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;
        var storage = services.GetKeyedService<IGrainStorage>(storageName)
            ?? throw new InvalidOperationException($"No grain storage named '{storageName}' is registered.");
        return (attribute.StateName, storage);
    }

    private object GetSuccessValue()
    {
        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }
        if (!_result.HasValue)
        {
            throw new Exception("When was not called, or both its exception and result are null.");
        }
        if (_result.Value.IsFailure)
        {
            if (_result.Value.Error is Error actualError)
            {
                throw new Exception(actualError.ToString());
            }
            throw new Exception("Unknown error occurred.");
        }
        return _result.Value.Value;
    }

    private TError GetError<TError>()
    {
        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }
        if (!_result.HasValue)
        {
            throw new Exception("When was not called, or both its exception and result are null.");
        }
        if (_result.Value.IsSuccess)
        {
            throw new Exception("Test was successful but an error was expected.");
        }
        if (_result.Value.Error is TError actualError)
        {
            return actualError;
        }
        throw new Exception($"Test was not successful, but the result.Error is not the expected type. Type: {_result.Value.Error.GetType().Name}");
    }
}
