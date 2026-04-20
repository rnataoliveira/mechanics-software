using FluentAssertions;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class SendBudgetUseCaseTests
{
    private static ServiceOrder BuildOrderWithBudget()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        return order;
    }

    [Fact]
    public async Task ExecuteAsync_OrderWithBudget_StatusBecomesAwaitingApproval()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderWithBudget();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await new SendBudgetUseCase(db).ExecuteAsync(order.Id);

        result.Status.Should().Be("AWAITING_APPROVAL");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new SendBudgetUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_NoBudget_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new SendBudgetUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
