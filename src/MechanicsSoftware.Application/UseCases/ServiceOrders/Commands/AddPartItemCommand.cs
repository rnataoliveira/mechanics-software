namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;

public sealed record AddPartItemCommand(Guid PartId, int Quantity);
