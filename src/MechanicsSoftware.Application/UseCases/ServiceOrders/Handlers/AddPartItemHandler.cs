using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Enums;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class AddPartItemHandler(IAppDbContext db)
{
    public async Task<AddPartItemResponse> ExecuteAsync(
        Guid serviceOrderId, AddPartItemCommand command, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        var part = await db.Parts.FindAsync([command.PartId], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), command.PartId);

        PartAvailability availability;
        string? warning = null;

        if (part.AvailableQuantity >= command.Quantity)
        {
            part.Reserve(command.Quantity, serviceOrderId);
            availability = PartAvailability.Available;
        }
        else
        {
            availability = PartAvailability.Unavailable;
            warning = $"Insufficient stock for part '{part.Name}'. " +
                      $"Available: {part.AvailableQuantity}, requested: {command.Quantity}. " +
                      $"Part added as UNAVAILABLE and excluded from budget.";
        }

        var item = order.AddPartItem(
            part.Id,
            part.Name,
            part.UnitPrice,
            command.Quantity,
            availability);

        await db.SaveChangesAsync(cancellationToken);

        return new AddPartItemResponse(
            item.Id,
            item.PartId,
            item.PartName,
            item.UnitPrice.Cents,
            item.Quantity,
            availability == PartAvailability.Available ? "AVAILABLE" : "UNAVAILABLE",
            item.Total.Cents,
            warning);
    }
}
