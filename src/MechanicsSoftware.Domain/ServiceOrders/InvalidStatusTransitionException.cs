using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public class InvalidStatusTransitionException : ConflictException
{
    public InvalidStatusTransitionException(ServiceOrderStatus currentStatus, ServiceOrderStatus attemptedStatus)
        : base($"Invalid transition from '{currentStatus}' to '{attemptedStatus}'.")
    {
    }
}