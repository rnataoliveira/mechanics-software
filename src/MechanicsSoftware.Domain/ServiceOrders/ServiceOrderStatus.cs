using MechanicsSoftware.Domain.ServiceOrders.Exceptions;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public sealed class ServiceOrderStatus(ServiceOrderStatus.Status value): ValueObject
{
    public Status Value => value;
    public enum Status
    {
        Received,
        InDiagnosis, 
        AwaitingApproval,
        InExecution, 
        Completed, 
        Delivered, 
        Cancelled
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public ServiceOrderStatus TransitionTo(Status newStatus)
    {
        return (value, newStatus) switch
        {
            (Status.Received, Status.InDiagnosis) => new ServiceOrderStatus(Status.InDiagnosis),
            (Status.InDiagnosis, Status.AwaitingApproval) => new ServiceOrderStatus(Status.AwaitingApproval),
            (Status.AwaitingApproval, Status.InExecution) => new ServiceOrderStatus(Status.InExecution),
            (Status.AwaitingApproval, Status.Cancelled) => new ServiceOrderStatus(Status.Cancelled),
            (Status.InExecution, Status.Completed) => new ServiceOrderStatus(Status.Completed),
            (Status.Completed, Status.Delivered) => new ServiceOrderStatus(Status.Delivered),
            _ => throw new InvalidStatusTransitionException(this, new ServiceOrderStatus(newStatus))
        };
    }

    public override string ToString() => Value switch
    {
        Status.Received => "RECEIVED",
        Status.InDiagnosis => "IN_DIAGNOSIS",
        Status.AwaitingApproval => "AWAITING_APPROVAL",
        Status.InExecution => "IN_EXECUTION",
        Status.Completed => "COMPLETED",
        Status.Delivered => "DELIVERED",
        Status.Cancelled => "CANCELLED",
        _ => Value.ToString()
    };

    public static ServiceOrderStatus CreateReceived() => new(Status.Received);

}