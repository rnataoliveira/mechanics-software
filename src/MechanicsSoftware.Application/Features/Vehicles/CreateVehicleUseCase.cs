using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Vehicles;

public sealed record CreateVehicleRequest(
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    Guid CustomerId
);

public sealed class CreateVehicleUseCase(IAppDbContext db)
{
    public async Task<VehicleResponse> ExecuteAsync(
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await db.Customers
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        var plate = new LicensePlate(request.LicensePlate);

        var plateExists = await db.Vehicles
            .AnyAsync(v => v.LicensePlate == plate, cancellationToken);

        if (plateExists)
            throw new DomainException($"License plate '{plate}' is already registered.");

        var vehicle = Vehicle.Create(
            Guid.NewGuid(), plate, request.Make, request.Model, request.Year, request.CustomerId);

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);

        return VehicleResponse.From(vehicle);
    }
}
