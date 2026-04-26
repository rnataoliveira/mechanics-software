using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public sealed class ServiceOrder : Entity<Guid>
{
    private readonly List<ServiceItem> _serviceItems = [];
    private readonly List<PartItem> _partItems = [];

    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ServiceOrderStatus Status { get; private set; } = null!;
    public Budget? Budget { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public IReadOnlyCollection<ServiceItem> ServiceItems => _serviceItems.AsReadOnly();
    public IReadOnlyCollection<PartItem> PartItems => _partItems.AsReadOnly();

    private ServiceOrder() { }

    public static ServiceOrder Create(Guid id, Guid customerId, Guid vehicleId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");

        if (vehicleId == Guid.Empty)
            throw new DomainException("VehicleId is required.");

        return new ServiceOrder
        {
            Id = id,
            CustomerId = customerId,
            VehicleId = vehicleId,
            Status = ServiceOrderStatus.CreateReceived(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void StartDiagnosis()
    {
        Status = Status.TransitionTo(ServiceOrderStatus.Status.InDiagnosis);
    }

    public void SendBudget()
    {
        if (Budget is null)
            throw new DomainException("A budget must be generated before it can be sent.");

        Status = Status.TransitionTo(ServiceOrderStatus.Status.AwaitingApproval);
    }

    public void Approve()
    {
        if (Budget is null)
            throw new DomainException("Cannot approve an order without a budget.");

        Status = Status.TransitionTo(ServiceOrderStatus.Status.InExecution);
        Budget.Approve();
    }

    public void Reject()
    {
        if (Budget is null)
            throw new DomainException("Cannot reject an order without a budget.");

        Status = Status.TransitionTo(ServiceOrderStatus.Status.Cancelled);
        Budget.Reject();
    }

    public void StartExecution()
    {
        if (!Status.Is(ServiceOrderStatus.Status.InExecution))
            throw new DomainException(
                $"Order must be IN_EXECUTION to start work. Current status: {Status}.");
    }

    public void Complete()
    {
        Status = Status.TransitionTo(ServiceOrderStatus.Status.Completed);
        CompletedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        Status = Status.TransitionTo(ServiceOrderStatus.Status.Delivered);
        DeliveredAt = DateTime.UtcNow;
    }

    public ServiceItem AddServiceItem(
        Guid serviceId,
        string serviceName,
        Money unitPrice,
        int quantity)
    {
        EnsureInDiagnosis();

        var item = ServiceItem.Create(Id, serviceId, serviceName, unitPrice, quantity);
        _serviceItems.Add(item);
        return item;
    }

    public PartItem AddPartItem(
        Guid partId,
        string partName,
        Money unitPrice,
        int quantity,
        PartAvailability availability)
    {
        EnsureInDiagnosis();

        var item = PartItem.Create(Id, partId, partName, unitPrice, quantity, availability);
        _partItems.Add(item);
        return item;
    }

    public Budget GenerateBudget()
    {
        if (Budget is not null)
            throw new DomainException("Budget already generated. Cannot overwrite an existing budget.");

        EnsureInDiagnosis();

        var serviceItems = _serviceItems;
        if (serviceItems.Count == 0)
            throw new DomainException(
                "Cannot generate a budget: at least one service item is required.");

        var servicesTotal = serviceItems
            .Aggregate(new Money(0), (acc, item) => acc.Add(item.Total));

        var partsTotal = _partItems
            .Where(p => p.Availability == PartAvailability.Available)
            .Aggregate(new Money(0), (acc, item) => acc.Add(item.Total));

        var total = servicesTotal.Add(partsTotal);

        Budget = ServiceOrders.Budget.Create(Id, total);
        return Budget;
    }

    private void EnsureInDiagnosis()
    {
        if (!Status.Is(ServiceOrderStatus.Status.InDiagnosis))
            throw new DomainException(
                $"Items can only be added or a budget generated when the order is IN_DIAGNOSIS. " +
                $"Current status: {Status}.");
    }
}
