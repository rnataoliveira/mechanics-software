using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class GenerateBudgetUseCase(IAppDbContext db)
{
    public async Task<BudgetResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken ct = default)
    {
        var order = await db.ServiceOrders
            .Include(o => o.ServiceItems)
            .Include(o => o.PartItems)
            .Include(o => o.Budget)
            .FirstOrDefaultAsync(o => o.Id == serviceOrderId, ct)
            ?? throw new NotFoundException(nameof(ServiceOrder), serviceOrderId);

        var budget = order.GenerateBudget();

        await db.SaveChangesAsync(ct);

        return BudgetResponse.From(budget);
    }
}
