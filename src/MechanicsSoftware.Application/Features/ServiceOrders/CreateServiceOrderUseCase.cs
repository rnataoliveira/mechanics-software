using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class CreateServiceOrderUseCase(IAppDbContext db)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        CreateServiceOrderRequest request, CancellationToken ct = default)
    {
        _ = await db.Customers.FindAsync([request.CustomerId], ct)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        _ = await db.Vehicles.FindAsync([request.VehicleId], ct)
            ?? throw new NotFoundException(nameof(Vehicle), request.VehicleId);

        var order = ServiceOrder.Create(Guid.NewGuid(), request.CustomerId, request.VehicleId);

        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync(ct);

        return ServiceOrderResponse.From(order);
    }
}
