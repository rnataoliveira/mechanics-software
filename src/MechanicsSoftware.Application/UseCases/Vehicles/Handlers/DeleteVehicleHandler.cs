using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Vehicles.Handlers;

public sealed class DeleteVehicleHandler(IAppDbContext db)
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), id);

        db.Vehicles.Remove(vehicle);
        await db.SaveChangesAsync(cancellationToken);
    }
}
