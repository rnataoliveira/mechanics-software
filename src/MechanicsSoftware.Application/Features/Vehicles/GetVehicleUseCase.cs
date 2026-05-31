using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Vehicles;

public sealed class GetVehicleUseCase(IAppDbContext db)
{
    public async Task<VehicleResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), id);

        return VehicleResponse.From(vehicle);
    }
}
