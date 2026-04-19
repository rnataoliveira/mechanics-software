using FluentAssertions;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class DeliverServiceOrderUseCaseTests
{
    private static ServiceOrder BuildOrderCompleted()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        order.SendBudget();
        order.Approve();
        order.Complete();
        return order;
    }

    [Fact]
    public async Task ExecuteAsync_CompletedOrder_StatusBecomesDelivered()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderCompleted();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await new DeliverServiceOrderUseCase(db).ExecuteAsync(order.Id);

        result.Status.Should().Be("DELIVERED");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new DeliverServiceOrderUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongStatus_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new DeliverServiceOrderUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
