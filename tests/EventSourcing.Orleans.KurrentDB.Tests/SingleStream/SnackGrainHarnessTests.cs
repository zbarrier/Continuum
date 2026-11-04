using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream;

/// <summary>
///     Given/When/Then tests for a snack, written with the grain test harness. Each test starts from events loaded
///     straight into a new grain, runs one command, and checks the events or the error it returns. These tests do not
///     exercise storage; they show how to test grain behavior.
/// </summary>
[Collection(ClusterCollection.Name)]
public class SnackGrainHarnessTests : EventDrivenGrainTestHarness<ISnackGrain, SnackGrain, SnackState, SnackEvent>
{
    private static readonly DateTimeOffset At = new(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);

    private readonly Guid _snackId;

    public SnackGrainHarnessTests(ClusterFixture fixture) : this(fixture, Guid.NewGuid()) { }

    private SnackGrainHarnessTests(ClusterFixture fixture, Guid snackId) : base(fixture.Cluster, snackId.ToString())
    {
        _snackId = snackId;
    }

    private ISnackGrain Snack => Cluster.GrainFactory.GetGrain<ISnackGrain>(GrainId);

    private SnackInitializedEvent Initialized(string name = "Apple") => new(_snackId, name, At);

    private SnackRemovedEvent Removed() => new(_snackId, At);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;

    private static void AssertValidationTarget(ValidationError error, string target, string code)
    {
        var entry = Assert.Single(error.Entries);
        Assert.Equal(target, entry.Target);
        Assert.Equal(code, entry.Code);
    }

    [Fact]
    public async Task Initialize_Creates_A_New_Snack()
    {
        await Given();

        await When(() => Snack.InitializeAsync(new SnackInitializeCommand("Apple")));

        Then<SnackInitializeResponse>(changes =>
        {
            var initialized = Assert.IsType<SnackInitializedEvent>(SingleEvent(changes));
            Assert.Equal(_snackId, initialized.Id);
            Assert.Equal("Apple", initialized.Name);
        });
    }

    [Fact]
    public async Task Initialize_Rejects_A_Snack_That_Already_Exists()
    {
        await Given(Initialized());

        await When(() => Snack.InitializeAsync(new SnackInitializeCommand("Apple")));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Initialize_Rejects_A_Snack_That_Was_Removed()
    {
        await Given(Initialized(), Removed());

        await When(() => Snack.InitializeAsync(new SnackInitializeCommand("Apple")));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Initialize_Rejects_An_Empty_Name()
    {
        await Given();

        await When(() => Snack.InitializeAsync(new SnackInitializeCommand(" ")));

        ThenError<ValidationError>(error => AssertValidationTarget(error, "Name", ValidationErrorCodes.NotEmpty));
    }

    [Fact]
    public async Task Initialize_Rejects_A_Name_That_Is_Too_Long()
    {
        await Given();

        await When(() => Snack.InitializeAsync(new SnackInitializeCommand(new string('a', SnackName.MaximumLength + 1))));

        ThenError<ValidationError>(error => AssertValidationTarget(error, "Name", ValidationErrorCodes.MaxLength));
    }

    [Fact]
    public async Task ChangeName_Renames_An_Existing_Snack()
    {
        await Given(Initialized());

        await When(() => Snack.ChangeNameAsync(new SnackChangeNameCommand("Orange")));

        Then<SnackChangeNameResponse>(changes =>
        {
            var changed = Assert.IsType<SnackNameChangedEvent>(SingleEvent(changes));
            Assert.Equal(_snackId, changed.Id);
            Assert.Equal("Orange", changed.Name);
        });
    }

    [Fact]
    public async Task ChangeName_Rejects_A_Snack_That_Was_Never_Initialized()
    {
        await Given();

        await When(() => Snack.ChangeNameAsync(new SnackChangeNameCommand("Orange")));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task ChangeName_Rejects_A_Snack_That_Was_Removed()
    {
        await Given(Initialized(), Removed());

        await When(() => Snack.ChangeNameAsync(new SnackChangeNameCommand("Orange")));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Remove_Removes_An_Existing_Snack()
    {
        await Given(Initialized());

        await When(() => Snack.RemoveAsync(new SnackRemoveCommand()));

        Then<SnackRemoveResponse>(changes =>
        {
            var removed = Assert.IsType<SnackRemovedEvent>(SingleEvent(changes));
            Assert.Equal(_snackId, removed.Id);
        });
    }

    [Fact]
    public async Task Remove_Rejects_A_Snack_That_Was_Never_Initialized()
    {
        await Given();

        await When(() => Snack.RemoveAsync(new SnackRemoveCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Remove_Rejects_A_Snack_That_Was_Already_Removed()
    {
        await Given(Initialized(), Removed());

        await When(() => Snack.RemoveAsync(new SnackRemoveCommand()));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }
}
