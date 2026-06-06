using FluentAssertions;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class BudgetDecisionHandlerTests
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

    private static BudgetDecisionHandler BuildHandler(
        MechanicsSoftware.Infrastructure.Persistence.AppDbContext db) =>
        new(new ApproveServiceOrderHandler(db), new RejectServiceOrderHandler(db));

    [Fact]
    public async Task ExecuteAsync_ApproveDecision_TransitionsToInExecution()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderAwaitingApproval();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await BuildHandler(db).ExecuteAsync(order.Id, approve: true, default);

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

        var result = await BuildHandler(db).ExecuteAsync(order.Id, approve: false, default);

        result.Status.Should().Be("CANCELLED");
        result.Budget!.Status.Should().Be("REJECTED");
    }
}
