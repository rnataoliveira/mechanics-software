using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Inventory;

public sealed class Part : Entity<Guid>
{
    private readonly List<StockMovement> _movements = [];

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }

    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    private Part() { }

    public static Part Create(
        Guid id,
        string code,
        string name,
        string? description,
        Money unitPrice,
        int initialStock = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Part code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Part name is required.");

        if (unitPrice is null)
            throw new DomainException("Unit price is required.");

        if (initialStock < 0)
            throw new DomainException("Initial stock cannot be negative.");

        var part = new Part
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            UnitPrice = unitPrice,
            StockQuantity = initialStock
        };

        if (initialStock > 0)
            part._movements.Add(StockMovement.Create(id, StockMovementType.Inbound, initialStock));

        return part;
    }

    public void Reserve(int quantity, Guid reference)
    {
        if (quantity <= 0)
            throw new DomainException("Reserve quantity must be greater than zero.");

        if (reference == Guid.Empty)
            throw new DomainException("Reference is required for reservation.");

        if (AvailableQuantity < quantity)
            throw new DomainException(
                $"Insufficient available stock. Available: {AvailableQuantity}, requested: {quantity}.");

        ReservedQuantity += quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Reservation, quantity, reference));
    }

    public void ConfirmUsage(int quantity, Guid reference)
    {
        if (quantity <= 0)
            throw new DomainException("Confirm usage quantity must be greater than zero.");

        if (reference == Guid.Empty)
            throw new DomainException("Reference is required for usage confirmation.");

        if (ReservedQuantity - quantity < 0)
            throw new DomainException(
                $"Cannot confirm usage: not enough reserved quantity. Reserved: {ReservedQuantity}, requested: {quantity}.");

        StockQuantity -= quantity;
        ReservedQuantity -= quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Outbound, quantity, reference));
    }

    public void Release(int quantity, Guid reference)
    {
        if (quantity <= 0)
            throw new DomainException("Release quantity must be greater than zero.");

        if (reference == Guid.Empty)
            throw new DomainException("Reference is required for release.");

        if (ReservedQuantity - quantity < 0)
            throw new DomainException(
                $"Cannot release more than reserved. Reserved: {ReservedQuantity}, requested: {quantity}.");

        ReservedQuantity -= quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Release, quantity, reference));
    }

    public void Replenish(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Replenish quantity must be greater than zero.");

        StockQuantity += quantity;
        _movements.Add(StockMovement.Create(Id, StockMovementType.Inbound, quantity));
    }
}
