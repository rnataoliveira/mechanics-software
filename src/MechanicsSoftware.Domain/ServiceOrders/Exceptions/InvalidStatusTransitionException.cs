using MechanicsSoftware.Domain.Shared;

namespace  MechanicsSoftware.Domain.ServiceOrders.Exceptions;

public class InvalidStatusTransitionException: DomainException
{
    public InvalidStatusTransitionException(ServiceOrderStatus currentStatus, ServiceOrderStatus attemptedStatus)
        : base($"Invalid transition from '{currentStatus.ToString()}' to '{attemptedStatus.ToString()}'.")
    {
        
    }
}