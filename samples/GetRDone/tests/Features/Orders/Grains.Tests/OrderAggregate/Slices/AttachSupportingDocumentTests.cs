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

using static GetRDone.Orders.Grains.Tests.Data.AttachSupportingDocumentTestData;

namespace GetRDone.Orders.Grains.Tests.OrderAggregate.Slices;

/// <summary>
///     Given/When/Then tests for attaching a supporting document to a draft. Each test uses a new order in the test tenant;
///     the documents come from <see cref="AttachSupportingDocumentTestData"/>.
/// </summary>
public sealed class AttachSupportingDocumentTests
    : EventDrivenStateMachineGrainTestHarness<IOrderGrain, OrderGrain, OrderState, OrderEvent, OrderStatus, OrderCommand>,
      IClassFixture<OrdersCluster>
{
    public AttachSupportingDocumentTests(OrdersCluster fixture)
        : base(fixture.Cluster, TenantKey.Create(TestData.TenantId, Guid.NewGuid().ToString())) { }

    private IOrderGrain Order => Cluster.GrainFactory.GetGrain<IOrderGrain>(GrainId);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Attach_RecordsTheDocument(int variation)
    {
        var (user, draftStarted, command, attached) = Variation(variation);
        var before = DateTimeOffset.UtcNow;

        GivenSignedIn(user);
        await Given(draftStarted(GrainId, null));

        await When(() => Order.AttachSupportingDocumentAsync(command));

        Then<AttachSupportingDocumentResponse>(response =>
        {
            Assert.Equal(2, response.Version);
            var recorded = Assert.IsType<SupportingDocumentAttached>(SingleEvent(response.Changes));
            Assert.InRange(recorded.OccurredAt, before, DateTimeOffset.UtcNow);
            Assert.Equivalent(attached(GrainId, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Attach_RecordsTheFilePathUnderTheTenantOrderAndDocument()
    {
        TenantKey.Split(GrainId, out _, out var orderGuid);
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command1));

        Then<AttachSupportingDocumentResponse>(response => Assert.Equal(
            $"summit/{orderGuid}/0f4b8d2e-6a1c-4e3f-9b7d-2c5e8a1f3d60",
            Assert.IsType<SupportingDocumentAttached>(SingleEvent(response.Changes)).BlobPath));
    }

    [Fact]
    public async Task Attach_AddsToTheDocumentsAlreadyAttached()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), Attached1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command3));

        Then<AttachSupportingDocumentResponse>(response =>
        {
            Assert.Equal(3, response.Version);
            var recorded = Assert.IsType<SupportingDocumentAttached>(SingleEvent(response.Changes));
            Assert.Equivalent(Attached(Command3, GrainId, RequestedByUsers.ClientUser1, recorded.OccurredAt), recorded, strict: true);
        });
    }

    [Fact]
    public async Task Attach_RejectsADocumentThatIsAlreadyAttached()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId), Attached1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command1 with { FileName = "Purchase_Contract_v2.pdf" }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.AlreadyExists, error.Code));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Attach_RecordsAnEmptyDescriptionAsNone(string? empty)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command1 with { Description = empty }));

        Then<AttachSupportingDocumentResponse>(response =>
            Assert.Null(Assert.IsType<SupportingDocumentAttached>(SingleEvent(response.Changes)).Description));
    }

    [Fact]
    public async Task Attach_AcceptsAFileOfTheLargestSize()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command1 with { FileSize = FileSize.MaxValue }));

        Then<AttachSupportingDocumentResponse>(response => Assert.Equal(
            100L * 1024 * 1024, Assert.IsType<SupportingDocumentAttached>(SingleEvent(response.Changes)).FileSize));
    }

    [Theory]
    [InlineData(1, "Purchase contract")]
    [InlineData(2, "Survey")]
    public async Task Attach_RejectsADocumentTypeThatIsNotCurrent(int version, string name)
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command1 with { DocumentTypeVersion = version, DocumentTypeName = name }));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.InvalidArgument, error.Code));
    }

    [Theory]
    [InlineData(nameof(AttachSupportingDocumentCommand.DocumentId), "not a GUID")]
    [InlineData(nameof(AttachSupportingDocumentCommand.DocumentId), "another tenant")]
    [InlineData(nameof(AttachSupportingDocumentCommand.DocumentTypeId), "missing")]
    [InlineData(nameof(AttachSupportingDocumentCommand.DocumentTypeVersion), "zero")]
    [InlineData(nameof(AttachSupportingDocumentCommand.DocumentTypeName), "missing")]
    [InlineData(nameof(AttachSupportingDocumentCommand.FileName), "missing")]
    [InlineData(nameof(AttachSupportingDocumentCommand.FileName), "too long")]
    [InlineData(nameof(AttachSupportingDocumentCommand.ContentType), "missing")]
    [InlineData(nameof(AttachSupportingDocumentCommand.FileSize), "empty file")]
    [InlineData(nameof(AttachSupportingDocumentCommand.FileSize), "over 100 MB")]
    [InlineData(nameof(AttachSupportingDocumentCommand.Description), "too long")]
    public async Task Attach_RejectsAnInvalidDetail(string property, string problem)
    {
        var command = (property, problem) switch
        {
            (nameof(AttachSupportingDocumentCommand.DocumentId), "not a GUID") => Command1 with { DocumentId = "summit|purchase-contract" },
            (nameof(AttachSupportingDocumentCommand.DocumentId), "another tenant") => Command1 with { DocumentId = $"other|{Guid.NewGuid()}" },
            (nameof(AttachSupportingDocumentCommand.DocumentTypeId), _) => Command1 with { DocumentTypeId = "" },
            (nameof(AttachSupportingDocumentCommand.DocumentTypeVersion), _) => Command1 with { DocumentTypeVersion = 0 },
            (nameof(AttachSupportingDocumentCommand.DocumentTypeName), _) => Command1 with { DocumentTypeName = " " },
            (nameof(AttachSupportingDocumentCommand.FileName), "missing") => Command1 with { FileName = "" },
            (nameof(AttachSupportingDocumentCommand.FileName), _) => Command1 with { FileName = new string('a', FileName.MaxLength + 1) },
            (nameof(AttachSupportingDocumentCommand.ContentType), _) => Command1 with { ContentType = "" },
            (nameof(AttachSupportingDocumentCommand.FileSize), "empty file") => Command1 with { FileSize = 0 },
            (nameof(AttachSupportingDocumentCommand.FileSize), _) => Command1 with { FileSize = FileSize.MaxValue + 1 },
            (nameof(AttachSupportingDocumentCommand.Description), _) => Command1 with { Description = new string('a', DocumentDescription.MaxLength + 1) },
            _ => throw new ArgumentOutOfRangeException(nameof(property))
        };
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(command));

        ThenError<ValidationError>(error => Assert.Equal(property, Assert.Single(error.Entries).Target));
    }

    [Fact]
    public async Task Attach_RejectsAnOrderThatWasNotStarted()
    {
        GivenSignedIn(RequestedByUsers.ClientUser1);
        await Given();

        await When(() => Order.AttachSupportingDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.NotFound, error.Code));
    }

    [Fact]
    public async Task Attach_RejectsAUserWhoDidNotStartTheOrder()
    {
        GivenSignedIn(RequestedByUsers.ClientUser2);
        await Given(StartOrderDraftTestData.DraftStarted1(GrainId));

        await When(() => Order.AttachSupportingDocumentAsync(Command1));

        ThenError<RequestError>(error => Assert.Equal(ErrorCodes.PermissionDenied, error.Code));
    }

    [Fact]
    public async Task Attach_RejectsAnUnauthenticatedCaller()
    {
        // Orleans Identity rejects the call before it reaches the grain, so this calls the grain directly instead of using When.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Order.AttachSupportingDocumentAsync(Command1));
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);

    private static (
        ClaimsPrincipal User,
        Func<string, DateTimeOffset?, OrderDraftStarted> DraftStarted,
        AttachSupportingDocumentCommand Command,
        Func<string, DateTimeOffset?, SupportingDocumentAttached> Attached)
        Variation(int variation) => variation switch
        {
            1 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted1, Command1, Attached1),
            2 => (RequestedByUsers.ClientUser2, StartOrderDraftTestData.DraftStarted2, Command2, Attached2),
            3 => (RequestedByUsers.ClientUser1, StartOrderDraftTestData.DraftStarted3, Command3, Attached3),
            _ => throw new ArgumentOutOfRangeException(nameof(variation))
        };

    private static object SingleEvent(IReadOnlyList<IChange> changes) => Assert.IsType<Change>(Assert.Single(changes)).Event;
}
