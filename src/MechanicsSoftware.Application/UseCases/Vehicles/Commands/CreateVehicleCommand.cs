namespace MechanicsSoftware.Application.UseCases.Vehicles.Commands;

public sealed record CreateVehicleCommand(
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    Guid CustomerId
);
