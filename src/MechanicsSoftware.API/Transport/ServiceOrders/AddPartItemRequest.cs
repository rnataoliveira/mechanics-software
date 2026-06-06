namespace MechanicsSoftware.API.Transport.ServiceOrders;

public sealed record AddPartItemRequest(Guid PartId, int Quantity);
