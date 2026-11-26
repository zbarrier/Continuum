using System.Net;
using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.OrderAggregate;

/// <summary>Decides who may start and change orders.</summary>
/// <remarks>
///     The sample keeps the rules simple: a client user may start an order for their own client, and only the user who
///     started an order may change it. A full application would check the user's permissions within the tenant.
/// </remarks>
public interface IOrderPermissions
{
    Task<Result> CanStartOrderAsync(UserId userId, ClientId clientId);

    Result CanChangeOrder(UserId userId, OrderState order);

    Result CanViewOrder(UserId userId, OrderState order);

    /// <summary>Only the Order Number Assigner automation issues order numbers.</summary>
    Result CanIssueOrderNumbers(UserId userId);

    /// <summary>Only the Order Number Assigner automation gives orders their numbers.</summary>
    Result CanAssignOrderNumber(UserId userId);

    /// <summary>Only the New Order Notifier automation sends the new order notice and asks whether it was sent.</summary>
    Result CanSendNewOrderNotice(UserId userId);

    /// <summary>Only the Client Services Director or the tenant admin assigns orders.</summary>
    Result CanAssignOrder(ClaimsPrincipal user);

    /// <summary>Orders are assigned only to the AMC's staff in the tenant's user directory.</summary>
    Task<Result> CanBeAssignedOrderAsync(UserId assigneeId);

    /// <summary>Only the staff member the order is assigned to acknowledges it.</summary>
    Result CanAcknowledgeOrder(UserId userId, OrderState order);

    /// <summary>
    ///     The staff member the order is assigned to, the Client Services Director or the tenant admin requests documents
    ///     from the client.
    /// </summary>
    Result CanRequestClientDocument(ClaimsPrincipal user, UserId userId, OrderState order);

    /// <summary>Only the Document Request Notifier automation sends document request notices and asks whether they were sent.</summary>
    Result CanSendDocumentRequestNotice(UserId userId);

    /// <summary>Only the client user the document was requested from provides it.</summary>
    Result CanProvideRequestedDocument(UserId userId, OrderDocumentRequest request);

    /// <summary>
    ///     The staff member the order is assigned to, the Client Services Director or the tenant admin submits the description
    ///     of work.
    /// </summary>
    Result CanSubmitDescriptionOfWork(ClaimsPrincipal user, UserId userId, OrderState order);

    /// <summary>Only the client user who approves the description of work, who started the order, approves it.</summary>
    Result CanApproveDescriptionOfWork(UserId userId, OrderDescriptionOfWork descriptionOfWork);

    /// <summary>
    ///     Only the Description of Work Approval Notifier automation sends the notices of descriptions of work to approve and
    ///     asks whether one is due.
    /// </summary>
    Result CanSendDescriptionOfWorkSubmittedNotice(UserId userId);

    /// <summary>
    ///     Only the Description of Work Approved Notifier automation sends the notice of an approved description of work and
    ///     asks whether it was sent.
    /// </summary>
    Result CanSendDescriptionOfWorkApprovedNotice(UserId userId);

    /// <summary>
    ///     The staff member the order is assigned to, the Client Services Director or the tenant admin assigns the order to a
    ///     vendor.
    /// </summary>
    Result CanAssignVendor(ClaimsPrincipal user, UserId userId, OrderState order);

    /// <summary>Orders are assigned only to the vendors on the tenant's list of assignable vendors.</summary>
    Task<Result> CanBeAssignedVendorAsync(VendorId vendorId);

    /// <summary>The notice of an assigned order goes only to users of the vendor the order is assigned to.</summary>
    Task<Result> AreVendorUsersAsync(VendorId vendorId, IReadOnlyCollection<string> userIds);

    /// <summary>Only the Vendor Assignment Notifier automation sends the notice of an assigned order and asks whether it was sent.</summary>
    Result CanSendVendorAssignedNotice(UserId userId);

    /// <summary>
    ///     Only users of the vendor the order is assigned to schedule and complete its inspection and deliver its report.
    /// </summary>
    Task<Result> CanDoVendorWorkAsync(UserId userId, OrderState order);

    /// <summary>Only the Report Delivered Notifier automation sends the notice of a delivered report and asks whether it was sent.</summary>
    Result CanSendReportDeliveredNotice(UserId userId);

    /// <summary>
    ///     The staff member the order is assigned to, the Client Services Director or the tenant admin ships the order.
    /// </summary>
    Result CanShipOrder(ClaimsPrincipal user, UserId userId, OrderState order);

    /// <summary>Only the Order Shipped Notifier automation sends the notice of a shipped order and asks whether it was sent.</summary>
    Result CanSendOrderShippedNotice(UserId userId);
}

public sealed class OrderPermissions(IGrainFactory grainFactory) : IOrderPermissions
{
    public Task<Result> CanStartOrderAsync(UserId userId, ClientId clientId)
        => grainFactory.GetUserDirectoryGrain(userId.TenantId)
            .IsClientUserAsync(userId, clientId)
            .MapError(_ => (Error)RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "You can only start orders for your own client."));

    public Result CanChangeOrder(UserId userId, OrderState order)
        => order.StartedBy == (string)userId
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "You can only change orders you started."));

    public Result CanViewOrder(UserId userId, OrderState order)
        => order.StartedBy == (string)userId
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "You can only view orders you started."));

    public Result CanIssueOrderNumbers(UserId userId)
        => IsOrderNumberAssigner(userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Order Number Assigner can issue order numbers."));

    public Result CanAssignOrderNumber(UserId userId)
        => IsOrderNumberAssigner(userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Order Number Assigner can assign order numbers."));

    public Result CanSendNewOrderNotice(UserId userId)
        => userId.TenantId is { } tenantId && (string)userId == AutomationUser.NewOrderNotifier.GetTenantUserId(tenantId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the New Order Notifier can send new order notices."));

    public Result CanAssignOrder(ClaimsPrincipal user)
        => user.IsInAnyRole(StaffRoles.OrderAssigners)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Client Services Director or the tenant admin can assign orders."));

    public Task<Result> CanBeAssignedOrderAsync(UserId assigneeId)
        => grainFactory.GetUserDirectoryGrain(assigneeId.TenantId)
            .IsStaffUserAsync(assigneeId)
            .MapError(_ => (Error)RequestErrors.NewInvalidArg("Orders can only be assigned to the AMC's staff."));

    public Result CanAcknowledgeOrder(UserId userId, OrderState order)
        => order.AssignedTo is null || order.AssignedTo == (string)userId
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the staff member the order is assigned to can acknowledge it."));

    public Result CanRequestClientDocument(ClaimsPrincipal user, UserId userId, OrderState order)
        => order.AssignedTo == (string)userId || user.IsInAnyRole(StaffRoles.OrderAssigners)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the staff member the order is assigned to, the Client Services Director or the tenant admin can request a document."));

    public Result CanSendDocumentRequestNotice(UserId userId)
        => userId.TenantId is { } tenantId && (string)userId == AutomationUser.DocumentRequestNotifier.GetTenantUserId(tenantId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Document Request Notifier can send document request notices."));

    public Result CanProvideRequestedDocument(UserId userId, OrderDocumentRequest request)
        => request.RequestedFromUserId == (string)userId
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the client the document was requested from can provide it."));

    public Result CanSubmitDescriptionOfWork(ClaimsPrincipal user, UserId userId, OrderState order)
        => order.AssignedTo == (string)userId || user.IsInAnyRole(StaffRoles.OrderAssigners)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the staff member the order is assigned to, the Client Services Director or the tenant admin can submit the description of work."));

    public Result CanApproveDescriptionOfWork(UserId userId, OrderDescriptionOfWork descriptionOfWork)
        => descriptionOfWork.ApproverUserId == (string)userId
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the client who started the order can approve its description of work."));

    public Result CanSendDescriptionOfWorkSubmittedNotice(UserId userId)
        => IsAutomation(AutomationUser.DescriptionOfWorkApprovalNotifier, userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Description of Work Approval Notifier can send the notices of descriptions of work to approve."));

    public Result CanSendDescriptionOfWorkApprovedNotice(UserId userId)
        => IsAutomation(AutomationUser.DescriptionOfWorkApprovedNotifier, userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Description of Work Approved Notifier can send the notices of approved descriptions of work."));

    public Result CanAssignVendor(ClaimsPrincipal user, UserId userId, OrderState order)
        => order.AssignedTo == (string)userId || user.IsInAnyRole(StaffRoles.OrderAssigners)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the staff member the order is assigned to, the Client Services Director or the tenant admin can assign the order to a vendor."));

    public Task<Result> CanBeAssignedVendorAsync(VendorId vendorId)
        => grainFactory.GetAssignableVendorsGrain(vendorId.TenantId)
            .IsAssignableVendorAsync(vendorId)
            .MapError(_ => (Error)RequestErrors.NewInvalidArg("Orders can only be assigned to the tenant's assignable vendors."));

    public async Task<Result> AreVendorUsersAsync(VendorId vendorId, IReadOnlyCollection<string> userIds)
    {
        var vendorUsers = (await grainFactory.GetUserDirectoryGrain(vendorId.TenantId).GetUsersAsync())
            .Where(user => user.VendorId == (string)vendorId)
            .Select(user => user.UserId)
            .ToHashSet(StringComparer.Ordinal);
        return userIds.All(vendorUsers.Contains)
            ? Result.Success()
            : Result.Failure(RequestErrors.NewInvalidArg("The notice must go only to users of the vendor the order is assigned to."));
    }

    public Result CanSendVendorAssignedNotice(UserId userId)
        => IsAutomation(AutomationUser.VendorAssignmentNotifier, userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Vendor Assignment Notifier can send the notices of orders assigned to vendors."));

    public async Task<Result> CanDoVendorWorkAsync(UserId userId, OrderState order)
    {
        var user = (await grainFactory.GetUserDirectoryGrain(userId.TenantId).GetUsersAsync())
            .FirstOrDefault(entry => entry.UserId == (string)userId);
        return order.VendorAssignment is { } assignment && user?.VendorId == assignment.VendorId
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only users of the vendor the order is assigned to can work on it."));
    }

    public Result CanSendReportDeliveredNotice(UserId userId)
        => IsAutomation(AutomationUser.ReportDeliveredNotifier, userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Report Delivered Notifier can send the notices of delivered reports."));

    public Result CanShipOrder(ClaimsPrincipal user, UserId userId, OrderState order)
        => order.AssignedTo == (string)userId || user.IsInAnyRole(StaffRoles.OrderAssigners)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the staff member the order is assigned to, the Client Services Director or the tenant admin can ship the order."));

    public Result CanSendOrderShippedNotice(UserId userId)
        => IsAutomation(AutomationUser.OrderShippedNotifier, userId)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the Order Shipped Notifier can send the notices of shipped orders."));

    private static bool IsAutomation(AutomationUser account, UserId userId)
        => userId.TenantId is { } tenantId && (string)userId == account.GetTenantUserId(tenantId);

    private static bool IsOrderNumberAssigner(UserId userId)
        => userId.TenantId is { } tenantId && (string)userId == AutomationUser.OrderNumberAssigner.GetTenantUserId(tenantId);
}
