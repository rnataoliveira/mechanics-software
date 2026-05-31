namespace MechanicsSoftware.API.Transport.ServiceOrders;

public sealed record CreateServiceOrderRequest(Guid CustomerId, Guid VehicleId);
