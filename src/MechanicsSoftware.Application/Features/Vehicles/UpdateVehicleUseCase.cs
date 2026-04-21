using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Application.Features.Vehicles;

public sealed record UpdateVehicleRequest(
    Guid Id,
    string Make,
    string Model,
    int Year
);

public sealed class UpdateVehicleUseCase(IAppDbContext db)
{
    public async Task<VehicleResponse> ExecuteAsync(
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await db.Vehicles
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.Id);

        vehicle.Update(request.Make, request.Model, request.Year);
        await db.SaveChangesAsync(cancellationToken);

        return VehicleResponse.From(vehicle);
    }
}
