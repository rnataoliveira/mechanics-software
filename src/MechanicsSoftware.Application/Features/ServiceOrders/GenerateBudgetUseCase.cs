using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ServiceOrders;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.ServiceOrders;

public sealed class GenerateBudgetUseCase(IAppDbContext db)
{
    public async Task<BudgetResponse> ExecuteAsync(
        Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        var order = await db.ServiceOrders.FindFullAsync(serviceOrderId, cancellationToken);

        var budget = order.GenerateBudget();

        await db.SaveChangesAsync(cancellationToken);

        return BudgetResponse.From(budget);
    }
}
