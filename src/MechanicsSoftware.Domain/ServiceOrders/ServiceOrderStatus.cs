using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public sealed class ServiceOrderStatus(ServiceOrderStatus.Status value) : ValueObject
{
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

    public Status Value => value;

    public ServiceOrderStatus TransitionTo(Status newStatus)
    {
        if (Value == newStatus)
            throw new InvalidStatusTransitionException(Value, newStatus, "Already in that state.");

        return (Value, newStatus) switch
        {
            (Status.Received, Status.InDiagnosis)           => new(newStatus),
            (Status.InDiagnosis, Status.AwaitingApproval)   => new(newStatus),
            (Status.AwaitingApproval, Status.InExecution)   => new(newStatus),
            (Status.AwaitingApproval, Status.Cancelled)     => new(newStatus),
            (Status.InExecution, Status.Completed)          => new(newStatus),
            (Status.Completed, Status.Delivered)            => new(newStatus),
            _ => throw new InvalidStatusTransitionException(Value, newStatus)
        };
    }

    public bool Is(Status status) => Value == status;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value switch
    {
        Status.Received         => "RECEIVED",
        Status.InDiagnosis      => "IN_DIAGNOSIS",
        Status.AwaitingApproval => "AWAITING_APPROVAL",
        Status.InExecution      => "IN_EXECUTION",
        Status.Completed        => "COMPLETED",
        Status.Delivered        => "DELIVERED",
        Status.Cancelled        => "CANCELLED",
        _                       => Value.ToString()
    };

    public static ServiceOrderStatus CreateReceived() => new(Status.Received);
}
