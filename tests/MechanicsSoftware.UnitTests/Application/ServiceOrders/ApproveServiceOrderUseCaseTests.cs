using FluentAssertions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.ServiceOrders;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

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

        var result = await new ApproveServiceOrderHandler(db, HandlerStubs.EmailNotifier(), HandlerStubs.Logger<ApproveServiceOrderHandler>()).ExecuteAsync(order.Id);

        result.Status.Should().Be("IN_EXECUTION");
        result.Budget!.Status.Should().Be("APPROVED");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new ApproveServiceOrderHandler(db, HandlerStubs.EmailNotifier(), HandlerStubs.Logger<ApproveServiceOrderHandler>()).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongStatus_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()); // RECEIVED, no budget
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new ApproveServiceOrderHandler(db, HandlerStubs.EmailNotifier(), HandlerStubs.Logger<ApproveServiceOrderHandler>()).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
