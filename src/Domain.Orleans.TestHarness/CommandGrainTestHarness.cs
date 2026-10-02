using Continuum.CSharpFunctionalExtensions;

using Orleans.TestingHost;

namespace Continuum.Domain.Orleans.TestHarness;

/// <summary>Given/When/Then harness for commands that return an <see cref="EventSourcedCommandResponseBase"/>.</summary>
/// <typeparam name="TEventBase">The base type of events applied to the grain state.</typeparam>
public abstract class CommandGrainTestHarness<TEventBase> : GrainTestHarness<TEventBase>
    where TEventBase : class
{
    /// <summary>Initializes a new instance of the <see cref="CommandGrainTestHarness{TEventBase}"/> class.</summary>
    /// <param name="cluster">The test cluster hosting the grain.</param>
    /// <param name="grainId">The key of the grain under test.</param>
    protected CommandGrainTestHarness(TestCluster cluster, string grainId) : base(cluster, grainId) { }

    /// <summary>Runs the command under test and captures its result or exception.</summary>
    /// <typeparam name="TResponse">The command response type.</typeparam>
    /// <param name="func">The command to run.</param>
    /// <returns>A task that completes when the command has run.</returns>
    protected Task When<TResponse>(Func<Task<Result<TResponse>>> func) where TResponse : EventSourcedCommandResponseBase =>
        Capture(func);

    /// <summary>Asserts the command succeeded and checks the changes it produced.</summary>
    /// <typeparam name="TResponse">The expected command response type.</typeparam>
    /// <param name="changes">Assertions over the produced changes.</param>
    protected void Then<TResponse>(Action<List<IChange>> changes) where TResponse : EventSourcedCommandResponseBase
    {
        if (GetSuccessValue() is TResponse response)
        {
            changes(response.Changes?.ToList() ?? []);
            return;
        }
        throw new Exception("Unexpected response type.");
    }

    /// <summary>Asserts the command succeeded and checks its response.</summary>
    /// <typeparam name="TResponse">The expected command response type.</typeparam>
    /// <param name="updates">Assertions over the response, or <see langword="null"/> if it is not a <typeparamref name="TResponse"/>.</param>
    protected void Then<TResponse>(Action<TResponse?> updates) where TResponse : EventSourcedCommandResponseBase
    {
        updates(GetSuccessValue() as TResponse);
    }
}
