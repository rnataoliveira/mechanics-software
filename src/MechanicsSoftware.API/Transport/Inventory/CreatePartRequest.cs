namespace MechanicsSoftware.API.Transport.Inventory;

public sealed record CreatePartRequest(
    string Code,
    string Name,
    string? Description,
    int UnitPriceInCents,
    int InitialStock
);
