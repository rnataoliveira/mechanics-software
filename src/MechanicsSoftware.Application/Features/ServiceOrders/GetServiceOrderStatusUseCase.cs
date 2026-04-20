using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class GetServiceOrderStatusUseCase(IAppDbContext db)
{
    public async Task<ServiceOrderStatusResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders
            .FirstOrDefaultAsync(o => o.Id == serviceOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceOrder), serviceOrderId);

        return new ServiceOrderStatusResponse(
            order.Id,
            order.Status.ToString(),
            order.CreatedAt,
            order.DeliveredAt);
    }
}
