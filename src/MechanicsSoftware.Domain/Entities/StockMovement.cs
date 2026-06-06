using MechanicsSoftware.Domain.Enums;

namespace MechanicsSoftware.Domain.Entities;

public sealed class StockMovement : Entity<Guid>
{
    public Guid PartId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public Guid? Reference { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    internal static StockMovement Create(
        Guid partId,
        StockMovementType type,
        int quantity,
        Guid? reference = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            PartId = partId,
            Type = type,
            Quantity = quantity,
            Reference = reference,
            CreatedAt = DateTime.UtcNow
        };
    }
}
