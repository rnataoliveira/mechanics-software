using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;
using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class AddServiceItemHandler(IAppDbContext db)
{
    public async Task<ServiceItemResponse> ExecuteAsync(
        Guid serviceOrderId, AddServiceItemCommand command, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        var service = await db.Services.FindAsync([command.ServiceId], cancellationToken)
            ?? throw new NotFoundException(nameof(Service), command.ServiceId);

        var item = order.AddServiceItem(
            service.Id,
            service.Name,
            service.BasePrice,
            command.Quantity);

        await db.SaveChangesAsync(cancellationToken);

        return ServiceItemResponse.From(item);
    }
}
