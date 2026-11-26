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

using static GetRDone.Orders.Grains.Tests.Data.InspectionTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for the vendor delivering the report of an inspected order. Each test uses a new order in the
///     test tenant. The first test checks what applying the slice's event does to the order state and the second checks
///     every value of the event the slice records; the behavior tests follow.
/// </summary>
public sealed class DeliverReportTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public DeliverReportTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Fact]
    public void ApplyingReportDelivered_KeepsTheReportWithTheOrdersDocuments()
    {
        var state = new OrderState();
        foreach (var evt in CompletedEvents())
        {
            state.When(evt);
        }

        state.When(Delivered1(GrainId));

        Assert.Equal(OrderStatus.ReportDelivered, state.Status);
        Assert.Equal(new OrderReport(DeliverCommand1.DocumentId, NoticeSent: false), state.Report);
        Assert.Equal(
            SupportingDocumentStorage.BlobPath(GrainId, DeliverCommand1.DocumentId),
            Assert.Single(state.SupportingDocumentBlobPaths, entry => entry.Key == DeliverCommand1.DocumentId).Value);
    }

    [Fact]
    public async Task Deliver_RecordsTheReportForTheAssignee()
    {
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenCompleted();

        await When(() => Order.DeliverReportAsync(DeliverCommand1));

        Then<DeliverReportResponse>(response =>
        {
            Assert.Equal(11, response.Version);
            var recorded = Assert.IsType<ReportDelivered>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(Delivered1(GrainId, occurredAt: recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Deliver_CarriesTheOrdersNumber()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenCompleted(AssignOrderNumberTestData.Assigned1(GrainId));

        await When(() => Order.DeliverReportAsync(DeliverCommand1));

        Then<DeliverReportResponse>(response => Assert.Equal(
            AssignOrderNumberTestData.FirstOrderNumber,
            Assert.IsType<ReportDelivered>(SingleEvent(response.Changes)).OrderNumber));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.OtherVendorUser1))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    [InlineData(nameof(RequestedByUsers.ClientUser1))]
    public async Task Deliver_RejectsAUserWhoIsNotOfTheAssignedVendor(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenCompleted();

        await When(() => Order.DeliverReportAsync(DeliverCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Deliver_RejectsAnInspectionThatIsNotCompleted()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenAssigned(Scheduled1(GrainId));

        await When(() => Order.DeliverReportAsync(DeliverCommand1));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The inspection must be completed before the report is delivered.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Deliver_RejectsAReportThatWasAlreadyDelivered()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenCompleted(Delivered1(GrainId));

        await When(() => Order.DeliverReportAsync(DeliverCommand1 with { DocumentId = "summit|1d3f5b7a-9c2e-4a6b-8d0f-2e4a6c8b0d35" }));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.FailedPrecondition, error.Code);
            Assert.Equal("The report was already delivered.", error.GetFormattedMessage());
        });
    }

    [Fact]
    public async Task Deliver_RejectsADocumentThatIsAlreadyAttachedToTheOrder()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given(
        [
            StartOrderDraftTestData.DraftStarted1(GrainId),
            AttachSupportingDocumentTestData.Attached1(GrainId),
            .. AssignedEvents().Skip(1),
            Scheduled1(GrainId),
            Completed1(GrainId)
        ]);

        await When(() => Order.DeliverReportAsync(DeliverCommand1 with { DocumentId = AttachSupportingDocumentTestData.Command1.DocumentId }));

        ThenError<RequestError>(error =>
        {
            Assert.Equal(ErrorCodes.AlreadyExists, error.Code);
            Assert.Equal("The document is already attached to the order.", error.GetFormattedMessage());
        });
    }

    [Theory]
    [InlineData(nameof(DeliverReportCommand.DocumentId))]
    [InlineData(nameof(DeliverReportCommand.FileName))]
    [InlineData(nameof(DeliverReportCommand.ContentType))]
    [InlineData(nameof(DeliverReportCommand.FileSize))]
    public async Task Deliver_RejectsAnInvalidDetail(string property)
    {
        var command = property switch
        {
            nameof(DeliverReportCommand.DocumentId) => DeliverCommand1 with { DocumentId = "other|8f2c4e6a-1b3d-4f5a-9c7e-0d2b4a6c8e13" },
            nameof(DeliverReportCommand.FileName) => DeliverCommand1 with { FileName = "" },
            nameof(DeliverReportCommand.ContentType) => DeliverCommand1 with { ContentType = "" },
            nameof(DeliverReportCommand.FileSize) => DeliverCommand1 with { FileSize = 0 },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await GivenCompleted();

        await When(() => Order.DeliverReportAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Deliver_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.VendorUser1);
        await Given();

        await When(() => Order.DeliverReportAsync(DeliverCommand1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Deliver_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.DeliverReportAsync(DeliverCommand1));
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser1) => RequestedByUsers.ClientUser1,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        nameof(RequestedByUsers.OtherVendorUser1) => RequestedByUsers.OtherVendorUser1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    private OrderEvent[] AssignedEvents() =>
    [
        StartOrderDraftTestData.DraftStarted1(GrainId),
        SaveBorrowerAndAccessContactTestData.Saved1(GrainId),
        SubmitOrderTestData.Submitted1(GrainId),
        AssignOrderTestData.OrderAssigned1(GrainId),
        AcknowledgeOrderTestData.Acknowledged1(GrainId),
        DescriptionOfWorkTestData.Submitted1(GrainId),
        DescriptionOfWorkTestData.Approved(GrainId),
        VendorAssignmentTestData.Assigned1(GrainId)
    ];

    private OrderEvent[] CompletedEvents() => [.. AssignedEvents(), Scheduled1(GrainId), Completed1(GrainId)];

    /// <summary>Sets up an order Director1 assigned to Ridge Appraisal Group, followed by <paramref name="more"/> events.</summary>
    private Task GivenAssigned(params OrderEvent[] more) => Given([.. AssignedEvents(), .. more]);

    /// <summary>Sets up an order whose inspection VendorUser1 completed, followed by <paramref name="more"/> events.</summary>
    private Task GivenCompleted(params OrderEvent[] more) => Given([.. CompletedEvents(), .. more]);

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
