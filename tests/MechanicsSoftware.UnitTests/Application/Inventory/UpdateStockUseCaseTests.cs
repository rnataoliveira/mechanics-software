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
    private static Part BuildPart(Guid? id = null) =>
        Part.Create(id ?? Guid.NewGuid(), "OIL-001", "Engine Oil", null, new Money(2500), 5);

    private static Mock<IAppDbContext> BuildContext(Part? part)
    {
        var db = new Mock<IAppDbContext>();
        var list = part is null ? new List<Part>() : new List<Part> { part };
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

        var result = await new UpdateStockUseCase(db.Object).ExecuteAsync(partId, new UpdateStockRequest(10));

        result.StockQuantity.Should().Be(15);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var db = BuildContext(null);

        var act = async () => await new UpdateStockUseCase(db.Object).ExecuteAsync(Guid.NewGuid(), new UpdateStockRequest(5));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ZeroQuantity_ThrowsDomainException()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var db = BuildContext(part);

        var act = async () => await new UpdateStockUseCase(db.Object).ExecuteAsync(partId, new UpdateStockRequest(0));

        await act.Should().ThrowAsync<DomainException>().WithMessage("*greater than zero*");
    }
}
