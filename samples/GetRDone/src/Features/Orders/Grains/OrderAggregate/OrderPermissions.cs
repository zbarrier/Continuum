using System.Net;

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
}
