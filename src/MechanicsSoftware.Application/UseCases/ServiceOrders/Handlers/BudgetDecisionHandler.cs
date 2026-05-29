using MechanicsSoftware.Application.Abstractions;

namespace MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;

public sealed class BudgetDecisionHandler(
    ApproveServiceOrderHandler approveHandler,
    RejectServiceOrderHandler rejectHandler)
{
    public Task<ServiceOrderResponse> ExecuteAsync(
        Guid serviceOrderId, bool approve, CancellationToken cancellationToken) =>
        approve
            ? approveHandler.ExecuteAsync(serviceOrderId, cancellationToken)
            : rejectHandler.ExecuteAsync(serviceOrderId, cancellationToken);
}
