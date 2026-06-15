using Microsoft.Extensions.Logging;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

internal static partial class HandlerLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to send status e-mail for ServiceOrder {ServiceOrderId}.")]
    public static partial void FailedToSendStatusEmail(
        this ILogger logger, Exception ex, Guid serviceOrderId);
}
