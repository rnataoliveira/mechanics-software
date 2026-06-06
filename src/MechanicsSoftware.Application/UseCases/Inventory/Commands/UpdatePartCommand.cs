namespace MechanicsSoftware.Application.UseCases.Inventory.Commands;

public sealed record UpdatePartCommand(
    string Name,
    string? Description,
    int UnitPriceInCents
);
