using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public class InvalidStatusTransitionException: DomainException
{
    public InvalidStatusTransitionException(ServiceOrderStatus currentStatus, ServiceOrderStatus attemptedStatus)
        : base($"Invalid transition from '{currentStatus}' to '{attemptedStatus}'.")
    {

    }
}