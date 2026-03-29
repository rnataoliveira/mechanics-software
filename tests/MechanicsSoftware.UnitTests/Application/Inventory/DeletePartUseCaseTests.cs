using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class DeletePartUseCaseTests
{
    private static Part BuildPart(Guid? id = null, int stock = 0) =>
        Part.Create(id ?? Guid.NewGuid(), "ENG-001", "Oil Filter", null, new Money(2500), stock);

    private static (Mock<IAppDbContext> db, Mock<Microsoft.EntityFrameworkCore.DbSet<Part>> mockParts)
        BuildContext(Part? part = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockParts = MockDbSetHelper.CreateMockDbSet(part is null ? [] : [part]);
        mockParts
            .Setup(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (db, mockParts);
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_RemovesPart()
    {
        var id = Guid.NewGuid();
        var part = BuildPart(id);
        var (db, mockParts) = BuildContext(part);

        var useCase = new DeletePartUseCase(db.Object);
        await useCase.ExecuteAsync(id);

        mockParts.Verify(m => m.Remove(part), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var (db, _) = BuildContext();

        var useCase = new DeletePartUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingReservations_ThrowsDomainException()
    {
        var id = Guid.NewGuid();
        var part = BuildPart(id, stock: 10);
        part.Reserve(3, Guid.NewGuid());
        var (db, _) = BuildContext(part);

        var useCase = new DeletePartUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(id);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*pending reservations*");
    }
}
