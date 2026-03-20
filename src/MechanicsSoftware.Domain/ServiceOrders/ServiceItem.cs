using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public sealed class ServiceItem : Entity<Guid>
{
    public Guid ServiceOrderId { get; private set; }
    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money Total => UnitPrice.Multiply(Quantity);

    private ServiceItem() { }

    internal static ServiceItem Create(
        Guid serviceOrderId,
        Guid serviceId,
        string serviceName,
        Money unitPrice,
        int quantity)
    {
        if (serviceId == Guid.Empty)
            throw new DomainException("ServiceId is required.");

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new DomainException("Service name is required.");

        if (unitPrice is null)
            throw new DomainException("Unit price is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        return new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            ServiceId = serviceId,
            ServiceName = serviceName.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
