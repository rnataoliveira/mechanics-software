using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class AddPartItemUseCase(IAppDbContext db)
{
    public async Task<AddPartItemResponse> ExecuteAsync(
        Guid serviceOrderId, AddPartItemRequest request, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        var part = await db.Parts.FindAsync([request.PartId], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), request.PartId);

        PartAvailability availability;
        string? warning = null;

        if (part.AvailableQuantity >= request.Quantity)
        {
            part.Reserve(request.Quantity, serviceOrderId);
            availability = PartAvailability.Available;
        }
        else
        {
            availability = PartAvailability.Unavailable;
            warning = $"Insufficient stock for part '{part.Name}'. " +
                      $"Available: {part.AvailableQuantity}, requested: {request.Quantity}. " +
                      $"Part added as UNAVAILABLE and excluded from budget.";
        }

        var item = order.AddPartItem(
            part.Id,
            part.Name,
            part.UnitPrice,
            request.Quantity,
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
