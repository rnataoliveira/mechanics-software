using FluentAssertions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class BudgetDecisionUseCaseTests
{
    private static ServiceOrder BuildOrderAwaitingApproval()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5_000), 1);
        order.GenerateBudget();
        order.SendBudget();
        return order;
    }

    private static BudgetDecisionUseCase BuildUseCase(
        MechanicsSoftware.Infrastructure.Persistence.AppDbContext db) =>
        new(new ApproveServiceOrderUseCase(db), new RejectServiceOrderUseCase(db));

    [Fact]
    public async Task ExecuteAsync_ApproveDecision_TransitionsToInExecution()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderAwaitingApproval();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await BuildUseCase(db).ExecuteAsync(
            order.Id, new BudgetDecisionRequest(BudgetDecision.Approve), default);

        result.Status.Should().Be("IN_EXECUTION");
        result.Budget!.Status.Should().Be("APPROVED");
    }

    [Fact]
    public async Task ExecuteAsync_RejectDecision_TransitionsToCancelled()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderAwaitingApproval();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await BuildUseCase(db).ExecuteAsync(
            order.Id, new BudgetDecisionRequest(BudgetDecision.Reject), default);

        result.Status.Should().Be("CANCELLED");
        result.Budget!.Status.Should().Be("REJECTED");
    }
}
