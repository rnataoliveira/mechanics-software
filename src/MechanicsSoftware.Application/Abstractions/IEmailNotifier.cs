namespace MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Domain.ValueObjects;

public interface IEmailNotifier
{
    Task SendStatusChangedAsync(
        string toEmail,
        string customerName,
        Guid serviceOrderId,
        ServiceOrderStatus.Status newStatus,
        CancellationToken cancellationToken = default);
}
