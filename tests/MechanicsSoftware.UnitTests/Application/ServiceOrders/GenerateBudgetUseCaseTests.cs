using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class GenerateBudgetUseCaseTests
{
    private static ServiceOrder BuildOrderInDiagnosis()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        return order;
    }

    [Fact]
    public async Task ExecuteAsync_WithServiceItems_ReturnsBudget()
    {
        // EF Core InMemory cannot INSERT a new OwnsOne (Budget) on an already-saved
        // parent. Use Moq so the domain logic runs without hitting that limitation.
        // Include() is a no-op on non-EF providers, so navigation props come from
        // the in-memory object graph.
        var order = BuildOrderInDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);

        var mockOrders = MockDbSetHelper.CreateMockDbSet([order]);
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new GenerateBudgetUseCase(db.Object).ExecuteAsync(order.Id);

        result.TotalInCents.Should().Be(5000);
        result.Status.Should().Be("PENDING");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new GenerateBudgetUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_NoServiceItems_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderInDiagnosis();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new GenerateBudgetUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*service item*");
    }

    [Fact]
    public async Task ExecuteAsync_BudgetAlreadyGenerated_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = BuildOrderInDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new GenerateBudgetUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already generated*");
    }
}
