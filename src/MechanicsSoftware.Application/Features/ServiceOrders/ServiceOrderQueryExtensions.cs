using MechanicsSoftware.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

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
