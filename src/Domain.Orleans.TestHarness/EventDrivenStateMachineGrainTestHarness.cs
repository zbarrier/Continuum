using Orleans.TestingHost;

namespace Continuum.Domain.Orleans.TestHarness;

/// <summary>Given/When/Then harness for an <see cref="EventDrivenStateMachineGrain{TState, TEventBase, TStatus, TCommand}"/>.</summary>
/// <typeparam name="TGrainInterface">The grain interface.</typeparam>
/// <typeparam name="TGrain">The grain implementation.</typeparam>
/// <typeparam name="TGrainState">The grain state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the grain state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public abstract class EventDrivenStateMachineGrainTestHarness<TGrainInterface, TGrain, TGrainState, TEventBase, TStatus, TCommand>
	: CommandGrainTestHarness<TEventBase>
	where TGrainInterface : IEventDrivenStateMachineGrain
	where TGrain : EventDrivenStateMachineGrain<TGrainState, TEventBase, TStatus, TCommand>, TGrainInterface
	where TGrainState : EventDrivenStateMachineState<TGrainState, TEventBase, TStatus, TCommand>, new()
	where TEventBase : StateMachineEvent<TCommand>
	where TStatus : notnull
	where TCommand : notnull
{
	/// <summary>Initializes a new instance of the <see cref="EventDrivenStateMachineGrainTestHarness{TGrainInterface, TGrain, TGrainState, TEventBase, TStatus, TCommand}"/> class.</summary>
	/// <param name="cluster">The test cluster hosting the grain.</param>
	/// <param name="grainId">The key of the grain under test.</param>
	protected EventDrivenStateMachineGrainTestHarness(TestCluster cluster, string grainId) : base(cluster, grainId) { }

	/// <inheritdoc/>
	protected override Task LoadGivenEvents(TEventBase[] events) =>
		Cluster.GrainFactory.GetGrain<TGrainInterface>(GrainId).LoadGivenEvents(events);
}
