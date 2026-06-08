namespace MechanicsSoftware.Application.Abstractions;

public interface IEmailNotifier
{
    Task SendStatusChangedAsync(
        string toEmail,
        string customerName,
        Guid serviceOrderId,
        string newStatus,
        CancellationToken cancellationToken = default);
}