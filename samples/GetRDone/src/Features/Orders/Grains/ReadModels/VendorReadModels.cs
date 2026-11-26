using System.Net;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>Helpers for the read models only vendor users can read: the vendor's orders and the vendor order page.</summary>
internal static class VendorReadModels
{
    /// <summary>The vendor the signed-in user works for, or permission denied when they are not a vendor user.</summary>
    public static Result<string> GetCurrentVendorId(this Grain grain, UserDirectoryNames names, string? tenantId)
        => names.VendorIdOf(grain.GetCurrentTenantUserId(tenantId)) is { } vendorId
            ? Result.Success(vendorId)
            : Result.Failure<string>(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only vendor users can see the orders assigned to vendors."));
}
