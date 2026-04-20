using FluentAssertions;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class StartExecutionUseCaseTests
{
    private static ServiceOrder BuildOrderInExecution()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        order.SendBudget();
        order.Approve();
        return order;
    }

    [Fact]
    public async Task ExecuteAsync_OrderInExecution_ReturnsResponse()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderInExecution();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await new StartExecutionUseCase(db).ExecuteAsync(order.Id);

        result.Status.Should().Be("IN_EXECUTION");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new StartExecutionUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongStatus_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new StartExecutionUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
