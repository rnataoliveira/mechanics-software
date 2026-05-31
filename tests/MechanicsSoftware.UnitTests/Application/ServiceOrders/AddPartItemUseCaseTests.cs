using FluentAssertions;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.ServiceOrders;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class AddPartItemUseCaseTests
{
    private static Part BuildPart(int stock = 10) =>
        Part.Create(Guid.NewGuid(), "FILTER-001", "Oil Filter", null, new Money(2000), stock);

    [Fact]
    public async Task ExecuteAsync_SufficientStock_ReturnsAvailableItem()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        var part = BuildPart(10);

        var mockOrders = MockDbSetHelper.CreateMockDbSet([order]);
        var mockParts = MockDbSetHelper.CreateMockDbSet([part]);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new AddPartItemHandler(db.Object).ExecuteAsync(order.Id, new AddPartItemCommand(part.Id, 3));

        result.Availability.Should().Be("AVAILABLE");
        result.Quantity.Should().Be(3);
        result.Warning.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_InsufficientStock_ReturnsUnavailableItemWithWarning()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        var part = BuildPart(0);

        var mockOrders = MockDbSetHelper.CreateMockDbSet([order]);
        var mockParts = MockDbSetHelper.CreateMockDbSet([part]);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new AddPartItemHandler(db.Object).ExecuteAsync(order.Id, new AddPartItemCommand(part.Id, 5));

        result.Availability.Should().Be("UNAVAILABLE");
        result.Warning.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new AddPartItemHandler(db).ExecuteAsync(
            Guid.NewGuid(), new AddPartItemCommand(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();

        var mockOrders = MockDbSetHelper.CreateMockDbSet([order]);
        var mockParts = MockDbSetHelper.CreateMockDbSet(new List<Part>());
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>((Part?)null));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);

        var act = async () => await new AddPartItemHandler(db.Object).ExecuteAsync(
            order.Id, new AddPartItemCommand(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
