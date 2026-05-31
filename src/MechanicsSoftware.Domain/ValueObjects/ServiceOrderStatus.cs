using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Domain.ValueObjects;

public sealed class ServiceOrderStatus(ServiceOrderStatus.Status value) : ValueObject
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

    private static readonly HashSet<(Status From, Status To)> _validTransitions =
    [
        (Status.Received,         Status.InDiagnosis),
        (Status.InDiagnosis,      Status.AwaitingApproval),
        (Status.AwaitingApproval, Status.InExecution),
        (Status.AwaitingApproval, Status.Cancelled),
        (Status.InExecution,      Status.Completed),
        (Status.Completed,        Status.Delivered),
    ];

    public ServiceOrderStatus TransitionTo(Status newStatus)
    {
        if (!_validTransitions.Contains((value, newStatus)))
            throw new InvalidStatusTransitionException(this, new ServiceOrderStatus(newStatus));

        return new ServiceOrderStatus(newStatus);
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

    public bool Is(Status s) => Value == s;

    public static ServiceOrderStatus CreateReceived() => new(Status.Received);
}
