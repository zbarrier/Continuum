using Continuum.CSharpFunctionalExtensions;

using Orleans.TestingHost;

using Xunit;

namespace Continuum.Domain.Orleans.TestHarness;

/// <summary>Base Given/When/Then harness for testing event-sourced grains in a <see cref="TestCluster"/>.</summary>
/// <typeparam name="TEventBase">The base type of events applied to the grain state.</typeparam>
public abstract class GrainTestHarness<TEventBase>
    where TEventBase : class
{
    private Result<object>? _result;
    private Exception? _exception;

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

    /// <summary>Sets up the initial grain state from the given events.</summary>
    /// <param name="events">The events used to set up the initial state.</param>
    /// <returns>A task that completes when the events have been applied.</returns>
    protected Task Given(params TEventBase[] events) =>
        events.Length == 0 ? Task.CompletedTask : LoadGivenEvents(events);

    /// <summary>Runs the operation under test and captures its result or exception.</summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="func">The operation to run.</param>
    /// <returns>A task that completes when the operation has run.</returns>
    protected async Task Capture<TValue>(Func<Task<Result<TValue>>> func) where TValue : class
    {
        try
        {
            _result = await func();
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
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
            throw _exception;
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

    private TError GetError<TError>()
    {
        if (_exception is not null)
        {
            throw _exception;
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
