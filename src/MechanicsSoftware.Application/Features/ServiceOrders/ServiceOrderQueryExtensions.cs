using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

internal static class ServiceOrderQueryExtensions
{
    internal static async Task<ServiceOrder> FindFullAsync(
        this IQueryable<ServiceOrder> query,
        Guid id,
        CancellationToken cancellationToken = default) =>
        await query
            .Include(o => o.ServiceItems)
            .Include(o => o.PartItems)
            .Include(o => o.Budget)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(ServiceOrder), id);
}
