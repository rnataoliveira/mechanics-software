namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;

public sealed record AddServiceItemCommand(Guid ServiceId, int Quantity);
