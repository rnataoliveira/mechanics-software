using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders {
    public sealed class BudgetStatus(BudgetStatus.Status value) : ValueObject
    {
        public enum Status
        {
            Pending,
            Approved,
            Rejected
        }

        public Status Value => value;
        
        public BudgetStatus TransitionTo(Status newStatus)
        {
            if (Value == newStatus)
                throw new DomainException($"Cannot transition from '{this}' to '{new BudgetStatus(newStatus)}': already in that state.");

            return (Value, newStatus) switch
            {
                (Status.Pending, Status.Approved) => new BudgetStatus(Status.Approved),
                (Status.Pending, Status.Rejected) => new BudgetStatus(Status.Rejected),
                (Status.Approved, _) => throw new DomainException("Cannot transition from 'APPROVED' state: it is terminal."),
                (Status.Rejected, _) => throw new DomainException("Cannot transition from 'REJECTED' state: it is terminal."),
                _ => throw new DomainException($"Invalid transition from '{this}' to '{new BudgetStatus(newStatus)}'.")
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value switch
        {
            Status.Pending => "PENDING",
            Status.Approved => "APPROVED",
            Status.Rejected => "REJECTED",
            _ => Value.ToString()
        };
        
        public static BudgetStatus CreatePending() => new(Status.Pending);
    }
}