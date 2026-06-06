using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class CreateServiceOrderUseCase(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        CreateServiceOrderRequest request, CancellationToken cancellationToken = default)
    {
        _ = await db.Customers.FindAsync([request.CustomerId], cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        _ = await db.Vehicles.FindAsync([request.VehicleId], cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var order = ServiceOrder.Create(Guid.NewGuid(), request.CustomerId, request.VehicleId);

        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceOrderResponse.From(order);
    }
}
