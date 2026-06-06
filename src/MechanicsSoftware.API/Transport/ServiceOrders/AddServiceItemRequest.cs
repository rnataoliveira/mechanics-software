namespace MechanicsSoftware.API.Transport.ServiceOrders;

public sealed record AddServiceItemRequest(Guid ServiceId, int Quantity);
