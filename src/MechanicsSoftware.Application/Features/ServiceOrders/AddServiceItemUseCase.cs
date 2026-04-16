using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;
using Service = MechanicsSoftware.Domain.Services.Service;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class AddServiceItemUseCase(IAppDbContext db)
{
    public async Task<ServiceItemResponse> ExecuteAsync(
        Guid serviceOrderId, AddServiceItemRequest request, CancellationToken ct = default)
    {
        var order = await db.ServiceOrders
            .FirstOrDefaultAsync(o => o.Id == serviceOrderId, ct)
            ?? throw new NotFoundException(nameof(ServiceOrder), serviceOrderId);

        var service = await db.Services.FindAsync([request.ServiceId], ct)
            ?? throw new NotFoundException(nameof(Service), request.ServiceId);

        var item = order.AddServiceItem(
            service.Id,
            service.Name,
            service.BasePrice,
            request.Quantity);

        await db.SaveChangesAsync(ct);

        return ServiceItemResponse.From(item);
    }
}
