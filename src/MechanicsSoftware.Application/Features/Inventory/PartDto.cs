using MechanicsSoftware.Domain.Inventory;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed record CreatePartRequest(
    string Code,
    string Name,
    string? Description,
    int UnitPriceInCents,
    int InitialStock
);

public sealed record UpdatePartRequest(
    string Name,
    string? Description,
    int UnitPriceInCents
);

public sealed record UpdateStockRequest(
    int Quantity
);

public sealed record PartOutput(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int UnitPriceInCents,
    string UnitPriceFormatted,
    int StockQuantity,
    int ReservedQuantity,
    int AvailableQuantity)
{
    public static PartOutput From(Part p) =>
        new(p.Id, p.Code, p.Name, p.Description,
            p.UnitPrice.Cents, p.UnitPrice.ToFormatted(),
            p.StockQuantity, p.ReservedQuantity, p.AvailableQuantity);
}
