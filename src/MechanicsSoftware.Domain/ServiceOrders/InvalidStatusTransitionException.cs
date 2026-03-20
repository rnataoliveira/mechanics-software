using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.ServiceOrders;

public sealed class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(
        ServiceOrderStatus.Status from,
        ServiceOrderStatus.Status to,
        string? reason = null)
        : base(BuildMessage(from, to, reason))
    {
    }

    private static string BuildMessage(
        ServiceOrderStatus.Status from,
        ServiceOrderStatus.Status to,
        string? reason)
    {
        var msg = $"Invalid status transition: '{new ServiceOrderStatus(from)}' → '{new ServiceOrderStatus(to)}'.";
        return reason is null ? msg : $"{msg} {reason}";
    }
}
