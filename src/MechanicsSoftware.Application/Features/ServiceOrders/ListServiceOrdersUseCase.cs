using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class ListServiceOrdersUseCase(IAppDbContext db)
{
    public async Task<IReadOnlyList<ServiceOrderSummaryResponse>> ExecuteAsync(
        ListServiceOrdersQuery query, CancellationToken ct = default)
    {
        var orders = db.ServiceOrders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var parsed = ParseStatus(query.Status);
            if (parsed.HasValue)
            {
                var statusVo = new ServiceOrderStatus(parsed.Value);
                orders = orders.Where(o => o.Status == statusVo);
            }
        }

        return await orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new ServiceOrderSummaryResponse(
                o.Id,
                o.CustomerId,
                o.VehicleId,
                o.Status.ToString()!,
                o.CreatedAt))
            .ToListAsync(ct);
    }

    private static ServiceOrderStatus.Status? ParseStatus(string value)
    {
        var normalized = value.Replace("_", "");
        return Enum.TryParse<ServiceOrderStatus.Status>(normalized, ignoreCase: true, out var result)
            ? result
            : null;
    }
}
