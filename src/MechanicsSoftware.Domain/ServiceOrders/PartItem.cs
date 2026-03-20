using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public enum PartAvailability
{
    Available,
    Unavailable
}

public sealed class PartItem : Entity<Guid>
{
    public Guid ServiceOrderId { get; private set; }
    public Guid PartId { get; private set; }
    public string PartName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public PartAvailability Availability { get; private set; }

    public Money Total => Availability == PartAvailability.Available
        ? UnitPrice.Multiply(Quantity)
        : new Money(0);

    private PartItem() { }

    internal static PartItem Create(
        Guid serviceOrderId,
        Guid partId,
        string partName,
        Money unitPrice,
        int quantity,
        PartAvailability availability)
    {
        if (partId == Guid.Empty)
            throw new DomainException("PartId is required.");

        if (string.IsNullOrWhiteSpace(partName))
            throw new DomainException("Part name is required.");

        if (unitPrice is null)
            throw new DomainException("Unit price is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        return new PartItem
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            PartId = partId,
            PartName = partName.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
            Availability = availability
        };
    }
}
