using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

internal static class EmailExtensions
{
    internal static async Task TrySendStatusEmailAsync(
        this IEmailNotifier emailNotifier,
        IAppDbContext db,
        ILogger logger,
        ServiceOrder order,
        CancellationToken cancellationToken)
    {
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
    }
}
