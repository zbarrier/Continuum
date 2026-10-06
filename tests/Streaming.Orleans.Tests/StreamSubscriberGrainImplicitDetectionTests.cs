using Continuum.Streaming.Orleans;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Runtime;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.Tests;

/// <summary>
///     Covers how <see cref="StreamSubscriberGrain{TGrain}" /> classifies a stream as implicitly or explicitly
///     subscribed, which is what decides whether the grain subscribes itself or waits for the runtime.
/// </summary>
/// <remarks>
///     Exercised without a cluster because the classification is a pure function of the attributes on the grain type
///     and the stream being considered. Getting it wrong is not a visible failure in an integration test: a stream
///     wrongly treated as explicit produces a second, duplicate subscription that still delivers, so the events
///     arrive and only the duplication is wrong. Asserting on the decision directly is what makes that observable.
/// </remarks>
public class StreamSubscriberGrainImplicitDetectionTests
{
    [Fact]
    public void Treats_A_Stream_As_Explicit_When_The_Grain_Declares_No_Implicit_Subscription()
    {
        var isImplicit = ExplicitOnlyGrain.Classify(StreamId.Create("orders", "key"));

        Assert.False(isImplicit);
    }

    [Fact]
    public void Treats_A_Stream_As_Implicit_When_Its_Namespace_Matches_The_Attribute()
    {
        var isImplicit = SingleImplicitGrain.Classify(StreamId.Create("chat", "key"));

        Assert.True(isImplicit);
    }

    [Fact]
    public void Treats_A_Stream_As_Explicit_When_Its_Namespace_Does_Not_Match_The_Attribute()
    {
        // The grain is implicitly subscribed, but not to this stream. Classifying per stream rather than per grain
        // is what allows a grain to subscribe explicitly to streams the runtime does not cover for it.
        var isImplicit = SingleImplicitGrain.Classify(StreamId.Create("orders", "key"));

        Assert.False(isImplicit);
    }

    [Fact]
    public void Recognises_Every_Namespace_When_The_Grain_Declares_Several_Implicit_Subscriptions()
    {
        // Orleans allows the attribute to be applied more than once, and calls OnSubscribed for each match, so the
        // classification has to consider all of them rather than only the first.
        Assert.True(MultipleImplicitGrain.Classify(StreamId.Create("chat", "key")));
        Assert.True(MultipleImplicitGrain.Classify(StreamId.Create("orders", "key")));
        Assert.False(MultipleImplicitGrain.Classify(StreamId.Create("audit", "key")));
    }

    [Fact]
    public void Classifies_Only_The_Implicit_Namespace_When_A_Grain_Mixes_Implicit_And_Explicit_Subscriptions()
    {
        // The shape this abstraction exists to support: the runtime supplies "chat", and the grain subscribes to
        // "audit" itself. Treating the grain as wholly implicit would leave the explicit stream unsubscribed.
        Assert.True(MixedSubscriptionGrain.Classify(StreamId.Create("chat", "key")));
        Assert.False(MixedSubscriptionGrain.Classify(StreamId.Create("audit", "key")));
    }

    [Fact]
    public void Matches_Namespaces_Using_The_Attributes_Own_Predicate()
    {
        // Orleans expresses wildcard subscriptions through a regex attribute deriving from the plain one, and matches
        // it through the predicate the attribute exposes. Comparing namespace strings instead would classify these as
        // explicit and duplicate every subscription the runtime already made.
        Assert.True(RegexImplicitGrain.Classify(StreamId.Create("chat-eu", "key")));
        Assert.True(RegexImplicitGrain.Classify(StreamId.Create("chat-us", "key")));
        Assert.False(RegexImplicitGrain.Classify(StreamId.Create("orders-eu", "key")));
    }

    /// <summary>
    ///     Exposes the classification, which is protected because it is an implementation detail of subscribing.
    /// </summary>
    private abstract class TestSubscriberGrain<TGrain> : StreamSubscriberGrain<TGrain>
        where TGrain : TestSubscriberGrain<TGrain>
    {
        protected override ILogger Logger => NullLogger.Instance;

        protected override Task OnNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken? token)
            => Task.CompletedTask;

        internal static bool Classify(StreamId streamId) => IsImplicit(streamId);
    }

    private sealed class ExplicitOnlyGrain : TestSubscriberGrain<ExplicitOnlyGrain>;

    [ImplicitStreamSubscription("chat")]
    private sealed class SingleImplicitGrain : TestSubscriberGrain<SingleImplicitGrain>;

    [ImplicitStreamSubscription("chat")]
    [ImplicitStreamSubscription("orders")]
    private sealed class MultipleImplicitGrain : TestSubscriberGrain<MultipleImplicitGrain>;

    [ImplicitStreamSubscription("chat")]
    private sealed class MixedSubscriptionGrain : TestSubscriberGrain<MixedSubscriptionGrain>
    {
        protected override IEnumerable<StreamSubscriptionSource> Subscriptions =>
        [
            new("SomeProvider", StreamId.Create("audit", "key"))
        ];
    }

    [RegexImplicitStreamSubscription("chat-.*")]
    private sealed class RegexImplicitGrain : TestSubscriberGrain<RegexImplicitGrain>;
}
