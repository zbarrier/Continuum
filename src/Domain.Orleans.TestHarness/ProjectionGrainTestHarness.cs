using Continuum.CSharpFunctionalExtensions;

using Orleans.TestingHost;

namespace Continuum.Domain.Orleans.TestHarness;

/// <summary>Given/When/Then harness for projection grains whose queries return a <typeparamref name="TProjection"/>.</summary>
/// <typeparam name="TGrainInterface">The projection grain interface.</typeparam>
/// <typeparam name="TProjection">The projection type returned by the grain.</typeparam>
public abstract class ProjectionGrainTestHarness<TGrainInterface, TProjection>
    : ProjectionGrainTestHarness<TGrainInterface, TProjection, object>
    where TGrainInterface : IEventDrivenGrain
    where TProjection : class
{
    /// <summary>Initializes a new instance of the <see cref="ProjectionGrainTestHarness{TGrainInterface, TProjection}"/> class.</summary>
    /// <param name="cluster">The test cluster hosting the grain.</param>
    /// <param name="grainId">The key of the grain under test.</param>
    protected ProjectionGrainTestHarness(TestCluster cluster, string grainId) : base(cluster, grainId) { }
}

/// <summary>Given/When/Then harness for projection grains whose queries return a <typeparamref name="TProjection"/>.</summary>
/// <typeparam name="TGrainInterface">The projection grain interface.</typeparam>
/// <typeparam name="TProjection">The projection type returned by the grain.</typeparam>
/// <typeparam name="TEventBase">The base type of events the projection handles.</typeparam>
public abstract class ProjectionGrainTestHarness<TGrainInterface, TProjection, TEventBase>
    : GrainTestHarness<TEventBase>
    where TGrainInterface : IEventDrivenGrain
    where TProjection : class
    where TEventBase : class
{
    /// <summary>Initializes a new instance of the <see cref="ProjectionGrainTestHarness{TGrainInterface, TProjection, TEventBase}"/> class.</summary>
    /// <param name="cluster">The test cluster hosting the grain.</param>
    /// <param name="grainId">The key of the grain under test.</param>
    protected ProjectionGrainTestHarness(TestCluster cluster, string grainId) : base(cluster, grainId) { }

    /// <summary>The projection grain under test.</summary>
    protected TGrainInterface Grain => Cluster.GrainFactory.GetGrain<TGrainInterface>(GrainId);

    /// <inheritdoc/>
    protected override Task LoadGivenEvents(TEventBase[] events) => Grain.LoadGivenEvents(events);

    /// <summary>Runs the query under test and captures its result or exception.</summary>
    /// <param name="func">The query to run.</param>
    /// <returns>A task that completes when the query has run.</returns>
    protected Task When(Func<Task<Result<TProjection>>> func) => Capture(func);

    /// <summary>Asserts the query succeeded and checks the returned projection.</summary>
    /// <param name="checkProjection">Assertions over the projection.</param>
    protected void Then(Action<TProjection> checkProjection)
    {
        if (GetSuccessValue() is TProjection projection)
        {
            checkProjection(projection);
            return;
        }
        throw new Exception("Unexpected projection type.");
    }
}
