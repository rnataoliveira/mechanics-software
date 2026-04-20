using FluentAssertions;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class ApproveServiceOrderUseCaseTests
{
    private static ServiceOrder BuildOrderAwaitingApproval()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        order.SendBudget();
        return order;
    }

    [Fact]
    public async Task ExecuteAsync_ValidOrder_StatusBecomesInExecution()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderAwaitingApproval();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await new ApproveServiceOrderUseCase(db).ExecuteAsync(order.Id);

        result.Status.Should().Be("IN_EXECUTION");
        result.Budget!.Status.Should().Be("APPROVED");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new ApproveServiceOrderUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongStatus_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()); // RECEIVED, no budget
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new ApproveServiceOrderUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
