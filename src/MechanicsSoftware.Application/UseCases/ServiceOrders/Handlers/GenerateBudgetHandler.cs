using MechanicsSoftware.Application.Abstractions;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class GenerateBudgetHandler(IAppDbContext db)
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
