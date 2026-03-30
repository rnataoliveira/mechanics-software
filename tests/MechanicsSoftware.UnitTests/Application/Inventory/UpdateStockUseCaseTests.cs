using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class UpdateStockUseCaseTests
{
    private static Part BuildPart(Guid? id = null, int stock = 5) =>
        Part.Create(id ?? Guid.NewGuid(), "ENG-001", "Oil Filter", null, new Money(2500), stock);

    private static Mock<IAppDbContext> BuildContext(Part? part = null)
    {
        var db = new Mock<IAppDbContext>();
        var list = part is null ? new List<Part>() : new List<Part> { part };
        var mockParts = MockDbSetHelper.CreateMockDbSet(list);
        mockParts
            .Setup(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<Part?>(part));
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ReplenishesStock()
    {
        var id = Guid.NewGuid();
        var part = BuildPart(id, stock: 5);
        var db = BuildContext(part);

        var useCase = new UpdateStockUseCase(db.Object);
        var result = await useCase.ExecuteAsync(id, new UpdateStockInput(10));

        result.StockQuantity.Should().Be(15);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();

        var useCase = new UpdateStockUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid(), new UpdateStockInput(5));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
