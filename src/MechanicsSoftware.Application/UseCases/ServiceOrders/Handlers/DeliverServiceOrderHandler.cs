using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class DeliverServiceOrderHandler(IAppDbContext db, IEmailNotifier emailNotifier, ILogger<DeliverServiceOrderHandler> logger)
{
    public async Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        order.Deliver();

        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var customer = await db.Customers.FindAsync([order.CustomerId], cancellationToken)
                ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

            await emailNotifier.SendStatusChangedAsync(
                toEmail: customer.Email.Value,
                customerName: customer.Name,
                serviceOrderId: order.Id,
                newStatus: order.Status.Value,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.FailedToSendStatusEmail(ex, order.Id);
        }

        return ServiceOrderResponse.From(order);
    }
}
