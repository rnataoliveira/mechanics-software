using MechanicsSoftware.Application.Abstractions;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class GetServiceOrderHandler(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        return ServiceOrderResponse.From(order);
    }
}
