using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Inventory;

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
