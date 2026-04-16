using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class CompleteServiceOrderUseCase(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken ct = default)
    {
        var order = await db.ServiceOrders
            .Include(o => o.ServiceItems)
            .Include(o => o.PartItems)
            .Include(o => o.Budget)
            .FirstOrDefaultAsync(o => o.Id == serviceOrderId, ct)
            ?? throw new NotFoundException(nameof(ServiceOrder), serviceOrderId);

        order.Complete();

        var availablePartItems = order.PartItems
            .Where(p => p.Availability == PartAvailability.Available);

        foreach (var partItem in availablePartItems)
        {
            var part = await db.Parts.FindAsync([partItem.PartId], ct)
                ?? throw new NotFoundException(nameof(Part), partItem.PartId);

            part.ConfirmUsage(partItem.Quantity, serviceOrderId);
        }

        await db.SaveChangesAsync(ct);

        return ServiceOrderResponse.From(order);
    }
}
