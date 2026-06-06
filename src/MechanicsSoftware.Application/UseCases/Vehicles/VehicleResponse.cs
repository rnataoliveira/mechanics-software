using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Vehicles;

public sealed record VehicleResponse(
    Guid Id,
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    Guid CustomerId
)
{
    public static VehicleResponse From(Vehicle v) =>
        new(v.Id, v.LicensePlate.Value, v.Make, v.Model, v.Year, v.CustomerId);
}
