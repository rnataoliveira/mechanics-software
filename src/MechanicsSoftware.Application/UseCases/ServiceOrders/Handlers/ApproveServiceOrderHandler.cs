using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class ApproveServiceOrderHandler(IAppDbContext db, IEmailNotifier emailNotifier, ILogger<StartExecutionHandler> logger)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        order.Approve();

        var costumer = await db.Customers.FindAsync([order.CustomerId], cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await emailNotifier.SendStatusChangedAsync(
                toEmail: costumer.Email.Value,
                customerName: costumer.Name,
                serviceOrderId: order.Id,
                newStatus: order.Status.Value,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send status e-mail for ServiceOrder {ServiceOrderId}.", order.Id);
        }

        return ServiceOrderResponse.From(order);
    }
}
