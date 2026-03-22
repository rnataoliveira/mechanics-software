namespace MechanicsSoftware.Application.Features.Vehicles;

public sealed record VehicleResponse(
    Guid Id,
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    Guid CustomerId
);
