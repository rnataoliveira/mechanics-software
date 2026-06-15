using MechanicsSoftware.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class SendBudgetHandler(IAppDbContext db, IEmailNotifier emailNotifier, ILogger<SendBudgetHandler> logger)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        order.SendBudget();

        await db.SaveChangesAsync(cancellationToken);

        await emailNotifier.TrySendStatusEmailAsync(db, logger, order, cancellationToken);

        return ServiceOrderResponse.From(order);
    }
}
