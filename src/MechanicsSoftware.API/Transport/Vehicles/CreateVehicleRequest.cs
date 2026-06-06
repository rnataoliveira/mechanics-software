namespace MechanicsSoftware.API.Transport.Vehicles;

public sealed record CreateVehicleRequest(
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    Guid CustomerId
);
