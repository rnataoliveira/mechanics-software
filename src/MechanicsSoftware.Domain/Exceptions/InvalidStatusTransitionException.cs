using MechanicsSoftware.Domain.ValueObjects;

namespace MechanicsSoftware.Domain.Exceptions;

public class InvalidStatusTransitionException : ConflictException
{
    public InvalidStatusTransitionException(ServiceOrderStatus currentStatus, ServiceOrderStatus attemptedStatus)
        : base($"Invalid transition from '{currentStatus}' to '{attemptedStatus}'.")
    {
    }
}
