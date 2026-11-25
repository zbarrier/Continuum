using System.Text.Json;

using Continuum.Orleans.Events;

using Orleans.Runtime;

namespace Continuum.Orleans.Tests;

public class StreamTargetsTests
{
    private static readonly GrainId Source = GrainId.Create("source", "1");
    private static readonly GrainId First = GrainId.Create("target", "1");
    private static readonly GrainId Second = GrainId.Create("target", "2");

    [Fact]
    public void Format_Returns_Null_When_No_Targets_Remain()
    {
        Assert.Null(StreamTargets.Format(Source, null));
        Assert.Null(StreamTargets.Format(Source, []));
        Assert.Null(StreamTargets.Format(Source, [StreamTarget.Transaction(Source), StreamTarget.Event(Source)]));
    }

    [Fact]
    public void Format_Splits_Targets_By_Scope_Without_Duplicates()
    {
        var formatted = StreamTargets.Format(Source,
            [StreamTarget.Transaction(First), StreamTarget.Event(Second), StreamTarget.Transaction(First), StreamTarget.Event(Second)]);

        Assert.NotNull(formatted);
        Assert.Equal([First.ToString()], formatted.TransactionScoped);
        Assert.Equal([Second.ToString()], formatted.EventScoped);
    }

    [Fact]
    public void Format_Keeps_A_Target_Named_In_Both_Scopes_Only_As_Transaction_Scoped()
    {
        var eventFirst = StreamTargets.Format(Source, [StreamTarget.Event(First), StreamTarget.Transaction(First)]);
        var transactionFirst = StreamTargets.Format(Source, [StreamTarget.Transaction(First), StreamTarget.Event(First)]);

        foreach (var formatted in new[] { eventFirst, transactionFirst })
        {
            Assert.NotNull(formatted);
            Assert.Equal([First.ToString()], formatted.TransactionScoped);
            Assert.Empty(formatted.EventScoped);
        }
    }

    [Fact]
    public void Event_Metadata_Json_Round_Trips_Both_Target_Scopes()
    {
        var metadata = EventMetadata.Create(null, null, null, null, null, ["whole/1"], ["single/1", "single/2"]);

        var read = JsonSerializer.Deserialize<EventMetadata>(JsonSerializer.Serialize(metadata));

        Assert.NotNull(read);
        Assert.Equal(["whole/1"], read.StreamTargets);
        Assert.Equal(["single/1", "single/2"], read.EventScopedStreamTargets);
    }
}
