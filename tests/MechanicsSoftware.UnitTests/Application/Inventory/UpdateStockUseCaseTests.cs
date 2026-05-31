using FluentAssertions;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Inventory;
using MechanicsSoftware.Application.UseCases.Inventory.Commands;
using MechanicsSoftware.Application.UseCases.Inventory.Handlers;
using MechanicsSoftware.Application.UseCases.Inventory.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class UpdateStockUseCaseTests
{
    private static Part BuildPart(Guid? id = null) =>
        Part.Create(id ?? Guid.NewGuid(), "OIL-001", "Engine Oil", null, new Money(2500), 5);

    private static Mock<IAppDbContext> BuildContext(Part? part)
    {
        var db = new Mock<IAppDbContext>();
        List<Part> list = part is null ? [] : [part];
        var mockParts = MockDbSetHelper.CreateMockDbSet(list);

        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ValidReplenishment_IncreasesStock()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var db = BuildContext(part);

        var result = await new UpdateStockHandler(db.Object).ExecuteAsync(partId, new UpdateStockCommand(10));

        result.StockQuantity.Should().Be(15);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var db = BuildContext(null);

        var act = async () => await new UpdateStockHandler(db.Object).ExecuteAsync(Guid.NewGuid(), new UpdateStockCommand(5));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ZeroQuantity_ThrowsDomainException()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var db = BuildContext(part);

        var act = async () => await new UpdateStockHandler(db.Object).ExecuteAsync(partId, new UpdateStockCommand(0));

        await act.Should().ThrowAsync<DomainException>().WithMessage("*greater than zero*");
    }
}
