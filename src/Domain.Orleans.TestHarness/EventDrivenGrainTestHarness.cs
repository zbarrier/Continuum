using Orleans.TestingHost;

namespace Continuum.Domain.Orleans.TestHarness;

/// <summary>Given/When/Then harness for an <see cref="EventDrivenGrain{TState}"/> whose events derive from <see cref="object"/>.</summary>
/// <typeparam name="TGrainInterface">The grain interface.</typeparam>
/// <typeparam name="TGrain">The grain implementation.</typeparam>
/// <typeparam name="TGrainState">The grain state type.</typeparam>
public abstract class EventDrivenGrainTestHarness<TGrainInterface, TGrain, TGrainState>
	: EventDrivenGrainTestHarness<TGrainInterface, TGrain, TGrainState, object>
	where TGrainInterface : IEventDrivenGrain
	where TGrain : EventDrivenGrain<TGrainState, object>, TGrainInterface
	where TGrainState : EventDrivenState<TGrainState, object>, new()
{
	/// <summary>Initializes a new instance of the <see cref="EventDrivenGrainTestHarness{TGrainInterface, TGrain, TGrainState}"/> class.</summary>
	/// <param name="cluster">The test cluster hosting the grain.</param>
	/// <param name="grainId">The key of the grain under test.</param>
	protected EventDrivenGrainTestHarness(TestCluster cluster, string grainId) : base(cluster, grainId) { }
}

/// <summary>Given/When/Then harness for an <see cref="EventDrivenGrain{TState, TEventBase}"/>.</summary>
/// <typeparam name="TGrainInterface">The grain interface.</typeparam>
/// <typeparam name="TGrain">The grain implementation.</typeparam>
/// <typeparam name="TGrainState">The grain state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the grain state.</typeparam>
public abstract class EventDrivenGrainTestHarness<TGrainInterface, TGrain, TGrainState, TEventBase>
	: CommandGrainTestHarness<TEventBase>
	where TGrainInterface : IEventDrivenGrain
	where TGrain : EventDrivenGrain<TGrainState, TEventBase>, TGrainInterface
	where TGrainState : EventDrivenState<TGrainState, TEventBase>, new()
	where TEventBase : class
{
	/// <summary>Initializes a new instance of the <see cref="EventDrivenGrainTestHarness{TGrainInterface, TGrain, TGrainState, TEventBase}"/> class.</summary>
	/// <param name="cluster">The test cluster hosting the grain.</param>
	/// <param name="grainId">The key of the grain under test.</param>
	protected EventDrivenGrainTestHarness(TestCluster cluster, string grainId) : base(cluster, grainId) { }

	/// <inheritdoc/>
	protected override Task LoadGivenEvents(TEventBase[] events) =>
		Cluster.GrainFactory.GetGrain<TGrainInterface>(GrainId).LoadGivenEvents(events);
}
