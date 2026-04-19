using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class RejectServiceOrderUseCase(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        order.Reject();

        var availablePartItems = order.PartItems
            .Where(p => p.Availability == PartAvailability.Available);

        foreach (var partItem in availablePartItems)
        {
            var part = await db.Parts.FindAsync([partItem.PartId], cancellationToken)
                ?? throw new NotFoundException(nameof(Part), partItem.PartId);

            part.Release(partItem.Quantity, serviceOrderId);
        }

        await db.SaveChangesAsync(cancellationToken);

        return ServiceOrderResponse.From(order);
    }
}
