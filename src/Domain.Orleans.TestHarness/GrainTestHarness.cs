using System.Runtime.ExceptionServices;

using Continuum.CSharpFunctionalExtensions;

using Orleans.TestingHost;

using Xunit;

namespace Continuum.Domain.Orleans.TestHarness;

/// <summary>Base Given/When/Then harness for testing event-sourced grains in a <see cref="TestCluster"/>.</summary>
/// <remarks>
///     Each test follows a single Given/When/Then flow: call <c>Given</c> to set up the initial state, call <c>When</c> exactly
///     once to run the operation under test, then assert the outcome with <c>Then</c>, <c>ThenError</c> or <c>ThenException</c>.
///     Create a new harness for every test (xUnit does this by default by creating a new test class instance per test).
///     When the operation under test runs a multi-stream transaction, set up the other grains in it with
///     <c>GivenParticipant</c> and check what was saved to them with <c>ThenParticipant</c> and <c>ThenNoChanges</c>.
/// </remarks>
/// <typeparam name="TEventBase">The base type of events applied to the grain state.</typeparam>
public abstract class GrainTestHarness<TEventBase>
    where TEventBase : class
{
    private readonly Dictionary<(Type GrainInterface, string Key), IEventLogGrain> _participants = new();
    private readonly Dictionary<(Type GrainInterface, string Key), int> _participantVersions = new();
    private int _grainUnderTestVersion;
    private Result<object>? _result;
    private Exception? _exception;
    private bool _whenCalled;

    /// <summary>Initializes a new instance of the <see cref="GrainTestHarness{TEventBase}"/> class.</summary>
    /// <param name="cluster">The test cluster hosting the grain.</param>
    /// <param name="grainId">The key of the grain under test.</param>
    protected GrainTestHarness(TestCluster cluster, string grainId)
    {
        Cluster = cluster;
        GrainId = grainId;
    }

    /// <summary>The test cluster hosting the grain.</summary>
    protected TestCluster Cluster { get; }

    /// <summary>The key of the grain under test.</summary>
    protected string GrainId { get; }

    /// <summary>Loads the given events into the grain under test.</summary>
    /// <param name="events">The events used to set up the initial state.</param>
    /// <returns>A task that completes when the events have been applied.</returns>
    protected abstract Task LoadGivenEvents(TEventBase[] events);

    /// <summary>Gets whether the grain under test saves its events, so <see cref="ThenNoChanges()"/> can check it.</summary>
    private protected virtual bool GrainUnderTestHasEventLog => false;

    /// <summary>Gets the saved version of the grain under test.</summary>
    /// <returns>The saved version.</returns>
    private protected virtual Task<int> GetGrainUnderTestSavedVersion() => Task.FromResult(0);

    /// <summary>Gets the saved events of the grain under test after a version.</summary>
    /// <param name="version">The version to read after.</param>
    /// <returns>The saved events.</returns>
    private protected virtual Task<object[]> GetGrainUnderTestSavedEventsSince(int version) => Task.FromResult(Array.Empty<object>());

    /// <summary>Sets up the initial grain state from the given events.</summary>
    /// <param name="events">The events used to set up the initial state.</param>
    /// <returns>A task that completes when the events have been applied.</returns>
    protected Task Given(params TEventBase[] events) =>
        events.Length == 0 ? Task.CompletedTask : LoadGivenEvents(events);

    /// <summary>
    ///     Sets up the initial state of another grain that takes part in the operation under test, such as a participant
    ///     in a multi-stream transaction. Only events saved after this are seen by <c>ThenParticipant</c> and
    ///     <c>ThenNoChanges</c>.
    /// </summary>
    /// <typeparam name="TGrain">The participant grain interface.</typeparam>
    /// <param name="key">The key of the participant grain.</param>
    /// <param name="events">The events used to set up the participant's initial state.</param>
    /// <returns>A task that completes when the events have been applied.</returns>
    /// <exception cref="InvalidOperationException"><c>When</c> has already been called in this test.</exception>
    protected async Task GivenParticipant<TGrain>(string key, params object[] events)
        where TGrain : IEventLogGrain
    {
        if (_whenCalled)
        {
            throw new InvalidOperationException("GivenParticipant must be called before When.");
        }
        var participant = GetParticipant<TGrain>(key);
        if (events.Length > 0)
        {
            await participant.LoadGivenEvents(events);
        }
    }

    /// <summary>Runs the operation under test and captures its result or exception.</summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="func">The operation to run.</param>
    /// <returns>A task that completes when the operation has run.</returns>
    /// <exception cref="InvalidOperationException">An operation has already been run by this harness; <c>When</c> can only be called once per test.</exception>
    protected async Task Capture<TValue>(Func<Task<Result<TValue>>> func) where TValue : class
    {
        if (_whenCalled)
        {
            throw new InvalidOperationException("When can only be called once per test.");
        }
        _whenCalled = true;

        if (GrainUnderTestHasEventLog)
        {
            _grainUnderTestVersion = await GetGrainUnderTestSavedVersion();
        }
        foreach (var (participantKey, participant) in _participants)
        {
            _participantVersions[participantKey] = await participant.GetSavedVersion();
        }

        try
        {
            _result = await func();
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    /// <summary>
    ///     Checks the events the operation under test saved to another grain, such as a participant in a multi-stream
    ///     transaction. A grain that was not set up with <c>GivenParticipant</c> is read from the start of its log.
    /// </summary>
    /// <typeparam name="TGrain">The participant grain interface.</typeparam>
    /// <param name="key">The key of the participant grain.</param>
    /// <param name="checkEvents">Assertions over the events saved to the participant.</param>
    /// <returns>A task that completes when the events have been checked.</returns>
    protected async Task ThenParticipant<TGrain>(string key, Action<IReadOnlyList<object>> checkEvents)
        where TGrain : IEventLogGrain
    {
        checkEvents(await GetParticipantEventsSinceWhen<TGrain>(key));
    }

    /// <summary>
    ///     Asserts that nothing was saved to the grain under test or to any grain set up with <c>GivenParticipant</c>,
    ///     such as after a multi-stream transaction was rolled back.
    /// </summary>
    /// <returns>A task that completes when every grain has been checked.</returns>
    protected async Task ThenNoChanges()
    {
        if (GrainUnderTestHasEventLog)
        {
            Assert.Empty(await GetGrainUnderTestSavedEventsSince(_grainUnderTestVersion));
        }
        foreach (var (participantKey, participant) in _participants)
        {
            Assert.Empty(await participant.GetSavedEventsSince(_participantVersions.GetValueOrDefault(participantKey)));
        }
    }

    /// <summary>
    ///     Asserts that nothing was saved to another grain. A grain that was not set up with <c>GivenParticipant</c> is
    ///     read from the start of its log.
    /// </summary>
    /// <typeparam name="TGrain">The participant grain interface.</typeparam>
    /// <param name="key">The key of the participant grain.</param>
    /// <returns>A task that completes when the grain has been checked.</returns>
    protected async Task ThenNoChanges<TGrain>(string key)
        where TGrain : IEventLogGrain
    {
        Assert.Empty(await GetParticipantEventsSinceWhen<TGrain>(key));
    }

    /// <summary>Asserts the command failed with an error of the given type and checks it.</summary>
    /// <typeparam name="TError">The expected error type.</typeparam>
    /// <param name="checkError">Assertions over the error.</param>
    protected void ThenError<TError>(Action<TError> checkError)
    {
        checkError(GetError<TError>());
    }

    /// <summary>Asserts the command failed with the expected error.</summary>
    /// <typeparam name="TError">The expected error type.</typeparam>
    /// <param name="expectedError">The expected error.</param>
    protected void ThenError<TError>(TError expectedError)
    {
        Assert.Equal(expectedError, GetError<TError>());
    }

    /// <summary>Asserts the command threw an exception of exactly the given type.</summary>
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
                throw new Exception("Test was successful but an error was expected.");
            }
            throw new Exception("Test failed but an exception was expected.");
        }
        throw new Exception("Unknown error occurred, both exception and result are null.");
    }

    /// <summary>Returns the captured success value, rethrowing a captured exception or failing on an error result.</summary>
    /// <returns>The success value of the captured result.</returns>
    protected object GetSuccessValue()
    {
        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }
        if (!_result.HasValue)
        {
            throw new Exception("Unknown error occurred, both exception and result are null.");
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

    private IEventLogGrain GetParticipant<TGrain>(string key)
        where TGrain : IEventLogGrain
    {
        var participantKey = (typeof(TGrain), key);
        if (!_participants.TryGetValue(participantKey, out var participant))
        {
            participant = Cluster.GrainFactory.GetGrain<TGrain>(key);
            _participants[participantKey] = participant;
        }
        return participant;
    }

    private async Task<object[]> GetParticipantEventsSinceWhen<TGrain>(string key)
        where TGrain : IEventLogGrain
    {
        if (!_whenCalled)
        {
            throw new InvalidOperationException("When must be called before checking participants.");
        }
        var version = _participantVersions.GetValueOrDefault((typeof(TGrain), key));
        return await GetParticipant<TGrain>(key).GetSavedEventsSince(version);
    }

    private TError GetError<TError>()
    {
        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }
        if (!_result.HasValue)
        {
            throw new Exception("Unknown error occurred, both exception and result are null.");
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
