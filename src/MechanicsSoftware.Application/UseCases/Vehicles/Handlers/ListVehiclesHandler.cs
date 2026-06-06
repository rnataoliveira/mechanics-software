using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.Vehicles.Queries;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Vehicles.Handlers;

public sealed class ListVehiclesHandler(IAppDbContext db)
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
