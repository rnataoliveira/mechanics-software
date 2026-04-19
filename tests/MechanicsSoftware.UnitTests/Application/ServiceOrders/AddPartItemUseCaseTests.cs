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

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { order });
        var mockParts = MockDbSetHelper.CreateMockDbSet(new List<Part> { part });
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(ValueTask.FromResult<Part?>(part));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new AddPartItemUseCase(db.Object).ExecuteAsync(order.Id, new AddPartItemRequest(part.Id, 3));

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

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { order });
        var mockParts = MockDbSetHelper.CreateMockDbSet(new List<Part> { part });
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(ValueTask.FromResult<Part?>(part));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new AddPartItemUseCase(db.Object).ExecuteAsync(order.Id, new AddPartItemRequest(part.Id, 5));

        result.Availability.Should().Be("UNAVAILABLE");
        result.Warning.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new AddPartItemUseCase(db).ExecuteAsync(
            Guid.NewGuid(), new AddPartItemRequest(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { order });
        var mockParts = MockDbSetHelper.CreateMockDbSet(new List<Part>());
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(ValueTask.FromResult<Part?>(null));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Parts).Returns(mockParts.Object);

        var act = async () => await new AddPartItemUseCase(db.Object).ExecuteAsync(
            order.Id, new AddPartItemRequest(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
