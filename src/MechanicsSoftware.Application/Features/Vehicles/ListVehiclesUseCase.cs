using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;

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
        IQueryable<Domain.Vehicles.Vehicle> vehicles = db.Vehicles;

        if (query.CustomerId.HasValue)
            vehicles = vehicles.Where(v => v.CustomerId == query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.LicensePlate))
        {
            var plateVo = new Domain.Vehicles.LicensePlate(query.LicensePlate);
            vehicles = vehicles.Where(v => v.LicensePlate == plateVo);
        }

        return await vehicles
            .Select(v => new VehicleResponse(v.Id, v.LicensePlate.Value, v.Make, v.Model, v.Year, v.CustomerId))
            .ToListAsync(cancellationToken);
    }
}
