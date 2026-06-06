namespace MechanicsSoftware.Application.UseCases.Vehicles.Commands;

public sealed record UpdateVehicleCommand(
    Guid Id,
    string Make,
    string Model,
    int Year
);
