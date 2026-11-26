using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Domain.Orleans.TestHarness;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.DescriptionOfWorkTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for submitting an acknowledged order's description of work, and revisions of it, for the
///     approval of the client who started the order. Each test uses a new order in the test tenant. The first test checks
///     what applying the slice's event does to the order state and the second checks every value of the event the slice
///     records; the behavior tests follow.
/// </summary>
public sealed class SubmitDescriptionOfWorkTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public SubmitDescriptionOfWorkTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingDescriptionOfWorkSubmitted_MakesTheOrderAwaitTheClientsApproval()
    {
        var state = new OrderState();
        var started = StartOrderDraftTestData.DraftStarted1(GrainId);
        state.When(started);
        state.When(SaveBorrowerAndAccessContactTestData.Saved1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));

        state.When(Submitted1(GrainId));

        Assert.Equivalent(new
        {
            state.StateMachine,
            Status = OrderStatus.DescriptionOfWorkAwaitingApproval,
            StartedBy = started.InitiatedBy,
            started.ClientId,
            HasBorrower = true,
            SupportingDocumentBlobPaths = new Dictionary<string, string>(),
            OrderNumber = (long?)null,
            NewOrderNoticeSent = false,
            AssignedTo = (string?)RequestedByUsers.Director1.GetTenantUserId(),
            VendorAssignment = (OrderVendorAssignment?)null,
            InspectionAt = (DateTimeOffset?)null,
            InspectionCompletedOn = (DateOnly?)null,
            Report = (OrderReport?)null,
            OrderShippedNoticeSent = false,
            DocumentRequests = new Dictionary<string, OrderDocumentRequest>(),
            DescriptionOfWork = (OrderDescriptionOfWork?)new OrderDescriptionOfWork(
                DescriptionOfWorkTestData.DescriptionOfWorkId, 1, RequestedByUsers.ClientUser1.GetTenantUserId(),
                Approved: false, NoticeSentVersion: null, ApprovedNoticeSent: false)
        }, state, strict: true);
    }

    [Fact]
    public void ApplyingARevision_KeepsTheNoticeOfTheEarlierVersion()
    {
        var state = new OrderState();
        state.When(StartOrderDraftTestData.DraftStarted1(GrainId));
        state.When(SubmitOrderTestData.Submitted1(GrainId));
        state.When(AssignOrderTestData.OrderAssigned1(GrainId));
        state.When(AcknowledgeOrderTestData.Acknowledged1(GrainId));
        state.When(Submitted1(GrainId));
        state.When(SubmittedNoticeSent(GrainId));

        state.When(Submitted(Command2, GrainId));

        Assert.Equal(
            new OrderDescriptionOfWork(DescriptionOfWorkTestData.DescriptionOfWorkId, 2, RequestedByUsers.ClientUser1.GetTenantUserId(),
                Approved: false, NoticeSentVersion: 1, ApprovedNoticeSent: false),
            state.DescriptionOfWork);
        Assert.Equal(OrderStatus.DescriptionOfWorkAwaitingApproval, state.Status);
    }

    [Fact]
    public async Task Submit_RecordsTheDescriptionOfWorkForTheApprovalOfTheClientWhoStartedTheOrder()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1));

        Then<SubmitDescriptionOfWorkResponse>(response =>
        {
            Assert.Equal(6, response.Version);
            var recorded = Assert.IsType<DescriptionOfWorkSubmitted>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Submitted1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Submit_CarriesTheOrdersNumber()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(AssignOrderNumberTestData.Assigned1(GrainId));

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1));

        Then<SubmitDescriptionOfWorkResponse>(response => Assert.Equal(
            AssignOrderNumberTestData.FirstOrderNumber,
            Assert.IsType<DescriptionOfWorkSubmitted>(SingleEvent(response.Changes)).OrderNumber));
    }

    [Fact]
    public async Task Submit_RemovesWhatIsNotSafeFromTheHtml()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1 with
        {
            DescriptionOfWorkHtml = "<p onclick=\"steal()\">Appraise the property.</p><script>steal()</script><img src=\"x\">"
        }));

        Then<SubmitDescriptionOfWorkResponse>(response => Assert.Equal(
            "<p>Appraise the property.</p>",
            Assert.IsType<DescriptionOfWorkSubmitted>(SingleEvent(response.Changes)).DescriptionOfWorkHtml));
    }

    /// <summary>Quill's CSS draws each bullet on the item's <c>ql-ui</c> span, so its classes must survive; other classes don't.</summary>
    [Fact]
    public async Task Submit_KeepsQuillsListAndFormattingClasses()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1 with
        {
            DescriptionOfWorkHtml = "<p class=\"ql-align-center mud-hidden\">Scope</p>"
                + "<ol><li data-list=\"bullet\" class=\"ql-indent-2\"><span class=\"ql-ui\" contenteditable=\"false\"></span>Copies of all leases.</li></ol>"
        }));

        Then<SubmitDescriptionOfWorkResponse>(response => Assert.Equal(
            "<p class=\"ql-align-center\">Scope</p>"
                + "<ol><li data-list=\"bullet\" class=\"ql-indent-2\"><span class=\"ql-ui\"></span>Copies of all leases.</li></ol>",
            Assert.IsType<DescriptionOfWorkSubmitted>(SingleEvent(response.Changes)).DescriptionOfWorkHtml));
    }

    [Fact]
    public async Task Submit_LetsTheAssigneeReviseTheDescriptionOfWorkWhileItWaitsForApproval()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(Submitted1(GrainId));

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command2));

        Then<SubmitDescriptionOfWorkResponse>(response =>
        {
            Assert.Equal(7, response.Version);
            Assert.Equal(2, Assert.IsType<DescriptionOfWorkSubmitted>(SingleEvent(response.Changes)).DescriptionOfWorkVersion);
        });
    }

    [Fact]
    public async Task Submit_LetsSomeoneWhoMayAssignOrdersSubmitForAnOrderAssignedToSomeoneElse()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenAcknowledged();

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1));

        Then<SubmitDescriptionOfWorkResponse>(response => Assert.Equal(
            RequestedByUsers.TenantAdmin1.GetTenantUserId(),
            Assert.IsType<DescriptionOfWorkSubmitted>(SingleEvent(response.Changes)).InitiatedBy));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.Coordinator1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Submit_RejectsAUserWhoIsNotTheAssigneeNorMayAssignOrders(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenAcknowledged();

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsAnOrderThatIsNotAcknowledgedYet()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(GrainId),
            SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
            SubmitOrderTestData.Submitted1(GrainId),
            AssignOrderTestData.OrderAssigned1(GrainId));

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.FailedPrecondition, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsARevisionOnceTheDescriptionOfWorkIsApproved()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(Submitted1(GrainId), Approved(GrainId));

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command2));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The description of work was already approved.", error.GetFormattedMessage());
        });
    }

    [Theory]
    [InlineData("a first version that is not version 1")]
    [InlineData("a second first version")]
    [InlineData("a revision of an outdated version")]
    [InlineData("a revision of another description of work")]
    public async Task Submit_RejectsAVersionThatDoesNotFollowTheCurrentOne(string problem)
    {
        (OrderEvent[] Given, SubmitDescriptionOfWorkCommand Command) setup = problem switch
        {
            "a first version that is not version 1" => ([], Command2),
            "a second first version" => ([Submitted1(GrainId)], Command1),
            "a revision of an outdated version" => ([Submitted1(GrainId), Submitted(Command2, GrainId)], Command2),
            "a revision of another description of work" => ([Submitted1(GrainId)],
                Command2 with { DescriptionOfWorkId = TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString()) }),
            _ => throw new ArgumentOutOfRangeException(nameof(problem))
        };
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(setup.Given);

        await When(() => Order.SubmitDescriptionOfWorkAsync(setup.Command));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.Aborted, error.Code));
    }

    [Theory]
    [InlineData(nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkId), "not a GUID")]
    [InlineData(nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkId), "another tenant")]
    [InlineData(nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkVersion), "zero")]
    [InlineData(nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkHtml), "missing")]
    [InlineData(nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkHtml), "an empty editor")]
    [InlineData(nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkHtml), "too long")]
    public async Task Submit_RejectsAnInvalidDetail(string property, string problem)
    {
        var command = (property, problem) switch
        {
            (nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkId), "not a GUID") => Command1 with { DescriptionOfWorkId = "summit|dow" },
            (nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkId), _) => Command1 with { DescriptionOfWorkId = $"other|{Guid.NewGuid()}" },
            (nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkVersion), _) => Command1 with { DescriptionOfWorkVersion = 0 },
            (nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkHtml), "missing") => Command1 with { DescriptionOfWorkHtml = " " },
            (nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkHtml), "an empty editor") => Command1 with { DescriptionOfWorkHtml = "<p><br></p>" },
            (nameof(SubmitDescriptionOfWorkCommand.DescriptionOfWorkHtml), _) => Command1 with
            {
                DescriptionOfWorkHtml = $"<p>{new string('a', DescriptionOfWorkHtml.MaxLength)}</p>"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Order.SubmitDescriptionOfWorkAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Submit_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given();

        await When(() => Order.SubmitDescriptionOfWorkAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Submit_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.SubmitDescriptionOfWorkAsync(Command1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.Coordinator1) => RequestedByUsers.Coordinator1,
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>Sets up a submitted order that Director1 was assigned and acknowledged, followed by <paramref name="more"/> events.</summary>
    private Task GivenAcknowledged(params OrderEvent[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        .. more
    ]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
