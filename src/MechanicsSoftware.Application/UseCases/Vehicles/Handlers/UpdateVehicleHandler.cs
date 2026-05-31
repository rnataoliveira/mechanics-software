using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Vehicles.Commands;
using MechanicsSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Vehicles.Handlers;

public sealed class UpdateVehicleHandler(IAppDbContext db)
{
    public async Task<VehicleResponse> ExecuteAsync(
        UpdateVehicleCommand command,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), command.Id);

        vehicle.Update(command.Make, command.Model, command.Year);
        await db.SaveChangesAsync(cancellationToken);

        return VehicleResponse.From(vehicle);
    }
}
