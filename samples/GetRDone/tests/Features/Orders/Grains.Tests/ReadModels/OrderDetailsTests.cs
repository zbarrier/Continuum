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

using static GetRDone.Orders.Grains.Tests.Data.StartOrderDraftTestData;

namespace GetRDone.Orders.Grains.Tests.ReadModels;

/// <summary>
///     Tests for the staff order page's OrderDetails projection. Each test projects a new order, so it starts with nothing.
///     The grain is <c>[Authorize]</c>d and <c>Given</c> calls <c>OnNextBatchAsync</c> directly, so tests sign in first.
/// </summary>
public sealed class OrderDetailsTests
    : ProjectionGrainTestHarness<IOrderDetailsGrain, OrderDetailsGrain, OrderDetailsState>,
      IClassFixture<OrdersCluster>
{
    private readonly string _orderId;

    public OrderDetailsTests(OrdersCluster fixture) : this(fixture, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private OrderDetailsTests(OrdersCluster fixture, string orderId) : base(fixture.Cluster, orderId) => _orderId = orderId;

    [Fact]
    public async Task OrderDetails_ShowTheOrderWithItsTimelineMostRecentFirst()
    {
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await Given(
            DraftStarted1(_orderId, TestData.At),
            SaveBorrowerAndAccessContactTestData.Saved1(_orderId, TestData.At),
            SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)),
            AssignOrderNumberTestData.Assigned1(_orderId, 610_900, TestData.At.AddMinutes(2)),
            AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(3)),
            AcknowledgeOrderTestData.Acknowledged1(_orderId, TestData.At.AddMinutes(4)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equivalent(new OrderDetails(
            _orderId,
            610_900,
            StaffOrderStatus.Acknowledged,
            "Lakeview Credit Union",
            RequestedByUsers.ClientUser1.GetTenantUserId(),
            "John Doe",
            TestData.UniformResidentialAppraisal.FormTypeName,
            "48 Birch Hollow Rd, Asheville, NC 28803",
            DueDate: null,
            ServiceFee: null,
            "Marcus Doyle",
            "8285550142",
            RequestedByUsers.Director1.GetTenantUserId(),
            "Tommy Jones",
            [],
            [],
            null,
            null,
            [
                new OrderTimelineEntry(TestData.At.AddMinutes(4), "Acknowledged by Tommy Jones"),
                new OrderTimelineEntry(TestData.At.AddMinutes(3), "Assigned to Tommy Jones"),
                new OrderTimelineEntry(TestData.At.AddMinutes(2), "Order number 610900"),
                new OrderTimelineEntry(TestData.At.AddMinutes(1), "Submitted by John Doe")
            ],
            OrderActions.None), details, strict: true));
    }

    [Fact]
    public async Task RequestAndPropertySaved_ChangesTheOrdersAddressAndFormType()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
            DraftStarted3(_orderId, TestData.At),
            SaveRequestAndPropertyTestData.Saved(
                SaveRequestAndPropertyTestData.Command3 with { PropertyStreet = "11 Laurel Ridge Ct" },
                _orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(1)),
            SubmitOrderTestData.Submitted(Command3, _orderId, RequestedByUsers.ClientUser1, TestData.At.AddMinutes(2)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal("11 Laurel Ridge Ct, Black Mountain, NC 28711", details.PropertyAddress);
            Assert.Equal(SaveRequestAndPropertyTestData.Command3.FormTypeName, details.FormTypeName);
        });
    }

    [Fact]
    public async Task OrderDetails_NameAUserTheDirectoryDoesNotKnowAsUnknown()
    {
        var newUser = RequestedByUsers.NewClientUser();
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(
            DraftStarted(Command1, _orderId, newUser, TestData.At),
            SubmitOrderTestData.Submitted(Command1, _orderId, newUser, TestData.At.AddMinutes(1)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Null(details.EnteredByName);
            Assert.Equal("Submitted by an unknown user", Assert.Single(details.Timeline).Entry);
        });
    }

    [Fact]
    public async Task OrderDetails_LetTheDirectorAssignASubmittedOrderToThemselves()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted();

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(OrderActions.AssignToMe, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_LetTheTenantAdminAssignASubmittedOrderToThemselves()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenSubmitted();

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(OrderActions.AssignToMe, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_DoNotLetACoordinatorAssignAnOrder()
    {
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await GivenSubmitted();

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal(StaffOrderStatus.Submitted, details.Status);
            Assert.Equal(OrderActions.None, details.AllowedActions);
        });
    }

    [Fact]
    public async Task OrderDetails_LetTheAssigneeAcknowledgeTheOrder()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenSubmitted(AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(2)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal(StaffOrderStatus.Assigned, details.Status);
            Assert.Equal(OrderActions.Acknowledge, details.AllowedActions);
        });
    }

    [Fact]
    public async Task OrderDetails_DoNotLetAnyoneElseAcknowledgeTheOrder()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenSubmitted(AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(2)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(OrderActions.None, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_ShowTheRequestsTheClientHasNotProvided()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(
            DocumentRequestTestData.Requested1(_orderId, occurredAt: TestData.At.AddMinutes(5)),
            DocumentRequestTestData.Requested(DocumentRequestTestData.Command2, _orderId, occurredAt: TestData.At.AddMinutes(6)),
            DocumentRequestTestData.Provided1(_orderId, TestData.At.AddMinutes(7)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal(
                [
                    new OrderDocumentRequestSummary(
                        DocumentRequestTestData.Command2.RequestId, "Purchase contract", DocumentRequestTestData.Command2.Message,
                        TestData.At.AddMinutes(6))
                ],
                details.OpenRequests);
            Assert.Equal(
                ["Survey provided by John Doe", "Document requested: Purchase contract", "Document requested: Survey"],
                details.Timeline.Take(3).Select(entry => entry.Entry));
        });
    }

    [Fact]
    public async Task OrderDetails_ShowTheDocumentsAttachedAndProvidedWithWhoAddedThem()
    {
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await Given(
            DraftStarted1(_orderId, TestData.At),
            AttachSupportingDocumentTestData.Attached1(_orderId, TestData.At),
            AttachSupportingDocumentTestData.Attached3(_orderId, TestData.At),
            RemoveSupportingDocumentTestData.Removed3(_orderId, TestData.At),
            SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)),
            AssignOrderTestData.OrderAssigned1(_orderId, TestData.At.AddMinutes(2)),
            AcknowledgeOrderTestData.Acknowledged1(_orderId, TestData.At.AddMinutes(3)),
            DocumentRequestTestData.Requested1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DocumentRequestTestData.Provided1(_orderId, TestData.At.AddMinutes(5)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(
            [
                new OrderDocumentSummary(
                    AttachSupportingDocumentTestData.Command1.DocumentId, "Purchase_Contract_48BirchHollow.pdf", "Purchase contract",
                    "John Doe", TestData.At),
                new OrderDocumentSummary(
                    DocumentRequestTestData.ProvideCommand1.DocumentId, "Survey_48BirchHollow.pdf", "Survey",
                    "John Doe", TestData.At.AddMinutes(5))
            ],
            details.Documents));
    }

    [Fact]
    public async Task OrderDetails_LetTheAssigneeRequestDocumentsAndSubmitTheDescriptionOfWorkOnceTheyAcknowledgeTheOrder()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged();

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(
            OrderActions.RequestDocument | OrderActions.SubmitDescriptionOfWork, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_LetTheTenantAdminRequestDocumentsAndSubmitTheDescriptionOfWorkForAnOrderAssignedToSomeoneElse()
    {
        GivenSignedIn(RequestedByUsers.TenantAdmin1);
        await GivenAcknowledged();

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(
            OrderActions.RequestDocument | OrderActions.SubmitDescriptionOfWork, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_DoNotLetACoordinatorWorkOnAnOrderAssignedToSomeoneElse()
    {
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await GivenAcknowledged(DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(OrderActions.None, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_ShowTheLatestDescriptionOfWorkWaitingForApproval()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Submitted(DescriptionOfWorkTestData.Command2, _orderId, occurredAt: TestData.At.AddMinutes(5)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal(StaffOrderStatus.DescriptionOfWorkAwaitingApproval, details.Status);
            Assert.Equal(
                new DescriptionOfWorkSummary(
                    DescriptionOfWorkTestData.DescriptionOfWorkId, 2, DescriptionOfWorkTestData.Command2.DescriptionOfWorkHtml,
                    Approved: false, TestData.At.AddMinutes(5)),
                details.DescriptionOfWork);
            Assert.Equal(
                ["Description of work revised (version 2) by Tommy Jones", "Description of work submitted by Tommy Jones"],
                details.Timeline.Take(2).Select(entry => entry.Entry));
            Assert.Equal(OrderActions.RequestDocument | OrderActions.SubmitDescriptionOfWork, details.AllowedActions);
        });
    }

    [Fact]
    public async Task OrderDetails_ShowTheApprovedDescriptionOfWorkAndLetTheAssigneeRequestDocumentsAndAssignAVendor()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal(StaffOrderStatus.DescriptionOfWorkApproved, details.Status);
            Assert.True(details.DescriptionOfWork!.Approved);
            Assert.Equal("Description of work version 1 approved by John Doe", details.Timeline[0].Entry);
            Assert.Equal(OrderActions.RequestDocument | OrderActions.AssignVendor, details.AllowedActions);
        });
    }

    [Fact]
    public async Task OrderDetails_DoNotLetACoordinatorAssignAVendorToAnOrderAssignedToSomeoneElse()
    {
        GivenSignedIn(RequestedByUsers.Coordinator1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details => Assert.Equal(OrderActions.None, details.AllowedActions));
    }

    [Fact]
    public async Task OrderDetails_ShowTheVendorTheOrderIsAssignedToWithTheServiceFeeAndDueDate()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await GivenAcknowledged(
            DescriptionOfWorkTestData.Submitted1(_orderId, occurredAt: TestData.At.AddMinutes(4)),
            DescriptionOfWorkTestData.Approved(_orderId, occurredAt: TestData.At.AddMinutes(5)),
            VendorAssignmentTestData.Assigned1(_orderId, occurredAt: TestData.At.AddMinutes(6)));

        await When(() => Grain.GetOrderDetailsAsync());

        Then<OrderDetails>(details =>
        {
            Assert.Equal(StaffOrderStatus.VendorAssigned, details.Status);
            Assert.Equal(
                new OrderVendorSummary(
                    TestData.RidgeAppraisalGroupVendorId, "Ridge Appraisal Group", 425.00m, VendorAssignmentTestData.VendorDueDate),
                details.Vendor);
            Assert.Equal(650.00m, details.ServiceFee);
            Assert.Equal(VendorAssignmentTestData.DueDate, details.DueDate);
            Assert.Equal("Assigned to vendor Ridge Appraisal Group by Tommy Jones", details.Timeline[0].Entry);
            Assert.Equal(OrderActions.RequestDocument, details.AllowedActions);
        });
    }

    [Fact]
    public async Task OrderDetails_AreNotFoundForADraft()
    {
        GivenSignedIn(RequestedByUsers.Director1);
        await Given(DraftStarted1(_orderId, TestData.At));

        await When(() => Grain.GetOrderDetailsAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task OrderDetails_CanOnlyBeReadByTheStaff()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await GivenSubmitted();

        await When(() => Grain.GetOrderDetailsAsync());

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    /// <summary>
    ///     Checks the stream wiring the harness bypasses: events produced on the order's own stream reach the order's
    ///     projection through its implicit subscription. Events are published signed out, as the stream provider delivers
    ///     them with no signed-in user, to check that delivery isn't blocked by the grain's <c>[Authorize]</c>.
    /// </summary>
    [Fact]
    public async Task OrderDetails_AreProjectedFromTheOrdersStream()
    {
        RequestContext.Clear();
        var stream = Cluster.Client.GetStreamProvider(OrderProjections.ProviderName)
            .GetStream<IStreamedEvent<object>>(StreamId.Create(OrderProjections.OrderEventsNamespace, _orderId));

        var started = DraftStarted1(_orderId, TestData.At);
        var submitted = SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1));
        await stream.OnNextAsync(OnOrderStream("Orders.OrderDraftStarted.V1", 0, started, started.OccurredAt));
        await stream.OnNextAsync(OnOrderStream("Orders.OrderSubmitted.V1", 1, submitted, submitted.OccurredAt));

        GivenSignedIn(RequestedByUsers.Director1);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await Grain.GetOrderDetailsAsync()).IsSuccess)
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    /// <summary>Sets up the order <c>Submitted1</c> submits, followed by <paramref name="more"/> events.</summary>
    private Task GivenSubmitted(params object[] more) => Given(
    [
        DraftStarted1(_orderId, TestData.At),
        SubmitOrderTestData.Submitted1(_orderId, TestData.At.AddMinutes(1)),
        .. more
    ]);

    /// <summary>Sets up the order <c>Submitted1</c> submits, assigned to Director1 who acknowledged it, followed by <paramref name="more"/> events.</summary>
    private Task GivenAcknowledged(params object[] more) => GivenSubmitted(
    [
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
