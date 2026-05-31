using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;

namespace MechanicsSoftware.Domain.Entities;

public sealed class Budget : Entity<Guid>
{
    public Guid ServiceOrderId { get; private set; }
    public Money Total { get; private set; } = null!;
    public BudgetStatus Status { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private Budget() { }

    internal static Budget Create(Guid serviceOrderId, Money total)
    {
        if (total is null)
            throw new DomainException("Budget total is required.");

        return new Budget
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            Total = total,
            Status = BudgetStatus.CreatePending(),
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void Approve()
    {
        Status = Status.TransitionTo(BudgetStatus.Status.Approved);
    }

    internal void Reject()
    {
        Status = Status.TransitionTo(BudgetStatus.Status.Rejected);
    }
}
