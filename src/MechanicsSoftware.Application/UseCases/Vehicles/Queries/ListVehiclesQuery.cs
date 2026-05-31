namespace MechanicsSoftware.Application.UseCases.Vehicles.Queries;

public sealed record ListVehiclesQuery(
    Guid? CustomerId = null,
    string? LicensePlate = null
);
