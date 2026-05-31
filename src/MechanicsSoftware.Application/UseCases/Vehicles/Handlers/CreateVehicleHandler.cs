using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Vehicles.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Vehicles.Handlers;

public sealed class CreateVehicleHandler(IAppDbContext db)
{
    public async Task<VehicleResponse> ExecuteAsync(
        CreateVehicleCommand command,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await db.Customers
            .AnyAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (!customerExists)
            throw new NotFoundException(nameof(Customer), command.CustomerId);

        var plate = new LicensePlate(command.LicensePlate);

        var plateExists = await db.Vehicles
            .AnyAsync(v => v.LicensePlate == plate, cancellationToken);

        if (plateExists)
            throw new DomainException($"License plate '{plate}' is already registered.");

        var vehicle = Vehicle.Create(
            Guid.NewGuid(), plate, command.Make, command.Model, command.Year, command.CustomerId);

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);

        return VehicleResponse.From(vehicle);
    }
}
