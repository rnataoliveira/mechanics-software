using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Application.Features.Vehicles;

public sealed class GetVehicleUseCase(IAppDbContext db)
{
    public async Task<VehicleResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), id);

        return new VehicleResponse(vehicle.Id, vehicle.LicensePlate.Value, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.CustomerId);
    }
}
