using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Web.Components.Client.OrderEntryTabs;

using ManagedCode.Orleans.Identity.Core.Constants;

using Microsoft.AspNetCore.Http.HttpResults;

namespace GetRDone.Web;

/// <summary>
///     Downloads the file of a submitted order's supporting document or report. Staff can download the documents and report of
///     their tenant's orders, a client the documents and shipped report of the orders they started, and a vendor's users the
///     report they delivered. The order's read model for the user's role (OrderDetails, ClientOrderDetails or VendorOrderDetails)
///     decides, so the endpoint only serves a file the user can see listed.
/// </summary>
/// <remarks>
///     Orleans Identity copies the signed-in user into the Orleans request context only for MVC actions and SignalR hubs
///     (Blazor), so this minimal API endpoint does it itself. Files are always sent as attachments with <c>nosniff</c>, so a
///     file a client uploaded, such as HTML or SVG, never runs in the app's origin.
/// </remarks>
internal static class OrderDocumentDownloads
{
    private const string ClientRole = "client";
    private const string VendorRole = "vendor";

    private enum Reader
    {
        Staff,
        Client,
        Vendor
    }

    /// <summary>The download URL of a document, <c>{TenantId}|{DocumentGuid}</c>, of an order, <c>{TenantId}|{OrderGuid}</c>.</summary>
    public static string Url(string orderId, string documentId)
    {
        TenantKey.Split(orderId, out _, out var orderGuid);
        TenantKey.Split(documentId, out _, out var documentGuid);
        return $"/orders/{orderGuid}/documents/{documentGuid}";
    }

    public static IEndpointConventionBuilder MapOrderDocumentDownloads(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/orders/{orderGuid:guid}/documents/{documentGuid:guid}", DownloadAsync).RequireAuthorization();

    private static async Task<Results<StatusCodeHttpResult, NotFound, FileStreamHttpResult>> DownloadAsync(
        Guid orderGuid,
        Guid documentGuid,
        HttpContext httpContext,
        IClusterClient clusterClient,
        SupportingDocumentStore store,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var user = httpContext.User;
        var tenantId = user.FindFirst("tenant_id")?.Value;
        Reader? reader = StaffRoles.Staff.Any(user.IsInRole) ? Reader.Staff
            : user.IsInRole(ClientRole) ? Reader.Client
            : user.IsInRole(VendorRole) ? Reader.Vendor
            : null;
        if (string.IsNullOrEmpty(tenantId) || reader is null)
        {
            return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
        }

        var orderId = TenantKey.Create(tenantId, orderGuid.ToString());
        var documentId = TenantKey.Create(tenantId, documentGuid.ToString());

        Result<string?> fileName;
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
        try
        {
            fileName = await FindFileNameAsync(clusterClient, reader.Value, orderId, documentId);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }

        if (fileName.IsFailure)
        {
            return TypedResults.StatusCode((int)fileName.Error.HttpStatusCode);
        }
        if (fileName.Value is null)
        {
            return TypedResults.NotFound();
        }

        if (await store.OpenReadAsync(SupportingDocumentStorage.BlobPath(orderId, documentId), cancellationToken) is not { } file)
        {
            loggerFactory.CreateLogger(typeof(OrderDocumentDownloads))
                .LogWarning("The file of document {DocumentId} of order {OrderId} is missing.", documentId, orderId);
            return TypedResults.NotFound();
        }

        httpContext.Response.Headers.XContentTypeOptions = "nosniff";
        httpContext.Response.Headers.CacheControl = "private, no-store";
        return TypedResults.Stream(file.Content, file.ContentType, fileName.Value);
    }

    /// <summary>The document's file name, or null when the order has no such document the reader can see.</summary>
    private static async Task<Result<string?>> FindFileNameAsync(
        IClusterClient clusterClient, Reader reader, string orderId, string documentId)
    {
        switch (reader)
        {
            case Reader.Staff:
                var details = await clusterClient.GetOrderDetailsGrain(orderId).GetOrderDetailsAsync();
                return details.Map(value => value.Report?.DocumentId == documentId
                    ? value.Report.FileName
                    : value.Documents.FirstOrDefault(document => document.DocumentId == documentId)?.FileName);
            case Reader.Vendor:
                var vendorDetails = await clusterClient.GetVendorOrderDetailsGrain(orderId).GetVendorOrderDetailsAsync();
                return vendorDetails.Map(value => value.Report?.DocumentId == documentId ? value.Report.FileName : null);
            default:
                var clientDetails = await clusterClient.GetClientOrderDetailsGrain(orderId).GetClientOrderDetailsAsync();
                return clientDetails.Map(value => value.Report?.DocumentId == documentId
                    ? value.Report.FileName
                    : value.Documents.FirstOrDefault(document => document.DocumentId == documentId)?.FileName);
        }
    }
}
