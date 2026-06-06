namespace MechanicsSoftware.Application.UseCases.Inventory.Commands;

public sealed record CreatePartCommand(
    string Code,
    string Name,
    string? Description,
    int UnitPriceInCents,
    int InitialStock
);
