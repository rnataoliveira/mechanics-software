using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class RejectServiceOrderUseCaseTests
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
    public async Task ExecuteAsync_ValidOrder_StatusBecomesCancelled()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderAwaitingApproval();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await new RejectServiceOrderUseCase(db).ExecuteAsync(order.Id);

        result.Status.Should().Be("CANCELLED");
        result.Budget!.Status.Should().Be("REJECTED");
    }

    [Fact]
    public async Task ExecuteAsync_WithAvailableParts_ReleasesReservations()
    {
        // EF Core InMemory cannot INSERT new OwnsMany entries (StockMovement) onto
        // an already-saved parent when SaveChanges is called a second time.
        // Use Moq so the domain logic runs without hitting that limitation.
        var part = Part.Create(Guid.NewGuid(), "FILTER-001", "Oil Filter", null, new Money(2000), 10);

        var orderWithPart = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        orderWithPart.StartDiagnosis();
        orderWithPart.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        part.Reserve(2, orderWithPart.Id);
        orderWithPart.AddPartItem(part.Id, part.Name, part.UnitPrice, 2, PartAvailability.Available);
        orderWithPart.GenerateBudget();
        orderWithPart.SendBudget();

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { orderWithPart });
        var mockParts = MockDbSetHelper.CreateMockDbSet(new List<Part> { part });
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await new RejectServiceOrderUseCase(db.Object).ExecuteAsync(orderWithPart.Id);

        part.AvailableQuantity.Should().Be(10); // reservation released: reserved 0, stock 10
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new RejectServiceOrderUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongStatus_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()); // RECEIVED, no budget
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new RejectServiceOrderUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
