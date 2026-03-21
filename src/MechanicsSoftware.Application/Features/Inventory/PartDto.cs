namespace MechanicsSoftware.Application.Features.Inventory;

// ---- Inputs ----

public sealed record CreatePartInput(
    string Code,
    string Name,
    string? Description,
    int UnitPriceInCents,  // ex: 2500 = R$ 25,00
    int InitialStock
);

public sealed record UpdatePartInput(
    string Name,
    string? Description,
    int UnitPriceInCents
);

public sealed record UpdateStockInput(
    int Quantity
);

// ---- Output ----

public sealed record PartOutput(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int UnitPriceInCents,
    string UnitPriceFormatted,  // ex: "R$ 25,00"
    int StockQuantity,
    int ReservedQuantity,
    int AvailableQuantity
);
