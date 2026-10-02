using Continuum.CSharpFunctionalExtensions;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.EventSourcing;
using Orleans.Serialization;

namespace Continuum.Domain.Orleans;

/// <summary>A grain whose state machine backed state is built from events.</summary>
public interface IEventDrivenStateMachineGrain : IGrainWithStringKey
{
    /// <summary>FOR TESTING ONLY. Applies the given events to set up the initial state for Given/When/Then tests.</summary>
    /// <param name="events">The given events used to set up the initial state.</param>
    /// <returns>A task that completes when the events have been applied.</returns>
    Task LoadGivenEvents(params object[] events);
}

/// <summary>A journaled grain whose <see cref="EventDrivenStateMachineState{TState, TEventBase, TStatus, TCommand}"/> is changed through sessions.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public abstract class EventDrivenStateMachineGrain<TState, TEventBase, TStatus, TCommand> : JournaledGrain<TState, TEventBase>,
    IEventDrivenStateMachineGrain,
    IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand>
    where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : StateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    /// <summary>The message used when events cannot be saved to storage.</summary>
    protected const string StorageErrorMessage = "Storage temporarily unavailable. Unable to save changes.";

    /// <summary>The error returned when events cannot be saved to storage.</summary>
    protected static readonly Error StorageFailureError = RequestErrors.NewUnavailable(StorageErrorMessage);

    /// <summary>The copier used to isolate session state from grain state.</summary>
    protected readonly DeepCopier<TState> _deepCopier;

    /// <summary>The type mapper used to name events.</summary>
    protected readonly ITypeMapper _typeMapper;

    /// <inheritdoc/>

    public ILogger Logger { get; }


    /// <summary>Initializes a new instance of the <see cref="EventDrivenStateMachineGrain{TState, TEventBase, TStatus, TCommand}"/> class.</summary>
    /// <param name="serviceProvider">The service provider used to resolve the deep copier and type mapper.</param>
    /// <param name="connectionName">The service key of the <see cref="ITypeMapper"/> to use.</param>
    protected EventDrivenStateMachineGrain(IServiceProvider serviceProvider, object connectionName)
    {
        _deepCopier = serviceProvider.GetRequiredService<DeepCopier<TState>>();
        _typeMapper = serviceProvider.GetRequiredKeyedService<ITypeMapper>(connectionName);
        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
    }

    /// <summary>
    /// FOR TESTING ONLY!!! Used for Given, When, Then tests.
    /// </summary>
    /// <param name="events">The given events used to setup the initial state.</param>
    /// <exception cref="InvalidOperationException">Thrown if applying the events fails.</exception>
    public async Task LoadGivenEvents(params object[] events)
    {
        var typedEvents = new List<TEventBase>();
        foreach (var evt in events)
        {
            if (evt is TEventBase typedEvent)
            {
                typedEvents.Add(typedEvent);
            }
            else
            {
                throw new ArgumentException($"Event must be of type {typeof(TEventBase).Name}", nameof(events));
            }
        }
        var success = await RaiseConditionalEvents(typedEvents);
        if (!success)
        {
            throw new InvalidOperationException("Failed to apply given events.");
        }
    }

    /// <inheritdoc/>
    public IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand> CreateSession()
        => new EventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>(_deepCopier, State);

    /// <inheritdoc/>
    public async Task<Result<ISaveChangesResponse>> SaveChanges(IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand> session)
    {
        if (session.UncommittedEvents.Count == 0)
        {
            ISaveChangesResponse noChangesResponse = new SaveChangesResponse(SaveChangesResponse.NoChanges, Version);
            return Result.Success(noChangesResponse);
        }

        var success = await RaiseConditionalEvents(session.UncommittedEvents);
        if (!success)
        {
            return Result.Failure<ISaveChangesResponse>(StorageFailureError);
        }

        var changes = new List<IChange>(session.UncommittedEvents.Count);
        foreach (var evt in session.UncommittedEvents)
        {
            var typeName = _typeMapper.GetTypeName(evt.GetType());
            changes.Add(new Change(typeName, evt));
        }

        ISaveChangesResponse changesResponse = new SaveChangesResponse(changes, Version);
        return Result.Success(changesResponse);
    }

    /// <inheritdoc/>
    protected override void TransitionState(TState state, TEventBase evt)
    {
        if (state == TentativeState)
        {
            return;
        }
        state.When(evt);
    }
}
