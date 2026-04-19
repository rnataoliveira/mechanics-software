using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ServiceOrders;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class ApproveServiceOrderUseCase(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        order.Approve();

        await db.SaveChangesAsync(cancellationToken);

        return ServiceOrderResponse.From(order);
    }
}
