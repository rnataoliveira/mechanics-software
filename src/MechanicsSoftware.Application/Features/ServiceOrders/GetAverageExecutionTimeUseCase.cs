using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class GetAverageExecutionTimeUseCase(IAppDbContext db)
{
    public async Task<AverageExecutionTimeResponse> ExecuteAsync(CancellationToken ct = default)
    {
        var completedStatus = new ServiceOrderStatus(ServiceOrderStatus.Status.Completed);
        var deliveredStatus = new ServiceOrderStatus(ServiceOrderStatus.Status.Delivered);

        var completedOrders = await db.ServiceOrders
            .Where(o => o.CompletedAt != null)
            .Where(o => o.Status == completedStatus || o.Status == deliveredStatus)
            .Select(o => new { o.CreatedAt, o.CompletedAt })
            .ToListAsync(ct);

        if (completedOrders.Count == 0)
            return new AverageExecutionTimeResponse(0, 0);

        var averageHours = completedOrders
            .Average(o => (o.CompletedAt!.Value - o.CreatedAt).TotalHours);

        return new AverageExecutionTimeResponse(
            Math.Round(averageHours, 2),
            completedOrders.Count);
    }
}
