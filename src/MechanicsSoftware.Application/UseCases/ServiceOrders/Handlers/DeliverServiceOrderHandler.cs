using MechanicsSoftware.Application.Abstractions;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class DeliverServiceOrderHandler(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        order.Deliver();

        await db.SaveChangesAsync(cancellationToken);

        return ServiceOrderResponse.From(order);
    }
}
