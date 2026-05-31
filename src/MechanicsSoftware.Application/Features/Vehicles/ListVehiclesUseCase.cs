using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;

namespace MechanicsSoftware.Application.Features.Vehicles;

public sealed record ListVehiclesQuery(
    Guid? CustomerId = null,
    string? LicensePlate = null
);

public sealed class ListVehiclesUseCase(IAppDbContext db)
{
    public async Task<IReadOnlyList<VehicleResponse>> ExecuteAsync(
        ListVehiclesQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Vehicle> vehicles = db.Vehicles;

        if (query.CustomerId.HasValue)
            vehicles = vehicles.Where(v => v.CustomerId == query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.LicensePlate))
        {
            var plateVo = new LicensePlate(query.LicensePlate);
            vehicles = vehicles.Where(v => v.LicensePlate == plateVo);
        }

        return await vehicles
            .Select(v => VehicleResponse.From(v))
            .ToListAsync(cancellationToken);
    }
}
