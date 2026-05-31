using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class AddServiceItemUseCase(IAppDbContext db)
{
    public async Task<ServiceItemResponse> ExecuteAsync(
        Guid serviceOrderId, AddServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        var service = await db.Services.FindAsync([request.ServiceId], cancellationToken)
            ?? throw new NotFoundException(nameof(Service), request.ServiceId);

        var item = order.AddServiceItem(
            service.Id,
            service.Name,
            service.BasePrice,
            request.Quantity);

        await db.SaveChangesAsync(cancellationToken);

        return ServiceItemResponse.From(item);
    }
}
