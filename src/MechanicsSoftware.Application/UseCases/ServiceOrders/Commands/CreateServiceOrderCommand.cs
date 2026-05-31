namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;

public sealed record CreateServiceOrderCommand(Guid CustomerId, Guid VehicleId);
