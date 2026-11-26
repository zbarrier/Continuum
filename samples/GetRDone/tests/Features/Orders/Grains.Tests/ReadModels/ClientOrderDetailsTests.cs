using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.TestHarness;

using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Core.Constants;

using Orleans.Runtime;

using static GetRDone.Orders.Grains.Tests.Data.DocumentRequestTestData;

namespace GetRDone.Orders.Grains.Tests.ReadModels;

/// <summary>
///     Tests for the client order page's ClientOrderDetails projection. Each test projects a new order, so it starts with
///     nothing. The grain is <c>[Authorize]</c>d and <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in
///     first.
/// </summary>
public sealed class ClientOrderDetailsTests
    : ProjectionGrainTestHarness<IClientOrderDetailsGrain, ClientOrderDetailsGrain, ClientOrderDetailsState>,
      IClassFixture<OrdersCluster>
{
    private readonly string _orderId;

    public ClientOrderDetailsTests(OrdersCluster fixture) : this(fixture, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private ClientOrderDetailsTests(OrdersCluster fixture, string orderId) : base(fixture.Cluster, orderId) => _orderId = orderId;

    [Fact]
    public async Task ClientOrderDetails_ShowTheOrderItsRequestsDocumentsAndTimelineMostRecentFirst()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At),
            AttachSupportingDocumentTestData.Attached1(_orderId, TestData.At),
            SaveBorrowerAndAccessContactTestData.Saved1(_orderId, TestData.At),
            SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)),
            AssignOrderNumberTestData.Assigned1(_orderId, 610_900, TestData.At.AddMinutes(2)),
            AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(3)),
            AcknowledgeOrderTestData.Acknowledged1(_orderId, TestData.At.AddMinutes(4)),
            Requested1(_orderId, 610_900, TestData.At.AddMinutes(5)),
            Provided1(_orderId, TestData.At.AddMinutes(6)),
            Requested(Command2, _orderId, 610_900, TestData.At.AddMinutes(7)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details => Assert.Equivalent(new ClientOrderDetails(
            _orderId,
            610_900,
            ClientOrderStatus.Acknowledged,
            TestData.UniformResidentialAppraisal.FormTypeName,
            "48 Birch Hollow Rd, Asheville, NC 28803",
            DueDate: null,
            ServiceFee: null,
            "Tommy Jones",
            [new ClientDocumentRequest(Command2.RequestId, "Purchase contract", Command2.Message, TestData.At.AddMinutes(7))],
            [
                new ClientOrderDocument(AttachSupportingDocumentTestData.Command1.DocumentId, "Purchase_Contract_48BirchHollow.pdf", "Purchase contract"),
                new ClientOrderDocument(ProvideCommand1.DocumentId, "Survey_48BirchHollow.pdf", "Survey")
            ],
            null,
            Report: null,
            [
                new OrderTimelineEntry(TestData.At.AddMinutes(7), "Document requested: Purchase contract"),
                new OrderTimelineEntry(TestData.At.AddMinutes(6), "Survey provided by John Doe"),
                new OrderTimelineEntry(TestData.At.AddMinutes(5), "Document requested: Survey"),
                new OrderTimelineEntry(TestData.At.AddMinutes(4), "Acknowledged by Tommy Jones"),
                new OrderTimelineEntry(TestData.At.AddMinutes(3), "Assigned to Tommy Jones"),
                new OrderTimelineEntry(TestData.At.AddMinutes(2), "Order number 610900"),
                new OrderTimelineEntry(TestData.At.AddMinutes(1), "Submitted by John Doe")
            ],
            ClientOrderActions.UploadRequestedDocument), details, strict: true));
    }

    [Fact]
    public async Task ClientOrderDetails_HaveNothingToUploadWithoutAnOpenRequest()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted();

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details =>
        {
            Assert.Equal(ClientOrderStatus.Submitted, details.Status);
            Assert.Null(details.AssignedToName);
            Assert.Empty(details.OpenRequests);
            Assert.Equal(ClientOrderActions.None, details.AllowedActions);
        });
    }

    [Fact]
    public async Task ClientOrderDetails_LetTheClientApproveTheDescriptionOfWorkWaitingForApproval()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged(DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details =>
        {
            Assert.Equal(ClientOrderStatus.DescriptionOfWorkAwaitingApproval, details.Status);
            Assert.Equal(
                new DescriptionOfWorkSummary(
                    DescriptionOfWorkTestData.DescriptionOfWorkId, 1, DescriptionOfWorkTestData.Command1.DescriptionOfWorkHtml,
                    Approved: false, TestData.At.AddMinutes(4)),
                details.DescriptionOfWork);
            Assert.Equal("Description of work submitted by Tommy Jones", details.Timeline[0].Entry);
            Assert.Equal(ClientOrderActions.ApproveDescriptionOfWork, details.AllowedActions);
        });
    }

    [Fact]
    public async Task ClientOrderDetails_HaveNothingToApproveOnceTheDescriptionOfWorkIsApproved()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details =>
        {
            Assert.Equal(ClientOrderStatus.DescriptionOfWorkApproved, details.Status);
            Assert.True(details.DescriptionOfWork!.Approved);
            Assert.Equal(ClientOrderActions.None, details.AllowedActions);
        });
    }

    [Fact]
    public async Task ClientOrderDetails_ShowThatAnAppraiserIsAssignedWithTheServiceFeeAndDueDateWithoutNamingTheVendor()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)),
            VendorAssignmentTestData.Assigned1(_orderId, occurredAt: TestData.At.AddMinutes(6)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details =>
        {
            Assert.Equal(ClientOrderStatus.VendorAssigned, details.Status);
            Assert.Equal(650.00m, details.ServiceFee);
            Assert.Equal(VendorAssignmentTestData.DueDate, details.DueDate);
            Assert.Equal(new OrderTimelineEntry(TestData.At.AddMinutes(6), "Appraiser assigned"), details.Timeline[0]);
            Assert.Equal(ClientOrderActions.None, details.AllowedActions);
        });
    }

    [Fact]
    public async Task ClientOrderDetails_ShowTheInspectionsProgressWithoutTheVendorsNameOrTheReport()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)),
            VendorAssignmentTestData.Assigned1(_orderId, occurredAt: TestData.At.AddMinutes(6)),
            InspectionTestData.Scheduled1(_orderId, TestData.At.AddMinutes(7)),
            InspectionTestData.Completed1(_orderId, TestData.At.AddMinutes(8)),
            InspectionTestData.Delivered1(_orderId, occurredAt: TestData.At.AddMinutes(9)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details =>
        {
            Assert.Equal(ClientOrderStatus.InspectionCompleted, details.Status);
            Assert.Equal(
                [
                    new OrderTimelineEntry(TestData.At.AddMinutes(8), "Inspection completed"),
                    new OrderTimelineEntry(TestData.At.AddMinutes(7), "Inspection scheduled"),
                    new OrderTimelineEntry(TestData.At.AddMinutes(6), "Appraiser assigned")
                ],
                details.Timeline.Take(3));
            Assert.DoesNotContain(details.Documents, document => document.DocumentId == InspectionTestData.DeliverCommand1.DocumentId);
            Assert.Null(details.Report);
        });
    }

    [Fact]
    public async Task ClientOrderDetails_LetTheClientDownloadTheReportOnceTheOrderShips()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)),
            VendorAssignmentTestData.Assigned1(_orderId, occurredAt: TestData.At.AddMinutes(6)),
            InspectionTestData.Scheduled1(_orderId, TestData.At.AddMinutes(7)),
            InspectionTestData.Completed1(_orderId, TestData.At.AddMinutes(8)),
            InspectionTestData.Delivered1(_orderId, occurredAt: TestData.At.AddMinutes(9)),
            ShippingTestData.Shipped1(_orderId, occurredAt: TestData.At.AddMinutes(10)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details =>
        {
            Assert.Equal(ClientOrderStatus.Shipped, details.Status);
            Assert.Equal(
                new ClientOrderReport(InspectionTestData.DeliverCommand1.DocumentId, InspectionTestData.DeliverCommand1.FileName,
                    TestData.At.AddMinutes(10)),
                details.Report);
            Assert.Equal(new OrderTimelineEntry(TestData.At.AddMinutes(10), "Report shipped"), details.Timeline[0]);
            Assert.DoesNotContain(details.Documents, document => document.DocumentId == InspectionTestData.DeliverCommand1.DocumentId);
            Assert.Equal(ClientOrderActions.None, details.AllowedActions);
        });
    }

    [Fact]
    public async Task SupportingDocumentRemoved_RemovesTheDocument()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(
            StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At),
            AttachSupportingDocumentTestData.Attached1(_orderId, TestData.At),
            AttachSupportingDocumentTestData.Attached3(_orderId, TestData.At),
            RemoveSupportingDocumentTestData.Removed1(_orderId, TestData.At),
            SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)));

        await When(() => Grain.GetClientOrderDetailsAsync());

        Then<ClientOrderDetails>(details => Assert.Equal(
            AttachSupportingDocumentTestData.Command3.DocumentId, Assert.Single(details.Documents).DocumentId));
    }

    [Fact]
    public async Task ClientOrderDetails_AreNotFoundForADraft()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At));

        await When(() => Grain.GetClientOrderDetailsAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Theory]
    [InlineData(nameof(RequestedByUsers.ClientUser2))]
    [InlineData(nameof(RequestedByUsers.Director1))]
    public async Task ClientOrderDetails_CanOnlyBeReadByTheClientWhoStartedTheOrder(string userName)
    {
        GivenSignedIn(User(userName));
        await GivenSubmitted();

        await When(() => Grain.GetClientOrderDetailsAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    /// <summary>
    ///     Checks the stream wiring the harness bypasses: events produced on the order's own stream reach the order's
    ///     client projection through its implicit subscription. Events are published signed out, as the stream provider
    ///     delivers them with no signed-in user, to check that delivery isn't blocked by the grain's <c>[Authorize]</c>.
    /// </summary>
    [Fact]
    public async Task ClientOrderDetails_AreProjectedFromTheOrdersStream()
    {
        RequestContext.Clear();
        var stream = Cluster.Client.GetStreamProvider(OrderProjections.ProviderName)
            .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.OrderEventsNamespace, _orderId));

        var started = StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At);
        var submitted = SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream("Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream("Orders.OrderSubmitted.V1", 1, submitted, submitted.OccurredAt));

        GivenSignedIn(RequestedByUsers.ClientUser1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetClientOrderDetailsAsync()).IsSuccess)
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private static ClaimsPrincipal User(string name) => name switch
    {
        nameof(RequestedByUsers.ClientUser2) => RequestedByUsers.ClientUser2,
        nameof(RequestedByUsers.Director1) => RequestedByUsers.Director1,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
    };

    /// <summary>Sets up the order <c>Submitted1</c> submits.</summary>
    private Task GivenSubmitted() => Given(
        StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At),
        SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)));

    /// <summary>Sets up the order <c>Submitted1</c> submits, assigned to Director1 who acknowledged it, followed by <paramref name="more"/> events.</summary>
    private Task GivenAcknowledged(params object[] more) => Given(
    [
        StartOrderDraftTestData.DraftStarted1(_orderId, TestData.At),
        SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)),
        AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(2)),
        AcknowledgeOrderTestData.Acknowledged1(_orderId, TestData.At.AddMinutes(3)),
        .. more
    ]);

    /// <summary>An event of the order as the stream provider delivers it, read from the order's stream at <paramref name="revision"/>.</summary>
    private StreamedEvent<object> OnOrderStream(string eventType, ulong revision, object evt, DateTimeOffset occurredAt)
    {
        TenantKey.Split(_orderId, out var tenantId, out var key);
        return new StreamedEvent<object>(
            Guid.NewGuid(),
            eventType,
            new StreamName($"order-{_orderId}", "order", tenantId, key),
            revision,
            new StreamedEventCursor("kurrentdb", "order", _orderId, NextPosition(), 0),
            occurredAt.UtcDateTime,
            evt);
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
